using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Managers;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// グリンスキンの呼子笛。「ゴブリン」を2枚手札に加える。
    /// </summary>
    public class GrimskinWhistleSpellCard : SpellCardTemplateBase, ISpellEffect
    {
        public GrimskinWhistleSpellCard()
        {
            cardName = "グリンスキンの呼子笛";
            playCost = 1;
            description = "「ゴブリン」を2枚手札に加える。";
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state)
        {
            return new List<EffectTarget> { EffectTarget.None() };
        }

        public void Resolve(EffectTarget target, GameState state)
        {
            var pm = PlayerManager.Instance;
            if (pm == null) return;
            var goblin = new GoblinUnitCard();
            pm.AddCardToHand(state.MyPlayerId, goblin);
            pm.AddCardToHand(state.MyPlayerId, goblin);
        }
    }
}
