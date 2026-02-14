using System.Collections.Generic;

namespace CardBattle.Core
{
    /// <summary>
    /// フィールド上のユニット1体分のスナップショット（値のコピーのみ保持）
    /// </summary>
    public struct UnitSnapshot
    {
        public int InstanceId { get; set; }
        public int HP { get; set; }
        public int Attack { get; set; }
        public int OwnerPlayerId { get; set; }
    }

    /// <summary>
    /// ある時点の盤面を不変で保持するスナップショット。参照ではなく値のコピーのみ持つ。
    /// </summary>
    public class BoardStateSnapshot
    {
        public int TurnPlayerId { get; set; }
        public int Player0HP { get; set; }
        public int Player1HP { get; set; }
        public int Player0MP { get; set; }
        public int Player1MP { get; set; }
        public int Player0HandCount { get; set; }
        public int Player1HandCount { get; set; }
        public List<UnitSnapshot> Player0Units { get; set; } = new();
        public List<UnitSnapshot> Player1Units { get; set; } = new();
    }
}
