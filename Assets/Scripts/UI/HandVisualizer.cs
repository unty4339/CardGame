using System.Collections.Generic;
using CardBattle.Core.Deck;
using UnityEngine;

namespace CardBattle.UI
{
    /// <summary>
    /// 手札にあるCardViewの扇状（アーチ状）配置の計算・適用について責任を持つ
    /// </summary>
    public class HandVisualizer : MonoBehaviour
    {
        [SerializeField] private float arcAngle = 30f;
        [SerializeField] private float spacing = 150f;

        private readonly List<CardView> _activeCards = new();

        public IReadOnlyList<CardView> ActiveCards => _activeCards;
        public int CurrentCount => _activeCards.Count;

        /// <summary>
        /// インデックスと総数を受け取り、指定インデックスのカードが画面上のどこにあるべきかを計算して返す（ローカル座標）
        /// </summary>
        public Vector3 CalculatePosition(int index, int totalCount)
        {
            if (totalCount <= 0) return Vector3.zero;
            if (totalCount == 1) return Vector3.zero;

            var halfArc = arcAngle * 0.5f * Mathf.Deg2Rad;
            var t = totalCount > 1 ? (float)index / (totalCount - 1) : 0f;
            var angle = Mathf.Lerp(-halfArc, halfArc, t);
            var radius = spacing * Mathf.Max(1, totalCount) * 0.5f;
            var x = radius * Mathf.Sin(angle);
            var y = -radius * Mathf.Cos(angle);
            return new Vector3(x, y, 0f);
        }

        /// <summary>
        /// CardViewを受け取り、アニメーションが完了したカードをリストに加え、整列処理を呼ぶ
        /// </summary>
        public void AddCard(CardView view)
        {
            if (view == null || _activeCards.Contains(view)) return;
            _activeCards.Add(view);
            view.SetHandVisualizer(this);
            UpdateLayout();
        }

        /// <summary>
        /// CardViewを受け取り、リストから外し、残りのカードを詰め直す
        /// </summary>
        public void RemoveCard(CardView view)
        {
            if (view == null) return;
            _activeCards.Remove(view);
            UpdateLayout();
        }

        /// <summary>
        /// 指定Cardに対応するCardViewを返す
        /// </summary>
        public CardView GetCardViewByCard(Card card)
        {
            if (card == null) return null;
            foreach (var view in _activeCards)
            {
                if (view.Card == card) return view;
            }
            return null;
        }

        /// <summary>
        /// リスト内の全カードに対して、目標座標へ滑らかに移動するよう指示する
        /// </summary>
        public void UpdateLayout()
        {
            var count = _activeCards.Count;
            for (var i = 0; i < count; i++)
            {
                var targetPos = CalculatePosition(i, count);
                _activeCards[i].SetTargetPosition(targetPos);
            }
        }
    }
}
