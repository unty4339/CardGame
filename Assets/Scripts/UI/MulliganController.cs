using System.Collections.Generic;
using System.Linq;
using CardBattle.Core.Deck;
using CardBattle.Managers;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardBattle.UI
{
    /// <summary>
    /// 対戦開始時のマリガン（カード引き直し）専用UI。プレイヤー0のみ対象。
    /// </summary>
    public class MulliganController : MonoBehaviour
    {
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private RectTransform cardContainer;
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI confirmButtonLabel;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private float cardSpacing = 120f;
        private const float MulliganSelectedOffsetY = 30f;

        private bool _isResultPhase;
        private readonly List<CardView> _mulliganCards = new();

        private void Awake()
        {
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmClicked);
        }

        /// <summary>
        /// マリガンUIを表示する。手札データから新規に CardView を生成してパネル内に並べる（元の手札オブジェクトは触らない）。
        /// </summary>
        public void Show()
        {
            if (panelRoot == null || cardContainer == null) return;
            var gvm = GameVisualManager.Instance;
            if (gvm == null || gvm.CardPrefab == null) return;

            var pm = PlayerManager.Instance;
            if (pm == null) return;
            var playerData = pm.GetPlayerData(0);
            if (playerData?.Hand?.Cards == null) return;

            if (panelRoot != null) panelRoot.gameObject.SetActive(true);

            _isResultPhase = false;
            if (descriptionText != null)
                descriptionText.text = "デッキに戻したいカードを選択してください";

            _mulliganCards.Clear();
            var handCards = playerData.Hand.Cards.ToList();
            foreach (var card in handCards)
            {
                var view = Instantiate(gvm.CardPrefab, cardContainer);
                view.Initialize(card);
                view.OwnerPlayerId = 0;
                view.SetMulliganSelected(false);
                view.OnClicked = null;
                _mulliganCards.Add(view);
            }

            LayoutCards();
            for (var i = 0; i < _mulliganCards.Count; i++)
            {
                var view = _mulliganCards[i];
                var index = i;
                view.OnClicked = () => OnCardClicked(_mulliganCards[index]);
            }
            RefreshButtonLabel();
        }

        /// <summary>
        /// マリガンUIを非表示にする。
        /// </summary>
        public void Hide()
        {
            foreach (var view in _mulliganCards)
            {
                if (view != null && view.gameObject != null)
                    Destroy(view.gameObject);
            }
            _mulliganCards.Clear();
            if (panelRoot != null) panelRoot.gameObject.SetActive(false);
        }

        private void LayoutCards()
        {
            var count = _mulliganCards.Count;
            if (count == 0) return;
            var totalWidth = (count - 1) * cardSpacing;
            var startX = -totalWidth * 0.5f;
            for (var i = 0; i < count; i++)
            {
                var view = _mulliganCards[i];
                var rt = view.transform as RectTransform;
                var x = startX + i * cardSpacing;
                var y = view.IsMulliganSelected ? MulliganSelectedOffsetY : 0f;
                if (rt != null)
                {
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchoredPosition = new Vector2(x, y);
                }
                view.SetTargetPosition(new Vector3(x, 0f, 0f));
            }
        }

        private void OnCardClicked(CardView view)
        {
            if (view == null) return;
            view.SetMulliganSelected(!view.IsMulliganSelected);
            LayoutCards();
            RefreshButtonLabel();
        }

        private void RefreshButtonLabel()
        {
            if (confirmButtonLabel == null) return;
            var selected = 0;
            foreach (var v in _mulliganCards)
                if (v.IsMulliganSelected) selected++;
            confirmButtonLabel.text = selected > 0 ? "引き直し" : "スキップ";
        }

        private void OnConfirmClicked()
        {
            if (_isResultPhase)
            {
                var gfm = GameFlowManager.Instance;
                if (gfm != null)
                    gfm.NotifyMulliganConfirmed();
                return;
            }

            var cardsToReturn = new List<Card>();
            foreach (var view in _mulliganCards)
            {
                if (view.IsMulliganSelected && view.Card != null)
                    cardsToReturn.Add(view.Card);
            }

            foreach (var view in _mulliganCards)
            {
                if (view != null && view.gameObject != null)
                    Destroy(view.gameObject);
            }
            _mulliganCards.Clear();

            if (cardsToReturn.Count > 0)
            {
                var pm = PlayerManager.Instance;
                if (pm != null)
                    pm.Mulligan(0, cardsToReturn);
            }

            _isResultPhase = true;
            var gvm = GameVisualManager.Instance;
            var playerData = PlayerManager.Instance?.GetPlayerData(0);
            if (gvm != null && gvm.CardPrefab != null && playerData?.Hand?.Cards != null && cardContainer != null)
            {
                foreach (var card in playerData.Hand.Cards)
                {
                    var view = Instantiate(gvm.CardPrefab, cardContainer);
                    view.Initialize(card);
                    view.OwnerPlayerId = 0;
                    view.OnClicked = null;
                    _mulliganCards.Add(view);
                }
                LayoutCards();
            }

            if (descriptionText != null)
                descriptionText.text = "カードを引き直しました";
            if (confirmButtonLabel != null)
                confirmButtonLabel.text = "閉じる";
        }
    }
}
