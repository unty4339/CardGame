namespace CardBattle.Core.Enums
{
    /// <summary>
    /// キーワード能力を定義する
    /// </summary>
    public enum KeywordAbility
    {
        /// <summary>登場してから0ターン経過時でも相手ユニットに攻撃できる</summary>
        Rush,

        /// <summary>登場してから0ターン経過時でも相手ユニット・相手プレイヤーに攻撃できる</summary>
        DivineSpeed,

        /// <summary>守護がいる限り相手はプレイヤーを攻撃対象にできない</summary>
        Guard
    }
}
