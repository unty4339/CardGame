using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Field;

namespace CardBattle.Core.Effects
{
    /// <summary>
    /// ペアリング対象としてよく使う候補（体力1の敵ユニット・パートナーユニット・パートナーカード）を返す共通ヘルパ。
    /// </summary>
    public static class PairingTargetCandidates
    {
        /// <summary>
        /// 「体力1の相手ユニット」「自分パートナーユニット」「パートナーカード」のうち、ペアリング対象に選べるものを返す。
        /// </summary>
        public static IList<EffectTarget> GetStandardPairingTargets(GameState state, bool isPartnerOnField)
        {
            var list = new List<EffectTarget>();
            if (state == null) return list;

            if (state.OpponentField?.Units != null)
            {
                foreach (var u in state.OpponentField.Units)
                {
                    if (u != null && u.HP == 1 && !state.IsAlreadySomeonesPairingTarget(u))
                        list.Add(EffectTarget.Unit(u.InstanceId));
                }
            }

            if (state.MyField?.Units != null && isPartnerOnField)
            {
                foreach (var u in state.MyField.Units)
                {
                    if (u != null && u.IsPartner && !state.IsAlreadySomeonesPairingTarget(u))
                        list.Add(EffectTarget.Unit(u.InstanceId));
                }
            }

            if (!isPartnerOnField && !state.IsPartnerCardAlreadyPairingTarget())
                list.Add(EffectTarget.PartnerCard(state.MyPlayerId));

            return list;
        }
    }
}
