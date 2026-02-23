using CardBattle.Core.Enums;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// ゴブリン。速攻。
    /// </summary>
    public class GoblinUnitCard : UnitCardTemplateBase
    {
        public GoblinUnitCard()
        {
            cardName = "ゴブリン";
            playCost = 1;
            baseHP = 1;
            baseAttack = 1;
            keywords.Add(KeywordAbility.Rush);
            description = "速攻";
        }
    }
}
