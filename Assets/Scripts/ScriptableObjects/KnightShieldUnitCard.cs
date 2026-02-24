using System.Collections.Generic;
using CardBattle.Core.Effects;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 騎士団の盾兵。2/2/1。効果なし。
    /// </summary>
    public class KnightShieldUnitCard : UnitCardTemplateBase
    {
        public KnightShieldUnitCard()
        {
            cardName = "騎士団の盾兵";
            playCost = 2;
            baseHP = 2;
            baseAttack = 1;
            description = "（効果なし）";
        }

        public override IEnumerable<IOnSummonEffect> GetOnSummonEffects()
        {
            yield break;
        }
    }
}
