using System.Collections.Generic;
using CardBattle.Core.Deck;
using CardBattle.Core.Field;

namespace CardBattle.AI
{
    /// <summary>
    /// 現在の自分から見た状況を一意に表現する状態データについて責任を持つ
    /// </summary>
    public class GameState
    {
        public int MyPlayerId { get; set; }
        public int OpponentPlayerId { get; set; }
        public List<Card> MyHand { get; set; } = new();
        public FieldZone MyField { get; set; } = new();
        public FieldZone OpponentField { get; set; } = new();
        public int MyHP { get; set; }
        public int OpponentHP { get; set; }
        public int MyMP { get; set; }
        public int OpponentMP { get; set; }
    }
}
