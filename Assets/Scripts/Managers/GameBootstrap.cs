using UnityEngine;

namespace CardBattle.Managers
{
    /// <summary>
    /// ゲーム起動時にInitializeBattleとターン開始を実行する
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        int _count = 0;
        private void Start()
        {
        }

        void Update()
        {
            _count++;
            if (_count == 10)
            {
                Debug.Log("GameBootstrap Start");
                var gameFlowManager = GameFlowManager.Instance;
                if (gameFlowManager != null)
                {
                    gameFlowManager.StartGame();
                }
            }
        }
    }
}
