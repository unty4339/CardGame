using CardBattle.Core.Enums;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 騎士団の槍兵。2/2/2。速攻。
    /// </summary>
    public class KnightSpearmanUnitCard : UnitCardTemplateBase
    {
        public KnightSpearmanUnitCard()
        {
            cardName = "騎士団の槍兵";
            playCost = 2;
            baseHP = 2;
            baseAttack = 2;
            keywords.Add(KeywordAbility.Rush);
            description = "速攻";
        }
    }
}
