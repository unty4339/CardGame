using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CardBattle.Core.Deck;
using CardBattle.Core.Field;
using CardBattle.Core.Partner;
using CardBattle.Core.Player;
using CardBattle.Managers;
using UnityEngine;

namespace CardBattle.UI
{
    /// <summary>
    /// PlayerManagerのイベントを監視し、カードの生成や移動アニメーションを指揮する。UIとデータの橋渡しについて責任を持つ
    /// </summary>
    public class GameVisualManager : MonoBehaviour
    {
        public static GameVisualManager Instance { get; private set; }

        // プレハブ・配置先
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private Transform deckTransformPlayer0;
        [SerializeField] private Transform deckTransformPlayer1;
        [SerializeField] private HandVisualizer handVisualizerPlayer0;
        [SerializeField] private HandVisualizer handVisualizerPlayer1;
        [SerializeField] private FieldVisualizer fieldVisualizerPlayer0;
        [SerializeField] private FieldVisualizer fieldVisualizerPlayer1;
        [SerializeField] private UnitView unitPrefab;
        [SerializeField] private float drawAnimationDuration = 0.3f;
        [SerializeField] private PlayerInfoView player0InfoView;
        [SerializeField] private PlayerInfoView player1InfoView;
        [SerializeField] private Transform partnerZoneAnchorPlayer0;
        [SerializeField] private Transform partnerZoneAnchorPlayer1;
        [SerializeField] private PartnerCardView partnerCardViewPrefab;

        private PartnerCardView _partnerCardViewPlayer0;
        private PartnerCardView _partnerCardViewPlayer1;

        // Addressables でロードするビデオプレハブ
        private const string BombVideoAddress = "Assets/Prefabs/BombVideo.prefab";
        private const string AttackVideoAddress = "Assets/Prefabs/AttackVideo.prefab";
        private GameObject _bombVideoPrefab;
        private GameObject _attackVideoPrefab;

        private void Awake()
        {
            // シングルトン
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // PlayerManager のイベント購読とビデオプレハブのロード
            var pm = PlayerManager.Instance;
            if (pm != null)
            {
                pm.OnCardDrawn += PlayDrawAnimation;
                pm.OnUnitSummoned += OnUnitSummoned;
                pm.OnSpellPlayed += OnSpellPlayed;
                pm.OnPlayerDataChanged += OnPlayerDataChanged;
                pm.OnUnitHpChanged += OnUnitHpChanged;
                pm.OnUnitDestroyed += OnUnitDestroyed;
            }
            var partnerManager = PartnerManager.Instance;
            if (partnerManager != null)
            {
                partnerManager.OnPartnerSummoned += OnPartnerSummoned;
                partnerManager.OnPartnerReturnedToZone += OnPartnerReturnedToZone;
            }
            StartCoroutine(LoadVideoPrefabs());
        }

        /// <summary>BombVideo / AttackVideo を Addressables からロードする。</summary>
        private IEnumerator LoadVideoPrefabs()
        {
            var am = AddressableManager.Instance;
            if (am == null) yield break;

            var bombTask = am.LoadAssetAsync<GameObject>(BombVideoAddress);
            yield return new WaitUntil(() => bombTask.IsCompleted);
            if (bombTask.Status == TaskStatus.RanToCompletion)
                _bombVideoPrefab = bombTask.Result;
            else
                Debug.LogWarning("[GameVisualManager] BombVideo prefab load failed: " + bombTask.Exception?.Message);

            var attackTask = am.LoadAssetAsync<GameObject>(AttackVideoAddress);
            yield return new WaitUntil(() => attackTask.IsCompleted);
            if (attackTask.Status == TaskStatus.RanToCompletion)
                _attackVideoPrefab = attackTask.Result;
            else
                Debug.LogWarning("[GameVisualManager] AttackVideo prefab load failed: " + attackTask.Exception?.Message);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            var pm = PlayerManager.Instance;
            if (pm != null)
            {
                pm.OnCardDrawn -= PlayDrawAnimation;
                pm.OnUnitSummoned -= OnUnitSummoned;
                pm.OnSpellPlayed -= OnSpellPlayed;
                pm.OnPlayerDataChanged -= OnPlayerDataChanged;
                pm.OnUnitHpChanged -= OnUnitHpChanged;
                pm.OnUnitDestroyed -= OnUnitDestroyed;
            }
            var partnerManager = PartnerManager.Instance;
            if (partnerManager != null)
            {
                partnerManager.OnPartnerSummoned -= OnPartnerSummoned;
                partnerManager.OnPartnerReturnedToZone -= OnPartnerReturnedToZone;
            }
        }

        private void OnPlayerDataChanged(int playerId)
        {
            RefreshPlayerInfoView(playerId);
            RefreshPartnerZoneView(playerId);
        }

        /// <summary>
        /// 攻撃演出を再生し、完了後に onComplete を呼ぶ。プレイヤー・AI 共通。
        /// </summary>
        public void PlayAttackAndResolve(Unit attacker, object target, Action onComplete)
        {
            if (onComplete == null) return;
            StartCoroutine(PlayAttackAndResolveCoroutine(attacker, target, onComplete));
        }


         private Vector2 attackTargetPositionWhenAttackingOpponentPlayer = new Vector2(0, 200);
         private Vector2 attackTargetPositionWhenAttackingSelfPlayer = new Vector2(0, -200);

        /// <summary>攻撃対象の突進先・ビデオ再生位置を解決し、攻撃モーション完了後に onComplete を呼ぶ。</summary>
        private IEnumerator PlayAttackAndResolveCoroutine(Unit attacker, object target, Action onComplete)
        {
            var attackerView = fieldVisualizerPlayer0?.GetViewByUnit(attacker) ?? fieldVisualizerPlayer1?.GetViewByUnit(attacker);
            Debug.Log("attackerView: " + attackerView);
            var uiParent = VideoEffectManager.Instance != null ? VideoEffectManager.Instance.uiParent : null;

            Vector3 targetWorldPos = default;
            Vector2 effectPosition = default;
            bool hasTarget = false;

            // 対象がユニット: その UnitView の位置を突進先・ビデオ位置にする
            if (target is Unit targetUnit)
            {
                var targetView = fieldVisualizerPlayer0?.GetViewByUnit(targetUnit) ?? fieldVisualizerPlayer1?.GetViewByUnit(targetUnit);
                if (targetView != null)
                {
                    targetWorldPos = targetView.transform.position;
                    if (uiParent != null)
                    {
                        var localInParent = uiParent.InverseTransformPoint(targetWorldPos);
                        effectPosition = new Vector2(localInParent.x, localInParent.y);
                    }
                    hasTarget = true;
                }
            }
            // 対象がプレイヤー: 定数座標を使用（自分→相手 / 相手→自分で別）
            else if (target is int targetPlayerId && uiParent != null)
            {
                var localPos = attacker?.OwnerPlayerId == 0
                    ? attackTargetPositionWhenAttackingOpponentPlayer
                    : attackTargetPositionWhenAttackingSelfPlayer;
                targetWorldPos = uiParent.TransformPoint(new Vector3(localPos.x, localPos.y, 0f));
                effectPosition = localPos;
                hasTarget = true;
            }

            if (attackerView != null && hasTarget)
            {
                var prefab = _attackVideoPrefab;
                yield return attackerView.PlayAttackAnimationCoroutine(
                    targetWorldPos,
                    onReachedTarget: () =>
                    {
                        if (VideoEffectManager.Instance != null && prefab != null)
                            VideoEffectManager.Instance.PlayEffect(prefab, effectPosition, 2f, 0.3f);
                    },
                    onComplete: null);
            }
            else if (attackerView == null || !hasTarget)
            {
                yield return new WaitForSeconds(0.3f);
            }

            onComplete();
        }

        /// <summary>ユニットの HP 変更時に表示を再同期する。</summary>
        private void OnUnitHpChanged(Unit unit)
        {
            if (unit == null) return;
            var view = fieldVisualizerPlayer0?.GetViewByUnit(unit) ?? fieldVisualizerPlayer1?.GetViewByUnit(unit);
            view?.RefreshFromUnit();
        }

        /// <summary>ユニット破壊時に破壊演出コルーチンを開始する。</summary>
        private void OnUnitDestroyed(Unit unit)
        {
            if (unit == null) return;
            var view0 = fieldVisualizerPlayer0?.GetViewByUnit(unit);
            var view1 = fieldVisualizerPlayer1?.GetViewByUnit(unit);
            var view = view0 ?? view1;
            if (view == null) return;
            StartCoroutine(PlayUnitDestroySequence(view, view0 != null));
        }

        /// <summary>ユニット破壊時: ビデオ再生 → 破壊アニメ → オブジェクト削除。</summary>
        private IEnumerator PlayUnitDestroySequence(UnitView view, bool isPlayer0)
        {
            var uiParent = VideoEffectManager.Instance != null ? VideoEffectManager.Instance.uiParent : null;
            Vector2 effectPosition = Vector2.zero;
            if (uiParent != null)
            {
                var worldPos = view.transform.position;
                var localInParent = uiParent.InverseTransformPoint(worldPos);
                effectPosition = new Vector2(localInParent.x, localInParent.y);
            }

            if (isPlayer0)
                fieldVisualizerPlayer0.RemoveUnit(view);
            else
                fieldVisualizerPlayer1?.RemoveUnit(view);

            if (VideoEffectManager.Instance != null && _bombVideoPrefab != null)
                VideoEffectManager.Instance.PlayEffect(_bombVideoPrefab, effectPosition, 2f);

            yield return view.PlayDestroyAnimation();

            yield return new WaitForSeconds(0.15f);
            Destroy(view.gameObject);
        }

        /// <summary>指定プレイヤーのパートナーゾーン用ビューを生成・更新する。</summary>
        private void RefreshPartnerZoneView(int playerId)
        {
            var pm = PlayerManager.Instance;
            if (pm == null) return;
            var data = pm.GetPlayerData(playerId);
            if (data?.PartnerZone?.Partner == null) return;

            var anchor = playerId == 0 ? partnerZoneAnchorPlayer0 : partnerZoneAnchorPlayer1;
            var fieldV = playerId == 0 ? fieldVisualizerPlayer0 : fieldVisualizerPlayer1;
            var currentView = playerId == 0 ? _partnerCardViewPlayer0 : _partnerCardViewPlayer1;

            if (anchor == null || partnerCardViewPrefab == null) return;

            if (currentView != null)
            {
                Destroy(currentView.gameObject);
                if (playerId == 0)
                    _partnerCardViewPlayer0 = null;
                else
                    _partnerCardViewPlayer1 = null;
            }

            var view = Instantiate(partnerCardViewPrefab, anchor);
            view.transform.localPosition = Vector3.zero;
            view.Initialize(data.PartnerZone.Partner);
            view.OwnerPlayerId = playerId;
            view.SetDraggable(!data.PartnerZone.IsPartnerOnField);
            if (fieldV != null && fieldV.FieldAreaRect != null)
                view.SetFieldAreaRect(fieldV.FieldAreaRect);

            if (playerId == 0)
                _partnerCardViewPlayer0 = view;
            else
                _partnerCardViewPlayer1 = view;
        }

        /// <summary>パートナーがユニットとして召喚されたとき、フィールドに UnitView を追加する。</summary>
        private void OnPartnerSummoned(int playerId, Unit unit)
        {
            var fieldV = playerId == 0 ? fieldVisualizerPlayer0 : fieldVisualizerPlayer1;
            if (unitPrefab == null || fieldV == null) return;

            var spawnPos = fieldV.GetNextSpawnPosition();
            var unitView = Instantiate(unitPrefab, fieldV.transform);
            (unitView.transform as RectTransform).localPosition = spawnPos;
            unitView.Bind(unit);
            fieldV.AddUnit(unitView);

            RefreshPartnerZoneView(playerId);
        }

        /// <summary>パートナーがゾーンに戻ったとき、パートナーゾーン表示を再表示・ドラッグ可能にする。</summary>
        private void OnPartnerReturnedToZone(int playerId)
        {
            RefreshPartnerZoneView(playerId);
        }

        /// <summary>指定プレイヤーの PlayerInfoView を PlayerManager のデータで更新する。</summary>
        private void RefreshPlayerInfoView(int playerId)
        {
            var pm = PlayerManager.Instance;
            if (pm == null) return;
            var data = pm.GetPlayerData(playerId);
            if (playerId == 0)
                player0InfoView?.UpdateState(data);
            else if (playerId == 1)
                player1InfoView?.UpdateState(data);
        }

        /// <summary>
        /// プレイヤーIDとカードデータを受け取り、デッキ位置にCardViewを生成して手札の予定位置へ移動アニメーションを再生する
        /// </summary>
        public void PlayDrawAnimation(int playerId, Card cardData)
        {
            var handV = playerId == 0 ? handVisualizerPlayer0 : handVisualizerPlayer1;
            var deckT = playerId == 0 ? deckTransformPlayer0 : deckTransformPlayer1;
            var fieldV = playerId == 0 ? fieldVisualizerPlayer0 : fieldVisualizerPlayer1;

            if (cardPrefab == null || handV == null || deckT == null) return;

            var cardView = Instantiate(cardPrefab, handV.transform);
            cardView.transform.localPosition = handV.transform.InverseTransformPoint(deckT.position);
            cardView.Initialize(cardData);
            cardView.OwnerPlayerId = playerId;
            if (fieldV != null && fieldV.FieldAreaRect != null)
                cardView.SetFieldAreaRect(fieldV.FieldAreaRect);

            var targetLocal = handV.CalculatePosition(handV.CurrentCount, handV.CurrentCount + 1);
            StartCoroutine(AnimateCardToHand(cardView, targetLocal, handV));
        }

        /// <summary>カードを手札の目標位置まで移動させ、完了後に HandVisualizer に追加する。</summary>
        private IEnumerator AnimateCardToHand(CardView cardView, Vector3 targetLocalPosition, HandVisualizer targetHand)
        {
            var rt = cardView.transform as RectTransform;
            if (rt == null)
            {
                targetHand?.AddCard(cardView);
                yield break;
            }

            var start = rt.localPosition;
            var elapsed = 0f;
            while (elapsed < drawAnimationDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / drawAnimationDuration);
                rt.localPosition = Vector3.Lerp(start, targetLocalPosition, t);
                yield return null;
            }

            rt.localPosition = targetLocalPosition;
            targetHand?.AddCard(cardView);
        }

        /// <summary>呪文プレイ時: 手札のカードビューを消す。</summary>
        private void OnSpellPlayed(int playerId, Card card)
        {
            var handV = playerId == 0 ? handVisualizerPlayer0 : handVisualizerPlayer1;
            var cardView = handV?.GetCardViewByCard(card);
            if (cardView != null)
            {
                handV.RemoveCard(cardView);
                Destroy(cardView.gameObject);
            }
        }

        /// <summary>ユニット召喚時: 手札のカードを消し、フィールドに UnitView を生成・配置する。</summary>
        private void OnUnitSummoned(int playerId, Card card, Unit unit)
        {
            var handV = playerId == 0 ? handVisualizerPlayer0 : handVisualizerPlayer1;
            var fieldV = playerId == 0 ? fieldVisualizerPlayer0 : fieldVisualizerPlayer1;

            var cardView = handV?.GetCardViewByCard(card);
            if (cardView != null)
            {
                handV.RemoveCard(cardView);
                Destroy(cardView.gameObject);
            }

            if (unitPrefab == null || fieldV == null) return;

            var spawnPos = fieldV.GetNextSpawnPosition();
            var unitView = Instantiate(unitPrefab, fieldV.transform);
            (unitView.transform as RectTransform).localPosition = spawnPos;
            unitView.Bind(unit);
            fieldV.AddUnit(unitView);
        }

        /// <summary>
        /// ユニット召喚時の演出を再生する（OnUnitSummoned から呼ばれる）
        /// </summary>
        public void PlaySummonAnimation()
        {
            // 召喚演出は OnUnitSummoned 内で UnitView 生成・配置として実装済み
        }

        /// <summary>
        /// 指定プレイヤーの指定 InstanceId の Unit に対応する UnitView を返す。効果のターゲット選択用。
        /// </summary>
        public UnitView GetUnitViewByInstanceId(int ownerPlayerId, int instanceId)
        {
            var unit = PlayerManager.Instance?.GetUnitByInstanceId(ownerPlayerId, instanceId);
            if (unit == null) return null;
            var fv = ownerPlayerId == 0 ? fieldVisualizerPlayer0 : fieldVisualizerPlayer1;
            return fv?.GetViewByUnit(unit);
        }

        /// <summary>
        /// 指定プレイヤーのフィールドビジュアライザーを返す。ターゲット選択時に全 UnitView をハイライトする用。
        /// </summary>
        public FieldVisualizer GetFieldVisualizer(int playerId)
        {
            return playerId == 0 ? fieldVisualizerPlayer0 : fieldVisualizerPlayer1;
        }

        /// <summary>
        /// 指定ユニットの位置で効果演出を再生する。召喚時効果の解決時などに使用。
        /// </summary>
        public void PlayEffectAtUnit(int ownerPlayerId, int instanceId)
        {
            var view = GetUnitViewByInstanceId(ownerPlayerId, instanceId);
            if (view == null || _bombVideoPrefab == null) return;
            var uiParent = VideoEffectManager.Instance != null ? VideoEffectManager.Instance.uiParent : null;
            if (uiParent == null) return;
            var worldPos = view.transform.position;
            var localInParent = uiParent.InverseTransformPoint(worldPos);
            var effectPosition = new Vector2(localInParent.x, localInParent.y);
            VideoEffectManager.Instance?.PlayEffect(_bombVideoPrefab, effectPosition, 2f);
        }
    }
}
