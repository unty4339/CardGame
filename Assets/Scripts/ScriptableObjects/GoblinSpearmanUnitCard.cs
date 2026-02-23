using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.Managers;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// ゴブリンの槍兵。速攻、登場時「ゴブリン」を1枚手札に加える。
    /// </summary>
    public class GoblinSpearmanUnitCard : UnitCardTemplateBase, IOnSummonEffect
    {
        public GoblinSpearmanUnitCard()
        {
            cardName = "ゴブリンの槍兵";
            playCost = 2;
            baseHP = 2;
            baseAttack = 2;
            keywords.Add(KeywordAbility.Rush);
            description = "速攻\n登場時：\n「ゴブリン」を1枚、手札に加える。";
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit)
        {
            return new List<EffectTarget>();
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit)
        {
            PlayerManager.Instance?.AddCardToHand(state.MyPlayerId, new GoblinUnitCard());
        }
    }
}
