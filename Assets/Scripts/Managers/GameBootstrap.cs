using UnityEngine;

namespace CardBattle.Managers
{
    /// <summary>
    /// ゲーム起動時にInitializeBattleとターン開始を実行する
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        private void Start()
        {
            var gameFlowManager = GameFlowManager.Instance;
            if (gameFlowManager != null)
            {
                gameFlowManager.StartGame();
            }
        }
    }
}
