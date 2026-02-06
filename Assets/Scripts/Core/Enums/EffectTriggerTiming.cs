namespace CardBattle.Core.Enums
{
    /// <summary>
    /// 効果の発動タイミングを定義する
    /// </summary>
    public enum EffectTriggerTiming
    {
        InHand,
        OnSummon,
        WhileOnField,
        OnDestroy,
        OnPairing,
        WhilePaired,
        OnUnpair
    }
}
