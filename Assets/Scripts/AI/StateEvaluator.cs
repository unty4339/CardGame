namespace CardBattle.AI
{
    /// <summary>
    /// 状態の評価について責任を持つ
    /// </summary>
    public class StateEvaluator
    {
        /// <summary>
        /// GameStateを受け取り、状態評価値を返す
        /// ユニットの数、ステータスなど状態のデータを特定の重みで評価し総和を取る
        /// </summary>
        public float Evaluate(GameState state)
        {
            var score = 0f;

            score += state.MyHP * 10f;
            score -= state.OpponentHP * 10f;
            score += state.MyMP * 5f;
            score -= state.OpponentMP * 5f;

            foreach (var unit in state.MyField.Units)
            {
                score += unit.HP * 2f;
                score += unit.Attack * 3f;
            }

            foreach (var unit in state.OpponentField.Units)
            {
                score -= unit.HP * 2f;
                score -= unit.Attack * 3f;
            }

            return score;
        }
    }
}
