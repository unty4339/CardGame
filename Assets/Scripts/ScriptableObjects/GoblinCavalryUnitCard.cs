using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.UI;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// ゴブリンの騎兵。登場時ペア。ペアリング中：攻撃できない付与、攻撃はペアの分だけ増加、戦闘時身代わり。
    /// </summary>
    public class GoblinCavalryUnitCard : UnitCardTemplateBase, IOnPairingEffect, IGrantsCannotAttackToPairTarget, IAddsPairAttackToSelf, IWhilePairedSubstitution, IPairingStandingPicture
    {
        public GoblinCavalryUnitCard()
        {
            cardName = "ゴブリンの騎兵";
            playCost = 3;
            baseHP = 2;
            baseAttack = 1;
            description = "登場時：\nペアリング中でない、自分パートナーか体力1の相手ユニットとペアリングする。\nペアリング中：\nペア対象に「攻撃できない」を付与する。\nこのカードの攻撃はペア対象の攻撃の分だけ増加する。\nこのカードが戦闘を行うとき、ダメージを受ける代わりにペア対象を破壊する。";
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit, bool isPartnerOnField)
        {
            return PairingTargetCandidates.GetStandardPairingTargets(state, isPartnerOnField);
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit, Unit pairTargetUnitOrNull)
        {
            // ペアリングは呼び出し側で双方向に設定済み。攻撃力加算は IAddsPairAttackToSelf、身代わりは IWhilePairedSubstitution で対応。
            if (pairTargetUnitOrNull != null)
            {
                pairTargetUnitOrNull.CanAttackUnit = false;
                pairTargetUnitOrNull.CanAttackPlayer = false;
            }
        }

        public string GetStandingPictureTypeWhenPartnerChosen(EffectTarget target, Unit pairTargetUnitOrNull)
        {
            var isPartnerChosen = (target.Kind == EffectTargetKind.PartnerCard && target.PlayerId == 0)
                || (pairTargetUnitOrNull != null && pairTargetUnitOrNull.IsPartner && pairTargetUnitOrNull.OwnerPlayerId == 0);
            return isPartnerChosen ? StandingPictureType.Riding : null;
        }
    }
}
