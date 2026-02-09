using System;
using System.Collections;
using System.Threading.Tasks;
using CardBattle.Core.Deck;
using CardBattle.Core.Field;
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

        private const string BombVideoAddress = "Assets/Prefabs/BombVideo.prefab";
        private GameObject _bombVideoPrefab;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            var pm = PlayerManager.Instance;
            if (pm != null)
            {
                pm.OnCardDrawn += PlayDrawAnimation;
                pm.OnUnitSummoned += OnUnitSummoned;
                pm.OnPlayerDataChanged += RefreshPlayerInfoView;
                pm.OnUnitHpChanged += OnUnitHpChanged;
                pm.OnUnitDestroyed += OnUnitDestroyed;
            }
            StartCoroutine(LoadBombVideoPrefab());
        }

        private IEnumerator LoadBombVideoPrefab()
        {
            var am = AddressableManager.Instance;
            if (am == null) yield break;

            Task<GameObject> task = am.LoadAssetAsync<GameObject>(BombVideoAddress);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Status == TaskStatus.RanToCompletion)
                _bombVideoPrefab = task.Result;
            else
                Debug.LogWarning("[GameVisualManager] BombVideo prefab load failed: " + task.Exception?.Message);
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
                pm.OnPlayerDataChanged -= RefreshPlayerInfoView;
                pm.OnUnitHpChanged -= OnUnitHpChanged;
                pm.OnUnitDestroyed -= OnUnitDestroyed;
            }
        }

        /// <summary>
        /// 攻撃演出を再生し、完了後に onComplete を呼ぶ。プレイヤー・AI 共通。
        /// </summary>
        public void PlayAttackAndResolve(Unit attacker, object target, Action onComplete)
        {
            if (onComplete == null) return;
            StartCoroutine(PlayAttackAndResolveCoroutine(attacker, target, onComplete));
        }

        private IEnumerator PlayAttackAndResolveCoroutine(Unit attacker, object target, Action onComplete)
        {
            var attackerView = fieldVisualizerPlayer0?.GetViewByUnit(attacker) ?? fieldVisualizerPlayer1?.GetViewByUnit(attacker);
            if (attackerView != null && target is Unit targetUnit)
            {
                var targetView = fieldVisualizerPlayer0?.GetViewByUnit(targetUnit) ?? fieldVisualizerPlayer1?.GetViewByUnit(targetUnit);
                if (targetView != null)
                {
                    attackerView.PlayAttackAnimation(targetView);
                    yield return new WaitForSeconds(0.35f);
                }
            }
            else if (target is int)
            {
                yield return new WaitForSeconds(0.3f);
            }

            onComplete();
        }

        private void OnUnitHpChanged(Unit unit)
        {
            if (unit == null) return;
            var view = fieldVisualizerPlayer0?.GetViewByUnit(unit) ?? fieldVisualizerPlayer1?.GetViewByUnit(unit);
            view?.RefreshFromUnit();
        }

        private void OnUnitDestroyed(Unit unit)
        {
            if (unit == null) return;
            var view0 = fieldVisualizerPlayer0?.GetViewByUnit(unit);
            var view1 = fieldVisualizerPlayer1?.GetViewByUnit(unit);
            var view = view0 ?? view1;
            if (view == null) return;
            StartCoroutine(PlayUnitDestroySequence(view, view0 != null));
        }

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
    }
}
