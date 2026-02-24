using System.Collections;
using System.Threading.Tasks;
using CardBattle.Core.Effects;
using CardBattle.Core.Enums;
using CardBattle.Core.Partner;
using CardBattle.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CardBattle.UI
{
    /// <summary>
    /// パートナーカード1枚の表示と、フィールドへのドラッグドロップによる召喚入力について責任を持つ
    /// </summary>
    public class PartnerCardView : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image artwork;
        [SerializeField] private TextMeshProUGUI cost;
        [SerializeField] private TextMeshProUGUI attack;
        [SerializeField] private TextMeshProUGUI hp;
        [SerializeField] private CanvasGroup canvasGroup;

        private RectTransform _rectTransform;
        private Canvas _canvas;
        private RectTransform _fieldAreaRect;

        public Partner Partner { get; private set; }
        public int OwnerPlayerId { get; set; }

        private bool _isDragging;
        private bool _draggable = true;
        private bool _selectableForEffect;

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
            if (artwork != null && !string.IsNullOrEmpty(partner.CardName))
                StartCoroutine(LoadArtworkIfExists(partner.CardName, artwork));
        }

        private static IEnumerator LoadArtworkIfExists(string cardName, Image target)
        {
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
                loadTask = am.LoadAssetAsync<Sprite>(address);
            }
            catch
            {
                yield break;
            }
            yield return new WaitUntil(() => loadTask.IsCompleted);
            if (loadTask.Status == TaskStatus.RanToCompletion && loadTask.Result != null && target != null)
                target.sprite = loadTask.Result;
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

        /// <summary>
        /// 効果のターゲットとしてパートナーカードを選択可能にする（ペアリング対象選択時など）
        /// </summary>
        public void SetSelectableForEffect(bool selectable)
        {
            _selectableForEffect = selectable;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_selectableForEffect || Partner == null) return;
            var gameFlow = GameFlowManager.Instance;
            if (gameFlow == null || gameFlow.CurrentPhase != GamePhase.TargetSelection) return;
            EffectResolver.Instance?.ConfirmTarget(EffectTarget.PartnerCard(OwnerPlayerId));
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            CardDescriptionPanel.ShowDescription(Partner?.Description);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CardDescriptionPanel.HideDescription();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_draggable) return;
            CardDescriptionPanel.HideDescription();
            _isDragging = true;
            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_draggable || !_isDragging) return;
            if (_rectTransform == null) return;
            BezierUIArrow.Instance?.UpdateArrow((Vector2)_rectTransform.position, eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
            _isDragging = false;
            BezierUIArrow.Instance?.Hide();

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
