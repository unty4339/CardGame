using CardBattle.Core;
using CardBattle.Core.Deck;
using CardBattle.Core.Enums;
using CardBattle.Managers;
using CardBattle.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardBattle.UI
{
    /// <summary>
    /// カード1枚の表示と、マウス操作（ドラッグおよびドロップ）の入力受け付けについて責任を持つ
    /// </summary>
    public class CardView : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        [SerializeField] private Image artwork;
        [SerializeField] private TextMeshProUGUI cost;
        [SerializeField] private TextMeshProUGUI attack;
        [SerializeField] private TextMeshProUGUI hp;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform fieldAreaRect;

        private RectTransform _rectTransform;
        private Canvas _canvas;

        public Card Card { get; private set; }
        public int OwnerPlayerId { get; set; }

        private HandVisualizer _handVisualizer;
        private Vector3 _targetLocalPosition;
        private bool _isDragging;
        private bool _hasTargetPosition;
        private const float MoveLerpSpeed = 12f;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            _canvas = GetComponentInParent<Canvas>();
        }

        private void Update()
        {
            if (_isDragging || !_hasTargetPosition) return;
            if (_rectTransform == null) return;

            _rectTransform.localPosition = Vector3.Lerp(_rectTransform.localPosition, _targetLocalPosition, Time.deltaTime * MoveLerpSpeed);
            if (Vector3.Distance(_rectTransform.localPosition, _targetLocalPosition) < 0.5f)
                _rectTransform.localPosition = _targetLocalPosition;
        }

        /// <summary>
        /// カードデータを受け取り、表示を更新する
        /// </summary>
        public void Initialize(Card data)
        {
            Card = data;
            if (data?.Template == null) return;

            if (cost != null) cost.text = data.Template.PlayCost.ToString();
            var unitTemplate = data.Template as UnitCardTemplateBase;
            if (attack != null) attack.text = unitTemplate != null ? unitTemplate.BaseAttack.ToString() : "0";
            if (hp != null) hp.text = unitTemplate != null ? unitTemplate.BaseHP.ToString() : "0";
            if (cardNameText != null) cardNameText.text = data.Template.CardName;
            if (artwork != null) { /* 絵柄は CardTemplate に Sprite が無いため未設定 */ }
        }

        /// <summary>
        /// HandVisualizer を設定する（AddCard 時に呼ばれる）
        /// </summary>
        public void SetHandVisualizer(HandVisualizer handVisualizer)
        {
            _handVisualizer = handVisualizer;
        }

        /// <summary>
        /// フィールドエリアの Rect を設定する（ドロップ判定用）
        /// </summary>
        public void SetFieldAreaRect(RectTransform rect)
        {
            fieldAreaRect = rect;
        }

        /// <summary>
        /// 目標ローカル座標を設定し、滑らかに移動する
        /// </summary>
        public void SetTargetPosition(Vector3 localPosition)
        {
            _targetLocalPosition = localPosition;
            _hasTargetPosition = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
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

            var isOverField = fieldAreaRect != null && RectTransformUtility.RectangleContainsScreenPoint(fieldAreaRect, eventData.position, eventData.pressEventCamera);

            var gameFlow = GameFlowManager.Instance;
            var playerManager = PlayerManager.Instance;
            if (isOverField && Card != null && gameFlow != null && gameFlow.CurrentPhase == GamePhase.Normal && playerManager != null)
            {
                var data = playerManager.GetPlayerData(OwnerPlayerId);
                if (data != null && data.CurrentMP >= Card.Template.PlayCost)
                {
                    if (Card.Template.CardType == CardType.Unit)
                    {
                        if (playerManager.TryPlayCard(OwnerPlayerId, Card))
                            return;
                    }
                    else
                    {
                        ActionQueueManager.Instance?.AddAction(new GameAction
                        {
                            ActionType = ActionType.Play,
                            SourceCard = Card
                        });
                        return;
                    }
                }
            }

            _handVisualizer?.UpdateLayout();
        }
    }
}
