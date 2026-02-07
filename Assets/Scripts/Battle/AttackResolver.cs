using System;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.Managers;

namespace CardBattle.Battle
{
    /// <summary>
    /// 攻撃時のダメージ計算と結果の適用について責任を持つ
    /// </summary>
    public class AttackResolver
    {
        /// <summary>
        /// 攻撃側ユニットと防御側ユニットを受け取り、双方の体力を減らし破壊判定を行う
        /// パートナーが破壊された場合はパートナーゾーンに戻す処理を行う
        /// </summary>
        public void ResolveUnitAttack(Unit attacker, Unit defender, FieldZone attackerField, FieldZone defenderField)
        {
            defender.HP -= attacker.Attack;
            attacker.HP -= defender.Attack;

            if (defender.HP <= 0)
            {
                defenderField.Units.Remove(defender);
                if (defender.IsPartner)
                {
                    var partnerManager = PartnerManager.Instance;
                    partnerManager?.ReturnPartnerToZone(defender, defender.OwnerPlayerId);
                }
            }

            if (attacker.HP <= 0)
            {
                attackerField.Units.Remove(attacker);
                if (attacker.IsPartner)
                {
                    var partnerManager = PartnerManager.Instance;
                    partnerManager?.ReturnPartnerToZone(attacker, attacker.OwnerPlayerId);
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

            targetData.HP -= attacker.Attack;
            playerManager.NotifyPlayerDataChanged(targetPlayerId);

            if (targetData.HP <= 0)
            {
                // TODO: 勝利判定へ遷移（GameFlowManager.CheckGameEnd）
            }
        }
    }
}
