using System.Collections.Generic;
using CardBattle.Core.Effects;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 騎士団の見習い。1/1/1。効果なし。
    /// </summary>
    public class KnightSquireUnitCard : UnitCardTemplateBase
    {
        public KnightSquireUnitCard()
        {
            cardName = "騎士団の見習い";
            playCost = 1;
            baseHP = 1;
            baseAttack = 1;
            description = "（効果なし）";
        }

        public override IEnumerable<IOnSummonEffect> GetOnSummonEffects()
        {
            yield break;
        }
    }
}
