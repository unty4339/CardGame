using System.Collections.Generic;
using System.Linq;

namespace CardBattle.Core.Field
{
    /// <summary>
    /// ユニットとトーテムが配置されるフィールドゾーンについて責任を持つ
    /// </summary>
    public class FieldZone
    {
        public List<Unit> Units { get; set; } = new();

        /// <summary>
        /// 攻撃権付与・攻撃対象選択の対象にできるユニット（トーテムを除く）を返す。
        /// </summary>
        public IEnumerable<Unit> GetAttackableUnits() =>
            Units?.Where(u => u != null && !u.IsTotem) ?? Enumerable.Empty<Unit>();

        /// <summary>
        /// スペル・効果の対象にできるユニット（トーテムを除く）を返す。
        /// </summary>
        public IEnumerable<Unit> GetTargetableUnits() =>
            Units?.Where(u => u != null && !u.IsTotem) ?? Enumerable.Empty<Unit>();
    }
}
