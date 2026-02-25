namespace CardBattle.Core.Effects
{
    /// <summary>
    /// ペアリング中に身代わり効果を持つことを示すマーカーインターフェース。
    /// 攻撃対象がこのユニットのとき、ペア対象がフィールド上のユニットならそのユニットを破壊し、
    /// パートナーカードのみとペア中ならペアリングを解除する。いずれの場合もこのユニットへのダメージを一度だけ無効にする。
    /// </summary>
    public interface IWhilePairedSubstitution
    {
    }
}
