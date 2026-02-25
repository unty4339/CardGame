using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;
using CardBattle.Managers;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 騎士団の号令。コスト1。自分のユニット1体を選び、+1/+1する。
    /// </summary>
    public class KnightOrderSpellCard : SpellCardTemplateBase, ISpellEffect
    {
        public KnightOrderSpellCard()
        {
            cardName = "騎士団の号令";
            playCost = 1;
            description = "自分のユニット1体を選び、+1/+1する。";
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state)
        {
            var list = new List<EffectTarget>();
            if (state?.MyField?.Units == null) return list;
            foreach (var u in state.MyField.Units)
                list.Add(EffectTarget.Unit(u.InstanceId));
            return list;
        }

        public void Resolve(EffectTarget target, GameState state)
        {
            if (state?.MyField?.Units == null) return;
            if (target.Kind != EffectTargetKind.Unit || target.UnitInstanceId == null) return;
            var unit = state.MyField.Units.Find(u => u.InstanceId == target.UnitInstanceId.Value);
            if (unit == null) return;
            PlayerManager.Instance?.AddUnitAttack(unit, 1);
            PlayerManager.Instance?.AddUnitHp(unit, 1);
        }
    }
}
