namespace CardBattle.Core.Effects
{
    /// <summary>
    /// ペアリング中にペア対象に「攻撃できない」を付与する効果を持つカードを示すマーカー。
    /// ターン開始時の攻撃権付与時に、このカードとペア中のユニットには攻撃権を付与しない。
    /// </summary>
    public interface IGrantsCannotAttackToPairTarget
    {
    }
}
