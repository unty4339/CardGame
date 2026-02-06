using System.Collections.Generic;
using CardBattle.Core;
using CardBattle.ScriptableObjects;

namespace CardBattle.Core.Deck
{
    /// <summary>
    /// 実際にデッキや手札を行き来するカードのデータについて責任を持つ
    /// </summary>
    public class Card
    {
        public int CardID { get; set; }
        public CardTemplate Template { get; set; }
        public List<GameAction> AvailableActions { get; set; } = new();
    }
}
