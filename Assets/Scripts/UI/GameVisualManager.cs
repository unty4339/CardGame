using System.Collections;
using CardBattle.Core.Deck;
using CardBattle.Core.Field;
using CardBattle.Managers;
using UnityEngine;

namespace CardBattle.UI
{
    /// <summary>
    /// PlayerManagerのイベントを監視し、カードの生成や移動アニメーションを指揮する。UIとデータの橋渡しについて責任を持つ
    /// </summary>
    public class GameVisualManager : MonoBehaviour
    {
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private Transform deckTransform;
        [SerializeField] private HandVisualizer handVisualizer;
        [SerializeField] private FieldVisualizer fieldVisualizer;
        [SerializeField] private UnitView unitPrefab;
        [SerializeField] private float drawAnimationDuration = 0.3f;

        private void Start()
        {
            var pm = PlayerManager.Instance;
            if (pm != null)
            {
                pm.OnCardDrawn += PlayDrawAnimation;
                pm.OnUnitSummoned += OnUnitSummoned;
            }
        }

        private void OnDestroy()
        {
            var pm = PlayerManager.Instance;
            if (pm != null)
            {
                pm.OnCardDrawn -= PlayDrawAnimation;
                pm.OnUnitSummoned -= OnUnitSummoned;
            }
        }

        /// <summary>
        /// プレイヤーIDとカードデータを受け取り、デッキ位置にCardViewを生成して手札の予定位置へ移動アニメーションを再生する
        /// </summary>
        public void PlayDrawAnimation(int playerId, Card cardData)
        {
            if (cardPrefab == null || handVisualizer == null || deckTransform == null) return;

            var cardView = Instantiate(cardPrefab, handVisualizer.transform);
            cardView.transform.localPosition = handVisualizer.transform.InverseTransformPoint(deckTransform.position);
            cardView.Initialize(cardData);
            cardView.OwnerPlayerId = playerId;
            if (fieldVisualizer != null && fieldVisualizer.FieldAreaRect != null)
                cardView.SetFieldAreaRect(fieldVisualizer.FieldAreaRect);

            var targetLocal = handVisualizer.CalculatePosition(handVisualizer.CurrentCount, handVisualizer.CurrentCount + 1);
            StartCoroutine(AnimateCardToHand(cardView, targetLocal));
        }

        private IEnumerator AnimateCardToHand(CardView cardView, Vector3 targetLocalPosition)
        {
            var rt = cardView.transform as RectTransform;
            if (rt == null)
            {
                handVisualizer.AddCard(cardView);
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
            handVisualizer.AddCard(cardView);
        }

        private void OnUnitSummoned(int playerId, Card card, Unit unit)
        {
            var cardView = handVisualizer?.GetCardViewByCard(card);
            if (cardView != null)
            {
                handVisualizer.RemoveCard(cardView);
                Destroy(cardView.gameObject);
            }

            if (unitPrefab == null || fieldVisualizer == null) return;

            var spawnPos = fieldVisualizer.GetNextSpawnPosition();
            var unitView = Instantiate(unitPrefab, fieldVisualizer.transform);
            (unitView.transform as RectTransform).localPosition = spawnPos;
            unitView.Bind(unit);
            fieldVisualizer.AddUnit(unitView);
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
