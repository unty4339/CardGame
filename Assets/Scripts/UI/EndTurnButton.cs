using CardBattle.Core;
using CardBattle.Core.Enums;
using CardBattle.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.UI
{
    /// <summary>
    /// クリック時に「ターン終了」用の GameAction を ActionQueueManager のキューに追加する。
    /// 相手ターン中・効果対象選択中・戦闘終了時は押せず、見た目も無効化する。
    /// Button の On Click () に OnEndTurnClicked を指定して使用する。
    /// </summary>
    public class EndTurnButton : MonoBehaviour
    {
        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void Update()
        {
            if (_button == null) return;
            var gameFlow = GameFlowManager.Instance;
            _button.interactable = gameFlow != null
                && !gameFlow.IsGameEnded
                && gameFlow.CurrentPhase == GamePhase.Normal
                && gameFlow.CurrentTurnPlayerId == 0;
        }

        /// <summary>
        /// ボタンクリック時に呼ぶ。ターン終了用の GameAction をキューに追加する。
        /// 相手ターン・対象選択中・戦闘終了時は何もしない。
        /// </summary>
        public void OnEndTurnClicked()
        {
            var gameFlow = GameFlowManager.Instance;
            if (gameFlow == null) return;
            if (gameFlow.IsGameEnded) return;
            if (gameFlow.CurrentPhase != GamePhase.Normal) return;
            if (gameFlow.CurrentTurnPlayerId != 0) return;
            var actionQueueManager = ActionQueueManager.Instance;
            var action = new GameAction { ActionType = ActionType.TurnEnd };
            actionQueueManager.AddAction(action);
        }
    }
}
