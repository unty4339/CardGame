using CardBattle.Core.Partner;
using CardBattle.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardBattle.UI
{
    /// <summary>
    /// パートナーカード1枚の表示と、フィールドへのドラッグドロップによる召喚入力について責任を持つ
    /// </summary>
    public class PartnerCardView : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private Image artwork;
        [SerializeField] private Text cost;
        [SerializeField] private Text attack;
        [SerializeField] private Text hp;
        [SerializeField] private CanvasGroup canvasGroup;

        private RectTransform _rectTransform;
        private Canvas _canvas;
        private RectTransform _fieldAreaRect;

        public Partner Partner { get; private set; }
        public int OwnerPlayerId { get; set; }

        private bool _isDragging;
        private bool _draggable = true;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            _canvas = GetComponentInParent<Canvas>();
        }

        /// <summary>
        /// パートナーデータを受け取り、表示を更新する
        /// </summary>
        public void Initialize(CardBattle.Core.Partner.Partner partner)
        {
            Partner = partner;
            if (partner == null) return;

            if (cost != null) cost.text = partner.Cost.ToString();
            if (attack != null) attack.text = partner.BaseAttack.ToString();
            if (hp != null) hp.text = partner.BaseHP.ToString();
            if (artwork != null) { /* 絵柄は未設定 */ }
        }

        /// <summary>
        /// フィールドエリアの Rect を設定する（ドロップ判定用）
        /// </summary>
        public void SetFieldAreaRect(RectTransform rect)
        {
            _fieldAreaRect = rect;
        }

        /// <summary>
        /// パートナーがフィールドに登場中はドラッグ不可にする
        /// </summary>
        public void SetDraggable(bool draggable)
        {
            _draggable = draggable;
            if (canvasGroup != null)
                canvasGroup.alpha = draggable ? 1f : 0.5f;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_draggable) return;
            _isDragging = true;
            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_draggable || !_isDragging) return;
            if (_rectTransform == null) return;
            if (_canvas != null && _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                _rectTransform.position = new Vector3(eventData.position.x, eventData.position.y, _rectTransform.position.z);
            else if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_rectTransform, eventData.position, eventData.pressEventCamera, out var world))
                _rectTransform.position = new Vector3(world.x, world.y, _rectTransform.position.z);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
            _isDragging = false;

            if (!_draggable || Partner == null) return;

            var isOverField = _fieldAreaRect != null && RectTransformUtility.RectangleContainsScreenPoint(_fieldAreaRect, eventData.position, eventData.pressEventCamera);

            if (isOverField)
            {
                var partnerManager = PartnerManager.Instance;
                if (partnerManager != null)
                    partnerManager.SpawnPartnerAsUnit(OwnerPlayerId);
            }
        }
    }
}
