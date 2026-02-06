using System.Collections.Generic;
using CardBattle.Core.Deck;

namespace CardBattle.Core.Player
{
    /// <summary>
    /// 各プレイヤーが保持する手札について責任を持つ
    /// </summary>
    public class Hand
    {
        public HashSet<Card> Cards { get; set; } = new();
    }
}
