using CardBattle.Core.Enums;

namespace CardBattle.Core
{
    /// <summary>
    /// ターン中の行動1件の記録。行動内容と、その行動直後の盤面スナップショットを持つ。
    /// </summary>
    public class TurnActionRecord
    {
        public ActionType ActionType { get; set; }
        public int PlayerId { get; set; }

        /// <summary>カードプレイ時のカード名</summary>
        public string CardName { get; set; }

        /// <summary>Play 行動がユニット召喚だった場合は true</summary>
        public bool IsUnitPlay { get; set; }

        /// <summary>攻撃時の攻撃元ユニットの InstanceId</summary>
        public int? AttackerInstanceId { get; set; }

        /// <summary>攻撃先がユニットの場合の InstanceId。リーダー攻撃の場合は null</summary>
        public int? TargetUnitInstanceId { get; set; }

        /// <summary>攻撃先がリーダー（プレイヤー）の場合は true</summary>
        public bool TargetIsLeader { get; set; }

        /// <summary>その行動を実行した直後の盤面</summary>
        public BoardStateSnapshot BoardStateAtAction { get; set; }
    }
}
