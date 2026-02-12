using System;
using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;

namespace CardBattle.ScriptableObjects.Effects
{
    /// <summary>
    /// 召喚時：敵ユニット1体を破壊する効果
    /// </summary>
    [Serializable]
    public class DestroyOneEnemyUnitEffect : IOnSummonEffect
    {
        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit)
        {
            var list = new List<EffectTarget>();
            if (state?.OpponentField?.Units == null) return list;
            foreach (var u in state.OpponentField.Units)
                list.Add(EffectTarget.Unit(u.InstanceId));
            return list;
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit)
        {
            if (state?.OpponentField?.Units == null) return;
            if (target.Kind != EffectTargetKind.Unit || target.UnitInstanceId == null) return;
            var idx = state.OpponentField.Units.FindIndex(u => u.InstanceId == target.UnitInstanceId.Value);
            if (idx >= 0)
                state.OpponentField.Units.RemoveAt(idx);
        }
    }
}
