using System.Collections.Generic;
using CardBattle.ScriptableObjects;
using Random = UnityEngine.Random;

namespace CardBattle.Core.Deck
{
    /// <summary>
    /// デッキの構築について責任を持つ
    /// </summary>
    public static class DeckBuilder
    {
        private static int _nextCardId;

        /// <summary>
        /// デッキレシピを受け取り、Deckを返す。デッキはカード1枚ずつが区別され順序付きになる
        /// </summary>
        public static Deck BuildDeck(DeckRecipe recipe)
        {
            var deck = new Deck();
            var cards = new List<Card>();

            if (recipe?.Entries == null) return deck;

            foreach (var entry in recipe.Entries)
            {
                for (var i = 0; i < entry.Count; i++)
                {
                    var card = new Card
                    {
                        CardID = _nextCardId++,
                        Template = entry.Template
                    };
                    cards.Add(card);
                }
            }

            Shuffle(cards);
            deck.Cards = cards;
            return deck;
        }

        /// <summary>
        /// デッキの順序をランダムに並べ替える
        /// </summary>
        public static void Shuffle(List<Card> cards)
        {
            var n = cards.Count;
            while (n > 1)
            {
                n--;
                var k = Random.Range(0, n + 1);
                (cards[k], cards[n]) = (cards[n], cards[k]);
            }
        }
    }
}
