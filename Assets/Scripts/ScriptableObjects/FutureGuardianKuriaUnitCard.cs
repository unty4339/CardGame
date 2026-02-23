using CardBattle.Core.Enums;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 未来航路の番人、クーリア。パートナー。登場時マナ2で選択効果（未実装）。
    /// </summary>
    public class FutureGuardianKuriaUnitCard : UnitCardTemplateBase
    {
        public FutureGuardianKuriaUnitCard()
        {
            cardName = "未来航路の番人、クーリア";
            playCost = 3;
            baseHP = 3;
            baseAttack = 3;
            description = "パートナー\n登場時：\nマナを2支払える。そうした場合、以下からひとつを選び適用する。\n・これを+1/0し超速攻を得る。\n・相手ユニット全体に1ダメージを与え、破壊したユニットの数と等しい量だけマナを回復する。";
        }
    }
}
