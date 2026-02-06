using CardBattle.Core.Deck;
using CardBattle.Core.Field;
using CardBattle.Core.Partner;
using DeckData = CardBattle.Core.Deck.Deck;

namespace CardBattle.Core.Player
{
    /// <summary>
    /// プレイヤーごとの情報について責任を持つ
    /// </summary>
    public class PlayerData
    {
        public int HP { get; set; }
        public int MaxMP { get; set; }
        public int CurrentMP { get; set; }
        public DeckData Deck { get; set; }
        public Hand Hand { get; set; }
        public FieldZone FieldZone { get; set; }
        public PartnerZone PartnerZone { get; set; }
    }
}
