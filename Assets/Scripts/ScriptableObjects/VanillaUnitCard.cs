using System.Collections.Generic;
using CardBattle.Core.Effects;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 効果なしのユニットカード。GetOnSummonEffects は空。ダミーデッキ等で使用。
    /// </summary>
    public class VanillaUnitCard : UnitCardTemplateBase
    {
        public VanillaUnitCard()
        {
            cardName = "雑兵";
            playCost = 1;
            baseHP = 1;
            baseAttack = 1;
        }

        /// <summary>
        /// 召喚時効果なし。空を返す。
        /// </summary>
        public override IEnumerable<IOnSummonEffect> GetOnSummonEffects()
        {
            yield break;
        }
    }
}
