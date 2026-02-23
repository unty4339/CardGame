using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 森の蛮人、オーク。手札中コスト軽減・登場時体力1（コスト軽減とマナ2で2体目は未実装）。部分実装として登場時1体のみ体力1に。
    /// </summary>
    public class ForestBarbarianOgreUnitCard : UnitCardTemplateBase, IOnSummonEffect
    {
        public ForestBarbarianOgreUnitCard()
        {
            cardName = "森の蛮人、オーク";
            playCost = 6;
            baseHP = 4;
            baseAttack = 3;
            description = "手札中：\nこのカードの登場コストはこのターン登場した「ゴブリン」の数だけ減少する（最低1）。\n\n登場時：\n相手のユニットを1体選ぶ。そのユニットの体力を1にする。\nマナを2支払える。そうした場合、相手のユニットを1体選ぶ。そのユニットの体力を1にする。";
        }

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
            var unit = state.OpponentField.Units.Find(u => u.InstanceId == target.UnitInstanceId.Value);
            if (unit != null)
                unit.HP = 1;
        }
    }
}
