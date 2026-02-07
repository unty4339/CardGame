using CardBattle.Core.Field;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardBattle.UI
{
    /// <summary>
    /// フィールドに出た後のキャラクター表示について責任を持つ。攻撃ドラッグ（自分ユニット→相手ユニット/相手プレイヤーゾーン）に対応。
    /// </summary>
    public class UnitView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Text attackText;
        [SerializeField] private Text hpText;
        [SerializeField] private Image bodyImage;

        private Canvas _canvas;
        private RectTransform _rectTransform;
        private GameObject _dragGhost;
        private bool _isDraggingForAttack;

        public Unit Unit { get; private set; }

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            _canvas = GetComponentInParent<Canvas>();
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
        /// ターゲットのUnitViewに向かって突進して戻る演出を再生する
        /// </summary>
        public void PlayAttackAnimation(UnitView target)
        {
            if (target == null) return;
            StartCoroutine(AttackAnimationCoroutine(target));
        }

        private System.Collections.IEnumerator AttackAnimationCoroutine(UnitView target)
        {
            var startPos = transform.position;
            var targetPos = target.transform.position;
            var duration = 0.15f;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            transform.position = targetPos;
            yield return new WaitForSeconds(0.05f);

            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / duration;
                transform.position = Vector3.Lerp(targetPos, startPos, t);
                yield return null;
            }

            transform.position = startPos;
        }

        /// <summary>
        /// ダメージ値を受け取り、揺れる・赤く点滅するなどの演出を再生する
        /// </summary>
        public void PlayDamageAnimation(int damage)
        {
            StartCoroutine(DamageAnimationCoroutine());
        }

        private System.Collections.IEnumerator DamageAnimationCoroutine()
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
    }
}
