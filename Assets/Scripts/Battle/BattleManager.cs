using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using UnityEngine;

namespace CardBattle.Battle
{
    /// <summary>
    /// 戦闘の実行と攻撃ルールの適用について責任を持つ
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        private static BattleManager _instance;
        public static BattleManager Instance => _instance;

        private AttackResolver _attackResolver;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            _attackResolver = new AttackResolver();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        /// <summary>
        /// 攻撃側ユニット、対象ユニット、相手フィールドを受け取り、ユニットへの攻撃が可能か判定する
        /// </summary>
        public bool CanAttackUnit(Unit attacker, Unit target, FieldZone opponentField)
        {
            if (!attacker.CanAttack) return false;

            var canAttackThisTurn = attacker.TurnsOnField > 0 ||
                attacker.Keywords.Contains(KeywordAbility.Rush) ||
                attacker.Keywords.Contains(KeywordAbility.DivineSpeed);

            return canAttackThisTurn;
        }

        /// <summary>
        /// 攻撃側ユニット、相手フィールドを受け取り、プレイヤーへの攻撃が可能か判定する
        /// </summary>
        public bool CanAttackPlayer(Unit attacker, FieldZone opponentField)
        {
            if (!attacker.CanAttack) return false;

            var hasGuard = false;
            foreach (var unit in opponentField.Units)
            {
                if (unit.Keywords.Contains(KeywordAbility.Guard))
                {
                    hasGuard = true;
                    break;
                }
            }
            if (hasGuard) return false;

            var canAttackPlayerThisTurn = attacker.TurnsOnField > 0 ||
                attacker.Keywords.Contains(KeywordAbility.DivineSpeed);

            return canAttackPlayerThisTurn;
        }

        /// <summary>
        /// 攻撃側ユニットと攻撃対象を受け取り、攻撃を実行する
        /// </summary>
        public void ExecuteAttack(Unit attacker, object target)
        {
            var playerManager = Managers.PlayerManager.Instance;
            if (playerManager == null) return;

            var attackerPlayerId = attacker.OwnerPlayerId;
            var opponentPlayerId = attackerPlayerId == 0 ? 1 : 0;
            var attackerData = playerManager.GetPlayerData(attackerPlayerId);
            var opponentData = playerManager.GetPlayerData(opponentPlayerId);
            if (attackerData == null || opponentData == null) return;

            if (target is Unit targetUnit)
            {
                if (!CanAttackUnit(attacker, targetUnit, opponentData.FieldZone)) return;
                _attackResolver.ResolveUnitAttack(attacker, targetUnit, attackerData.FieldZone, opponentData.FieldZone);
            }
            else if (target is int targetPlayerId)
            {
                if (!CanAttackPlayer(attacker, opponentData.FieldZone)) return;
                _attackResolver.ResolvePlayerAttack(attacker, targetPlayerId);
            }

            attacker.CanAttack = false;
        }
    }
}
