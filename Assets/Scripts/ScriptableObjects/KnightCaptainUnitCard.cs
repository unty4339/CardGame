using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;
using CardBattle.Managers;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 騎士団の隊長。4/3/3。登場時：自分の他のユニット1体を選び、そのユニットは攻撃力+1を得る。
    /// </summary>
    public class KnightCaptainUnitCard : UnitCardTemplateBase, IOnSummonEffect
    {
        public KnightCaptainUnitCard()
        {
            cardName = "騎士団の隊長";
            playCost = 4;
            baseHP = 3;
            baseAttack = 3;
            description = "登場時、自分の他のユニット1体を選び、そのユニットは攻撃力+1を得る。";
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit)
        {
            var list = new List<EffectTarget>();
            if (state?.MyField?.Units == null) return list;
            foreach (var u in state.MyField.Units)
            {
                if (sourceUnit != null && u.InstanceId == sourceUnit.InstanceId) continue;
                list.Add(EffectTarget.Unit(u.InstanceId));
            }
            return list;
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit)
        {
            if (state?.MyField?.Units == null) return;
            if (target.Kind != EffectTargetKind.Unit || target.UnitInstanceId == null) return;
            var unit = state.MyField.Units.Find(u => u.InstanceId == target.UnitInstanceId.Value);
            if (unit == null) return;
            PlayerManager.Instance?.AddUnitAttack(unit, 1);
        }
    }
}
