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
        /// 肩代わり: defender が IWhilePairedSubstitution を持ち、PairingTarget がフィールド上のユニットのとき、ダメージ先を PairingTarget に振り替える。
        /// パートナーが破壊された場合はパートナーゾーンに戻す処理を行う。
        /// </summary>
        public void ResolveUnitAttack(Unit attacker, Unit defender, FieldZone attackerField, FieldZone defenderField)
        {
            var playerManager = PlayerManager.Instance;

            Unit damageReceiver;
            if (defender.SourceCardTemplate is IWhilePairedSubstitution
                && defender.PairingTarget != null
                && !defender.PairingWithPartnerCard)
            {
                damageReceiver = defender.PairingTarget;
            }
            else
            {
                damageReceiver = defender;
            }

            damageReceiver.HP -= attacker.Attack;
            attacker.HP -= defender.Attack;

            playerManager?.NotifyUnitHpChanged(attacker);
            playerManager?.NotifyUnitHpChanged(damageReceiver);
            if (damageReceiver != defender)
                playerManager?.NotifyUnitHpChanged(defender);

            if (damageReceiver.HP <= 0)
            {
                playerManager?.UnpairIfNeededAndNotifyDestroyed(damageReceiver);
                defenderField.Units.Remove(damageReceiver);
            }

            if (attacker.HP <= 0)
            {
                playerManager?.UnpairIfNeededAndNotifyDestroyed(attacker);
                attackerField.Units.Remove(attacker);
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

            targetData.HP -= attacker.Attack;
            playerManager.NotifyPlayerDataChanged(targetPlayerId);

            if (targetData.HP <= 0)
            {
                // TODO: 勝利判定へ遷移（GameFlowManager.CheckGameEnd）
            }
        }
    }
}
