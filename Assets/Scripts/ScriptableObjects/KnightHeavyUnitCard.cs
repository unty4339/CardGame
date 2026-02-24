using System.Collections.Generic;
using CardBattle.Core.Effects;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 騎士団の重装兵。3/3/2。効果なし。
    /// </summary>
    public class KnightHeavyUnitCard : UnitCardTemplateBase
    {
        public KnightHeavyUnitCard()
        {
            cardName = "騎士団の重装兵";
            playCost = 3;
            baseHP = 3;
            baseAttack = 2;
            description = "（効果なし）";
        }

        public override IEnumerable<IOnSummonEffect> GetOnSummonEffects()
        {
            yield break;
        }
    }
}
