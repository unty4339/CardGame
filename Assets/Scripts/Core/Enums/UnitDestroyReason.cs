namespace CardBattle.Core.Enums
{
    /// <summary>
    /// ユニットが破壊された原因（台詞の出し分け用）。
    /// </summary>
    public enum UnitDestroyReason
    {
        /// <summary>戦闘で体力0になった</summary>
        Battle,

        /// <summary>苗床のターン開始時効果で破壊された</summary>
        Nursery,

        /// <summary>騎兵・肉鎧のオーク等の身代わり効果で破壊された</summary>
        Substitution
    }
}
