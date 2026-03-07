using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public class CardView : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image artwork;
        [SerializeField] private Image frameImage;
        [SerializeField] private TextMeshProUGUI cost;
        [SerializeField] private TextMeshProUGUI attack;
        [SerializeField] private TextMeshProUGUI hp;
        [SerializeField] private TextMeshProUGUI cardNameText;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform fieldAreaRect;
        [SerializeField] private Image cardBackImage;
        [SerializeField] private List<GameObject> performanceDisplayObjects = new List<GameObject>();

        private RectTransform _rectTransform;
        private Canvas _canvas;

        public Card Card { get; private set; }
        public int OwnerPlayerId { get; set; }

        private HandVisualizer _handVisualizer;
        private Vector3 _baseTargetLocalPosition;
        private Vector3 _targetLocalPosition;
        private bool _isDragging;
        private bool _hasTargetPosition;
        private bool _isOpponentHandCard;
        private bool _mulliganSelected;
        private float _mulliganSelectedOffsetY = 30f;
        private const float MoveLerpSpeed = 12f;

        /// <summary>
        /// クリック時に発火するコールバック（マリガン時などで使用）。未設定なら何もしない。
        /// </summary>
        public Action OnClicked;

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
            if (artwork != null && !string.IsNullOrEmpty(data.Template.CardName))
                StartCoroutine(LoadArtworkIfExists(data.Template.CardName, artwork));

            var frameType = data.Template.CardType switch
            {
                CardType.Unit => FrameType.Unit,
                CardType.Spell => FrameType.Spell,
                CardType.Totem => FrameType.Spell,
                _ => FrameType.Unit
            };
            if (frameImage != null)
                StartCoroutine(FrameImageHelper.LoadFrameAsync(frameType, frameImage));
        }

        private static IEnumerator LoadArtworkIfExists(string cardName, Image target)
        {
            Debug.Log($"LoadArtworkIfExists: {cardName}");
            if (string.IsNullOrEmpty(cardName) || target == null) yield break;
            var address = "Assets/Images/" + cardName + ".png";
            var am = AddressableManager.Instance;
            if (am == null) yield break;

            var hasTask = am.HasAssetAsync(address);
            yield return new WaitUntil(() => hasTask.IsCompleted);
            if (!hasTask.Result) yield break;

            Task<Sprite> loadTask = null;
            try
            {
                Debug.Log($"LoadAssetAsync: {address}");
                loadTask = am.LoadAssetAsync<Sprite>(address);
            }
            catch
            {
                yield break;
            }
            yield return new WaitUntil(() => loadTask.IsCompleted);
            Debug.Log($"LoadAssetAsync completed: {loadTask.Result}");
            if (loadTask.Status == TaskStatus.RanToCompletion && loadTask.Result != null && target != null)
                target.sprite = loadTask.Result;
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
            _baseTargetLocalPosition = localPosition;
            _targetLocalPosition = _baseTargetLocalPosition + (_mulliganSelected ? Vector3.up * _mulliganSelectedOffsetY : Vector3.zero);
            _hasTargetPosition = true;
        }

        /// <summary>
        /// マリガン時の選択状態。true のとき少し上にオフセット表示する。
        /// </summary>
        public void SetMulliganSelected(bool selected)
        {
            if (_mulliganSelected == selected) return;
            _mulliganSelected = selected;
            _targetLocalPosition = _baseTargetLocalPosition + (_mulliganSelected ? Vector3.up * _mulliganSelectedOffsetY : Vector3.zero);
            _hasTargetPosition = true;
        }

        /// <summary>
        /// マリガンで選択中かどうか
        /// </summary>
        public bool IsMulliganSelected => _mulliganSelected;

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked?.Invoke();
        }

        /// <summary>
        /// 相手の手札として裏向き表示にする（back.png を表示し、性能表示を非表示にする）
        /// </summary>
        public void SetOpponentHandFaceDown(bool value)
        {
            _isOpponentHandCard = value;
            if (value)
            {
                if (cardBackImage != null)
                {
                    cardBackImage.gameObject.SetActive(true);
                    StartCoroutine(LoadCardBack());
                }
                if (performanceDisplayObjects != null)
                {
                    foreach (var go in performanceDisplayObjects)
                    {
                        if (go != null) go.SetActive(false);
                    }
                }
            }
            else
            {
                if (cardBackImage != null) cardBackImage.gameObject.SetActive(false);
                if (performanceDisplayObjects != null)
                {
                    foreach (var go in performanceDisplayObjects)
                    {
                        if (go != null) go.SetActive(true);
                    }
                }
            }
        }

        private const string CardBackAddress = "Assets/Images/back.png";

        private IEnumerator LoadCardBack()
        {
            if (cardBackImage == null) yield break;
            var am = AddressableManager.Instance;
            if (am == null) yield break;

            Task<Sprite> loadTask = null;
            try
            {
                loadTask = am.LoadAssetAsync<Sprite>(CardBackAddress);
            }
            catch
            {
                yield break;
            }
            yield return new WaitUntil(() => loadTask.IsCompleted);
            if (loadTask.Status == TaskStatus.RanToCompletion && loadTask.Result != null && cardBackImage != null)
                cardBackImage.sprite = loadTask.Result;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isOpponentHandCard) return;
            if (Card?.Template == null) return;
            CardDescriptionPanel.ShowDescription(Card.Template.Description);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CardDescriptionPanel.HideDescription();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var gameFlow = GameFlowManager.Instance;
            if (gameFlow != null && gameFlow.CurrentPhase == GamePhase.Mulligan)
                return;
            CardDescriptionPanel.HideDescription();
            _isDragging = true;
            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_rectTransform == null) return;
            BezierUIArrow.Instance?.UpdateArrow((Vector2)_rectTransform.position, eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
            _isDragging = false;
            BezierUIArrow.Instance?.Hide();

            var isOverField = fieldAreaRect != null && RectTransformUtility.RectangleContainsScreenPoint(fieldAreaRect, eventData.position, eventData.pressEventCamera);

            var gameFlow = GameFlowManager.Instance;
            var playerManager = PlayerManager.Instance;
            if (isOverField && Card != null && gameFlow != null && gameFlow.CurrentPhase == GamePhase.Normal && playerManager != null)
            {
                var data = playerManager.GetPlayerData(OwnerPlayerId);
                if (data != null && data.CurrentMP >= Card.Template.PlayCost)
                {
                    ActionQueueManager.Instance?.AddAction(new GameAction
                    {
                        ActionType = ActionType.Play,
                        SourceCard = Card
                    });
                    return;
                }
                if (data != null && data.CurrentMP < Card.Template.PlayCost)
                    GamePromptView.Instance?.ShowFadingMessage("マナが足りません");
            }

            _handVisualizer?.UpdateLayout();
        }
    }
}
