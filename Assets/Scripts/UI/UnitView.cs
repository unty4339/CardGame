using System;
using System.Collections;
using CardBattle.Core.Effects;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.Managers;
using Coffee.UIEffects;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardBattle.UI
{
    /// <summary>
    /// フィールドに出た後のキャラクター表示について責任を持つ。攻撃ドラッグ（自分ユニット→相手ユニット/相手プレイヤーゾーン）に対応。
    /// </summary>
    public class UnitView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        [SerializeField] private Text attackText;
        [SerializeField] private Text hpText;
        [SerializeField] private Image bodyImage;
        [SerializeField] private float stepBackAmount = 50f;
        [SerializeField] private float rushDuration = 0.1f;
        [Tooltip("攻撃時に対象を向くときのZ回転オフセット（度）。正面が上のスプライトなら -90 が目安")]
        [SerializeField] private float rotationOffsetDeg = -90f;
        [SerializeField] private UIEffect uiEffect;

        private Canvas _canvas;
        private RectTransform _rectTransform;
        private GameObject _dragGhost;
        private bool _isDraggingForAttack;
        private Color _normalColor = Color.white;
        private bool _selectableForEffect;

        public Unit Unit { get; private set; }

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            _canvas = GetComponentInParent<Canvas>();
        }
        private void Update()
        {
            if (uiEffect != null)
            {
                var gameFlow = GameFlowManager.Instance;
                bool myTurnAndCanAttack = Unit != null && gameFlow != null
                    && gameFlow.CurrentTurnPlayerId == Unit.OwnerPlayerId
                    && Unit.CanAttack;
                uiEffect.shadowFade = myTurnAndCanAttack ? 0.1f : 0f;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (Unit == null || Unit.OwnerPlayerId != 0) return;
            var controller = AttackDragController.Instance;
            if (controller == null || !controller.TryStartAttackDrag(this)) return;

            _isDraggingForAttack = true;
            CreateDragGhost(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDraggingForAttack || _dragGhost == null) return;
            var rt = _dragGhost.transform as RectTransform;
            if (rt == null) return;
            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                rt.position = new Vector3(eventData.position.x, eventData.position.y, rt.position.z);
            else if (eventData.pressEventCamera != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out var world))
                rt.position = new Vector3(world.x, world.y, rt.position.z);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isDraggingForAttack)
            {
                var controller = AttackDragController.Instance;
                controller?.OnAttackDragEnded(this, eventData.position, eventData.pressEventCamera);
                DestroyDragGhost();
                _isDraggingForAttack = false;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var gameFlow = GameFlowManager.Instance;
            if (gameFlow == null || gameFlow.CurrentPhase != GamePhase.TargetSelection) return;
            if (!_selectableForEffect || Unit == null) return;
            EffectResolver.Instance?.ConfirmTarget(EffectTarget.Unit(Unit.InstanceId));
        }

        /// <summary>
        /// 効果のターゲットとして選択可能かどうか。ターゲット選択モード時にハイライトと連動させる。
        /// </summary>
        public void SetSelectableForEffect(bool selectable)
        {
            _selectableForEffect = selectable;
        }

        /// <summary>
        /// ターゲット候補としてハイライト表示する
        /// </summary>
        public void SetHighlight(bool on)
        {
            if (bodyImage != null)
                bodyImage.color = on ? Color.Lerp(_normalColor, Color.yellow, 0.4f) : _normalColor;
        }

        /// <summary>
        /// ターゲット外としてグレーアウト（半透明）する
        /// </summary>
        public void SetGrayedOut(bool gray)
        {
            if (bodyImage != null)
                bodyImage.color = gray ? new Color(0.6f, 0.6f, 0.6f, 0.6f) : _normalColor;
        }

        private void CreateDragGhost(PointerEventData eventData)
        {
            if (_canvas == null || _rectTransform == null) return;

            _dragGhost = new GameObject("UnitViewDragGhost");
            var ghostRt = _dragGhost.AddComponent<RectTransform>();
            _dragGhost.transform.SetParent(_canvas.transform, false);

            ghostRt.pivot = _rectTransform.pivot;
            ghostRt.anchorMin = new Vector2(0.5f, 0.5f);
            ghostRt.anchorMax = new Vector2(0.5f, 0.5f);
            ghostRt.sizeDelta = _rectTransform.sizeDelta;
            ghostRt.localScale = _rectTransform.localScale;

            if (_canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                ghostRt.position = new Vector3(eventData.position.x, eventData.position.y, 0f);
            else if (eventData.pressEventCamera != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(ghostRt, eventData.position, eventData.pressEventCamera, out var world))
                ghostRt.position = new Vector3(world.x, world.y, ghostRt.position.z);

            if (bodyImage != null)
            {
                var ghostImage = _dragGhost.AddComponent<Image>();
                ghostImage.sprite = bodyImage.sprite;
                ghostImage.color = bodyImage.color;
                ghostImage.raycastTarget = false;
            }

            var canvasGroup = _dragGhost.AddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.9f;
        }

        private void DestroyDragGhost()
        {
            if (_dragGhost != null)
            {
                Destroy(_dragGhost);
                _dragGhost = null;
            }
        }

        /// <summary>
        /// ユニットデータを受け取り、紐付けてHPやATKの表示を同期する
        /// </summary>
        public void Bind(Unit unitData)
        {
            Unit = unitData;
            if (bodyImage != null) _normalColor = bodyImage.color;
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (Unit == null) return;
            if (attackText != null) attackText.text = Unit.Attack.ToString();
            if (hpText != null) hpText.text = Unit.HP.ToString();
        }

        /// <summary>
        /// 紐付いたUnitの現在値で表示を再同期する（HP変更・攻撃力変更時などに外部から呼ぶ）
        /// </summary>
        public void RefreshFromUnit()
        {
            RefreshDisplay();
        }

        /// <summary>
        /// ターゲットのUnitViewに向かって突進して戻る演出を再生する（従来API・コールバックなし）
        /// </summary>
        public void PlayAttackAnimation(UnitView target)
        {
            if (target == null) return;
            StartCoroutine(PlayAttackAnimationCoroutine(target.transform.position, null, null));
        }

        /// <summary>
        /// 後退→急突進→到達時コールバック→元の位置に戻るの4段階攻撃演出。呼び出し元で yield return して完了を待つ。
        /// </summary>
        public IEnumerator PlayAttackAnimationCoroutine(Vector3 targetWorldPosition, Action onReachedTarget, Action onComplete)
        {
            var startPos = transform.position;
            var startEuler = transform.localEulerAngles;

            // 対象方向のZ回転角（rotationOffsetDeg で正面が上のときなどに合わせる。相手攻撃時は下を正面として +180°）
            var dir = (targetWorldPosition - startPos);
            dir.z = 0f;
            var offset = rotationOffsetDeg;
            if (Unit != null && Unit.OwnerPlayerId == 1)
                offset += 180f;
            var targetAngleZ = dir.sqrMagnitude > 0.0001f
                ? Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + offset
                : startEuler.z;

            var stepBackDir = Unit != null && Unit.OwnerPlayerId == 0 ? Vector3.down : Vector3.up;
            var stepBackPos = startPos + stepBackDir * stepBackAmount;
            var duration = rushDuration;

            // 1. 後退（位置＋少しずつ対象方向へ向きを変える）
            var stepBackDuration = 0.08f;
            var elapsed = 0f;
            while (elapsed < stepBackDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / stepBackDuration);
                transform.position = Vector3.Lerp(startPos, stepBackPos, t);
                var z = Mathf.LerpAngle(startEuler.z, targetAngleZ, t);
                transform.localEulerAngles = new Vector3(startEuler.x, startEuler.y, z);
                yield return null;
            }
            transform.position = stepBackPos;
            transform.localEulerAngles = new Vector3(startEuler.x, startEuler.y, targetAngleZ);

            // 2. 急突進（対象向きのまま）
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                var tEased = 1f - (1f - t) * (1f - t);
                transform.position = Vector3.Lerp(stepBackPos, targetWorldPosition, tEased);
                yield return null;
            }
            transform.position = targetWorldPosition;

            // 3. 到達時コールバック（ビデオ再生など）
            onReachedTarget?.Invoke();
            yield return null;

            // 4. 元の位置に戻る（向きも元に戻す）
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                transform.position = Vector3.Lerp(targetWorldPosition, startPos, t);
                var z = Mathf.LerpAngle(targetAngleZ, startEuler.z, t);
                transform.localEulerAngles = new Vector3(startEuler.x, startEuler.y, z);
                yield return null;
            }
            transform.position = startPos;
            transform.localEulerAngles = startEuler;

            onComplete?.Invoke();
        }

        /// <summary>
        /// ダメージ値を受け取り、揺れる・赤く点滅するなどの演出を再生する
        /// </summary>
        public void PlayDamageAnimation(int damage)
        {
            StartCoroutine(DamageAnimationCoroutine());
        }

        private IEnumerator DamageAnimationCoroutine()
        {
            if (bodyImage != null)
            {
                var original = bodyImage.color;
                bodyImage.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                bodyImage.color = original;
            }

            var rt = transform as RectTransform;
            if (rt != null)
            {
                var orig = rt.localPosition;
                for (var i = 0; i < 3; i++)
                {
                    rt.localPosition = orig + new Vector3(3f, 0f, 0f);
                    yield return new WaitForSeconds(0.03f);
                    rt.localPosition = orig + new Vector3(-3f, 0f, 0f);
                    yield return new WaitForSeconds(0.03f);
                }
                rt.localPosition = orig;
            }

            RefreshDisplay();
        }

        /// <summary>
        /// 破壊演出を再生する（画像を白くして縮小）。呼び出し元で yield return して完了を待つ。
        /// </summary>
        public IEnumerator PlayDestroyAnimation()
        {
            const float duration = 0.3f;
            if (bodyImage != null)
                bodyImage.color = Color.white;

            if (_rectTransform != null)
            {
                var startScale = _rectTransform.localScale;
                var endScale = new Vector3(0.01f, 0.01f, 0.01f);
                var elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    var t = Mathf.Clamp01(elapsed / duration);
                    var tEased = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);
                    _rectTransform.localScale = Vector3.Lerp(startScale, endScale, tEased);
                    yield return null;
                }
                _rectTransform.localScale = endScale;
            }
        }
    }
}
