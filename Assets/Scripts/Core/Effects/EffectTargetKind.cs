namespace CardBattle.Core.Effects
{
    /// <summary>
    /// 効果の対象の種類を定義する
    /// </summary>
    public enum EffectTargetKind
    {
        None,
        Unit,
        Player,
        /// <summary>ペアリング対象としてのパートナーカード（ユニットとしてまだ場に出ていない状態）</summary>
        PartnerCard
    }
}
