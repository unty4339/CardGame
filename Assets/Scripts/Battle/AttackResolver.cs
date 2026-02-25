using System;
using CardBattle.Core.Effects;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.Managers;
using CardBattle.ScriptableObjects;

namespace CardBattle.Battle
{
    /// <summary>
    /// 攻撃時のダメージ計算と結果の適用について責任を持つ
    /// </summary>
    public class AttackResolver
    {
        /// <summary>
        /// 攻撃側ユニットと防御側ユニットを受け取り、双方の体力を減らし破壊判定を行う。
        /// 身代わり: defender または attacker が IWhilePairedSubstitution を持ちペア中の場合、ペア先を破壊するかペア解除し、そのユニットへのダメージを一度だけ無効にする。
        /// </summary>
        public void ResolveUnitAttack(Unit attacker, Unit defender, FieldZone attackerField, FieldZone defenderField)
        {
            var playerManager = PlayerManager.Instance;

            var defenderSubstitutionActive = defender.SourceCardTemplate is IWhilePairedSubstitution
                && (defender.PairingTarget != null || defender.PairingWithPartnerCard);

            if (defenderSubstitutionActive)
            {
                if (defender.PairingWithPartnerCard)
                {
                    playerManager?.UnpairPartnerCardOnly(defender);
                }
                else
                {
                    var pairTarget = defender.PairingTarget;
                    playerManager?.UnpairIfNeededAndNotifyDestroyed(pairTarget);
                    defenderField.Units.Remove(pairTarget);
                }
            }
            else
            {
                playerManager?.AddUnitHp(defender, -playerManager.GetEffectiveAttack(attacker));
                if (defender.HP <= 0)
                {
                    playerManager?.UnpairIfNeededAndNotifyDestroyed(defender);
                    defenderField.Units.Remove(defender);
                }
            }

            var attackerSubstitutionActive = attacker.SourceCardTemplate is IWhilePairedSubstitution
                && (attacker.PairingTarget != null || attacker.PairingWithPartnerCard);

            if (attackerSubstitutionActive)
            {
                if (attacker.PairingWithPartnerCard)
                {
                    playerManager?.UnpairPartnerCardOnly(attacker);
                }
                else
                {
                    var pairTarget = attacker.PairingTarget;
                    playerManager?.UnpairIfNeededAndNotifyDestroyed(pairTarget);
                    var fieldForPair = pairTarget.OwnerPlayerId == attacker.OwnerPlayerId ? attackerField : defenderField;
                    fieldForPair.Units.Remove(pairTarget);
                }
            }
            else
            {
                playerManager?.AddUnitHp(attacker, -defender.Attack);
                if (attacker.HP <= 0)
                {
                    playerManager?.UnpairIfNeededAndNotifyDestroyed(attacker);
                    attackerField.Units.Remove(attacker);
                }
            }
        }

        /// <summary>
        /// 攻撃側ユニットと対象プレイヤーIDを受け取り、相手HPを減らす
        /// </summary>
        public void ResolvePlayerAttack(Unit attacker, int targetPlayerId)
        {
            var playerManager = PlayerManager.Instance;
            if (playerManager == null)
                throw new InvalidOperationException("PlayerManager.Instance is null.");

            var targetData = playerManager.GetPlayerData(targetPlayerId);
            if (targetData == null)
                throw new InvalidOperationException($"Target player data not found. targetPlayerId={targetPlayerId}");

            targetData.HP -= playerManager.GetEffectiveAttack(attacker);
            playerManager.NotifyPlayerDataChanged(targetPlayerId);

            if (targetData.HP <= 0)
            {
                GameFlowManager.Instance?.RequestGameEnd(1 - targetPlayerId);
            }
        }
    }
}
