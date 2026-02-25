using CardBattle.Core.Field;

namespace CardBattle.Core.Effects
{
    /// <summary>
    /// ペアリング対象にパートナー（カード or ユニット）を選んだときに表示する立ち絵を返す。
    /// PairingService.ApplyPairingResult から呼ばれ、非 null/非空のとき StandingPictureManager に渡す。
    /// </summary>
    public interface IPairingStandingPicture
    {
        /// <summary>
        /// パートナーがペア対象に選ばれたときに表示する立ち絵の種類 ID（StandingPictureType 定数）。
        /// 変更しない場合は null または空文字を返す。
        /// </summary>
        /// <param name="target">選択された効果対象</param>
        /// <param name="pairTargetUnitOrNull">ユニットを選んだ場合はそのユニット、パートナーカードの場合は null</param>
        string GetStandingPictureTypeWhenPartnerChosen(EffectTarget target, Unit pairTargetUnitOrNull);
    }
}
