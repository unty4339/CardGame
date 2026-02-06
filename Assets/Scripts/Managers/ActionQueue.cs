using System.Collections.Generic;
using CardBattle.Core;

namespace CardBattle.Managers
{
    /// <summary>
    /// 行動のキューについて責任を持つ
    /// プレイヤーまたはAIの操作をFIFOで保持する
    /// </summary>
    public class ActionQueue
    {
        private readonly Queue<GameAction> _queue = new();

        public void Enqueue(GameAction action)
        {
            _queue.Enqueue(action);
        }

        public GameAction Dequeue()
        {
            return _queue.Dequeue();
        }

        public bool IsEmpty()
        {
            return _queue.Count == 0;
        }
    }
}
