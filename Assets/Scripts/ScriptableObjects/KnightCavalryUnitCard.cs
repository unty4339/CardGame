using CardBattle.Core.Enums;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 騎士団の騎兵。3/2/2。速攻。
    /// </summary>
    public class KnightCavalryUnitCard : UnitCardTemplateBase
    {
        public KnightCavalryUnitCard()
        {
            cardName = "騎士団の騎兵";
            playCost = 3;
            baseHP = 2;
            baseAttack = 2;
            keywords.Add(KeywordAbility.Rush);
            description = "速攻";
        }
    }
}
