using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;
using CardBattle.Managers;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// ゴブリンの統率者。登場時：自分の場の全ての「ゴブリン」を+1/+1。
    /// </summary>
    public class GoblinCommanderUnitCard : UnitCardTemplateBase, IOnSummonEffect
    {
        public GoblinCommanderUnitCard()
        {
            cardName = "ゴブリンの統率者";
            playCost = 4;
            baseHP = 2;
            baseAttack = 2;
            description = "登場時：\n自分の場の全ての「ゴブリン」ユニットを+1/+1。";
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit)
        {
            return new List<EffectTarget>();
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit)
        {
            if (state?.MyField?.Units == null) return;
            foreach (var u in state.MyField.Units)
            {
                if (u.SourceCardTemplate?.CardName?.Contains("ゴブリン") != true) continue;
                PlayerManager.Instance?.AddUnitAttack(u, 1);
                PlayerManager.Instance?.AddUnitHp(u, 1);
            }
        }
    }
}
