using CardBattle.Core;
using CardBattle.Core.Enums;
using CardBattle.Managers;
using UnityEngine;

namespace CardBattle.UI
{
    /// <summary>
    /// クリック時に「ターン終了」用の GameAction を ActionQueueManager のキューに追加する。
    /// Button の On Click () に OnEndTurnClicked を指定して使用する。
    /// </summary>
    public class EndTurnButton : MonoBehaviour
    {
        /// <summary>
        /// ボタンクリック時に呼ぶ。ターン終了用の GameAction をキューに追加する。
        /// </summary>
        public void OnEndTurnClicked()
        {
            var gameFlow = GameFlowManager.Instance;
            if (gameFlow != null && gameFlow.CurrentPhase != Core.Enums.GamePhase.Normal)
                return;
            var actionQueueManager = ActionQueueManager.Instance;
            var action = new GameAction { ActionType = ActionType.TurnEnd };
            actionQueueManager.AddAction(action);
        }
    }
}
