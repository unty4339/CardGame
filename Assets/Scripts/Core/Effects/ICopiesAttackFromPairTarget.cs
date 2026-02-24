namespace CardBattle.Core.Effects
{
    /// <summary>
    /// ペアリング中にこのカードの攻撃力がペア対象の攻撃力と等しくなる効果を持つカードを示すマーカー。
    /// 実効攻撃力の算出は PlayerManager.GetEffectiveAttack で行う。
    /// </summary>
    public interface ICopiesAttackFromPairTarget
    {
    }
}
