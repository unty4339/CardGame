using System;
using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;
using UnityEngine;

namespace CardBattle.ScriptableObjects.Effects
{
    /// <summary>
    /// 召喚時：自分のHPを回復する効果。対象選択なし。
    /// </summary>
    [Serializable]
    public class HealSelfEffect : IOnSummonEffect
    {
        [SerializeField] private int healAmount = 2;

        public int HealAmount => healAmount;

        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit)
        {
            return new List<EffectTarget> { EffectTarget.None() };
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit)
        {
            if (state == null) return;
            const int maxHP = 15;
            state.MyHP = Mathf.Min(state.MyHP + healAmount, maxHP);
        }
    }
}
