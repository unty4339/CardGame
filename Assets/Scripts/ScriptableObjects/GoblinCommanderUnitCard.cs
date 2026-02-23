namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// ゴブリンの統率者。登場中ゴブリン2枚手札・味方ゴブリン超速攻（ターン開始時/オーラは未実装）。
    /// </summary>
    public class GoblinCommanderUnitCard : UnitCardTemplateBase
    {
        public GoblinCommanderUnitCard()
        {
            cardName = "ゴブリンの統率者";
            playCost = 4;
            baseHP = 2;
            baseAttack = 2;
            description = "登場中：\n「ゴブリン」を2枚手札に加える。\n自分の「ゴブリン」は超速攻を得る。";
        }
    }
}
