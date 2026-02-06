using System.Collections.Generic;

namespace CardBattle.Core.Field
{
    /// <summary>
    /// ユニットとトーテムが配置されるフィールドゾーンについて責任を持つ
    /// </summary>
    public class FieldZone
    {
        public List<Unit> Units { get; set; } = new();
        public List<Totem> Totems { get; set; } = new();
    }
}
