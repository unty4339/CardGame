namespace CardBattle.Core.Effects
{
    /// <summary>
    /// ペアリング中にこのカードの実効攻撃力が「自身の攻撃力＋ペア対象の攻撃力」になる効果を持つカードを示すマーカー。
    /// 実効攻撃力の算出は PlayerManager.GetEffectiveAttack で行う。
    /// </summary>
    public interface IAddsPairAttackToSelf
    {
    }
}
