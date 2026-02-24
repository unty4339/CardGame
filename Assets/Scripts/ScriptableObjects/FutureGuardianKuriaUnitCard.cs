using CardBattle.Core.Enums;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 未来航士、リュシア。パートナー。登場時マナ2以上で相手全体1ダメージ・破壊数分マナ回復。
    /// </summary>
    public class FutureGuardianKuriaUnitCard : UnitCardTemplateBase
    {
        public FutureGuardianKuriaUnitCard()
        {
            cardName = "未来航士、リュシア";
            playCost = 3;
            baseHP = 3;
            baseAttack = 3;
            description = "パートナー\n登場時：\nマナが2以上ある場合、マナを2消費して相手ユニット全体に1ダメージを与え、破壊したユニットの数と等しい量だけマナを回復する。";
        }
    }
}
