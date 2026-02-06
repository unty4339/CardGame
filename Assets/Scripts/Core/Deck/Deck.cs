using System.Collections.Generic;

namespace CardBattle.Core.Deck
{
    /// <summary>
    /// 各プレイヤーが保持するカードの束について責任を持つ
    /// </summary>
    public class Deck
    {
        public List<Card> Cards { get; set; } = new();
    }
}
