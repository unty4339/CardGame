using CardBattle.Core.Enums;

namespace CardBattle.Core
{
    /// <summary>
    /// 各カードやユニットを選択して使用できる行動について責任を持つ
    /// </summary>
    public class GameAction
    {
        public ActionType ActionType { get; set; }
        public object Target { get; set; }
        public Deck.Card SourceCard { get; set; }
        public Field.Unit SourceUnit { get; set; }
    }
}
