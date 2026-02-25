using CardBattle.Core.Field;

namespace CardBattle.Core.Effects
{
    /// <summary>
    /// ターン開始時に発動する効果のインターフェース。
    /// </summary>
    public interface ITurnStartEffect
    {
        /// <summary>
        /// ターン開始時の効果を適用する。sourceUnit はこの効果を持つフィールド上のユニット（トーテム含む）。
        /// </summary>
        /// <param name="sourceUnit">効果を持つユニット</param>
        /// <param name="turnPlayerId">ターンプレイヤーID</param>
        void Resolve(Unit sourceUnit, int turnPlayerId);
    }
}
