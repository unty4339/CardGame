namespace CardBattle.Core.Enums
{
    /// <summary>
    /// ゲームのフェーズ。ターゲット選択中はカードプレイ・攻撃・ターン終了などを制限する。
    /// </summary>
    public enum GamePhase
    {
        Normal,
        TargetSelection
    }
}
