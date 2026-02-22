namespace CardBattle.Core.Effects
{
    /// <summary>
    /// ペアリング中に攻撃を肩代わりすることを示すマーカーインターフェース。
    /// Unit の SourceCardTemplate がこれを実装している場合、ペア対象がフィールド上のユニットのときのみ、
    /// 攻撃対象がこのユニットのときにダメージ先がペア対象に振り替えられる。
    /// パートナーカードとだけペア中（PairingWithPartnerCard == true）のときは肩代わりしない。
    /// </summary>
    public interface IWhilePairedSubstitution
    {
    }
}
