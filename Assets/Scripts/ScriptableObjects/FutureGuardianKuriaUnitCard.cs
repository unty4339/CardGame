using CardBattle.Core.Enums;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 未来航士、リュシア。パートナー。登場時マナ1以上で1消費・相手全体1ダメージ・破壊数分マナ回復。
    /// </summary>
    public class FutureGuardianKuriaUnitCard : UnitCardTemplateBase
    {
        public FutureGuardianKuriaUnitCard()
        {
            cardName = "未来航士、リュシア";
            playCost = 4;
            baseHP = 2;
            baseAttack = 4;
            description = "パートナー\n登場時：\nマナが1以上ある場合、マナを1消費する。相手ユニット全体に1ダメージを与え、破壊したユニットの数と等しい量だけマナを回復する。";
        }
    }
}
