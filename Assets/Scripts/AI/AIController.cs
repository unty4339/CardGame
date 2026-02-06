using System;
using System.Collections.Generic;
using CardBattle.Core;
using CardBattle.Core.Deck;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.Managers;
using UnityEngine;

namespace CardBattle.AI
{
    /// <summary>
    /// AIの思考と行動決定について責任を持つ
    /// </summary>
    public class AIController : MonoBehaviour
    {
        private static AIController _instance;
        public static AIController Instance => _instance;

        private StateEvaluator _stateEvaluator;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            _stateEvaluator = new StateEvaluator();
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        /// <summary>
        /// プレイヤーIDを受け取り、現在のデッキ・手札・ゾーンの内容からGameStateを作成する
        /// </summary>
        public GameState CreateState(int playerId)
        {
            var playerManager = PlayerManager.Instance;
            if (playerManager == null)
                throw new InvalidOperationException("PlayerManager.Instance is null.");

            var myData = playerManager.GetPlayerData(playerId);
            var opponentId = playerId == 0 ? 1 : 0;
            var opponentData = playerManager.GetPlayerData(opponentId);
            if (myData == null || opponentData == null)
                throw new InvalidOperationException($"Player data is null. my={myData != null}, opponent={opponentData != null}");

            var state = new GameState
            {
                MyPlayerId = playerId,
                OpponentPlayerId = opponentId,
                MyHand = new List<Card>(myData.Hand.Cards),
                MyField = CloneFieldZone(myData.FieldZone),
                OpponentField = CloneFieldZone(opponentData.FieldZone),
                MyHP = myData.HP,
                OpponentHP = opponentData.HP,
                MyMP = myData.CurrentMP,
                OpponentMP = opponentData.CurrentMP
            };

            return state;
        }

        /// <summary>
        /// GameStateを受け取り、行動一覧を作成する
        /// </summary>
        public List<GameAction> GetAvailableActions(GameState state)
        {
            var actions = new List<GameAction>();

            foreach (var card in state.MyHand)
            {
                if (card.Template.PlayCost <= state.MyMP)
                {
                    actions.Add(new GameAction
                    {
                        ActionType = ActionType.Play,
                        SourceCard = card,
                        Target = null
                    });
                }
            }

            foreach (var unit in state.MyField.Units)
            {
                if (!unit.CanAttack) continue;

                var canAttackPlayer = state.OpponentField.Units.TrueForAll(u => !u.Keywords.Contains(Core.Enums.KeywordAbility.Guard));
                var canAttackThisTurn = unit.TurnsOnField > 0 ||
                    unit.Keywords.Contains(Core.Enums.KeywordAbility.Rush) ||
                    unit.Keywords.Contains(Core.Enums.KeywordAbility.DivineSpeed);

                if (canAttackThisTurn)
                {
                    foreach (var target in state.OpponentField.Units)
                    {
                        actions.Add(new GameAction
                        {
                            ActionType = ActionType.Attack,
                            SourceUnit = unit,
                            Target = target
                        });
                    }

                    if (canAttackPlayer && (unit.TurnsOnField > 0 || unit.Keywords.Contains(Core.Enums.KeywordAbility.DivineSpeed)))
                    {
                        actions.Add(new GameAction
                        {
                            ActionType = ActionType.Attack,
                            SourceUnit = unit,
                            Target = state.OpponentPlayerId
                        });
                    }
                }
            }

            return actions;
        }

        /// <summary>
        /// GameStateとGameActionを受け取り、その行動を取った時の次のGameStateを作成する
        /// 簡易版の実装（完全なシミュレーションは後続で拡張）
        /// </summary>
        public GameState SimulateNextState(GameState state, GameAction action)
        {
            var nextState = CloneState(state);

            if (action.ActionType == ActionType.Play && action.SourceCard != null)
            {
                nextState.MyHand.RemoveAll(c => c.CardID == action.SourceCard.CardID);
                nextState.MyMP -= action.SourceCard.Template.PlayCost;
                // ユニット/トーテムの登場処理は簡略化
            }
            else if (action.ActionType == ActionType.Attack && action.SourceUnit != null)
            {
                var attackerId = action.SourceUnit.InstanceId;
                var attacker = nextState.MyField.Units.Find(u => u.InstanceId == attackerId);
                if (attacker != null) attacker.CanAttack = false;

                if (action.Target is Unit targetUnit)
                {
                    var defender = nextState.OpponentField.Units.Find(u => u.InstanceId == targetUnit.InstanceId);
                    if (attacker != null && defender != null)
                    {
                        defender.HP -= attacker.Attack;
                        attacker.HP -= defender.Attack;
                        if (defender.HP <= 0) nextState.OpponentField.Units.Remove(defender);
                        if (attacker.HP <= 0) nextState.MyField.Units.Remove(attacker);
                    }
                }
                else if (action.Target is int targetPlayerId && targetPlayerId == nextState.OpponentPlayerId && attacker != null)
                {
                    nextState.OpponentHP -= attacker.Attack;
                }
            }

            return nextState;
        }

        /// <summary>
        /// プレイヤーIDを受け取り、状態評価値を最大化するような行動の使用順を返す
        /// </summary>
        public List<GameAction> DecideActions(int playerId)
        {
            var currentState = CreateState(playerId);

            var actionSequence = new List<GameAction>();

            while (true)
            {
                var actions = GetAvailableActions(currentState);
                if (actions.Count == 0) break;

                GameAction bestAction = null;
                var bestScore = float.MinValue;

                foreach (var action in actions)
                {
                    var nextState = SimulateNextState(currentState, action);
                    var score = _stateEvaluator.Evaluate(nextState);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestAction = action;
                    }
                }

                if (bestAction == null) break;

                actionSequence.Add(bestAction);
                currentState = SimulateNextState(currentState, bestAction);
            }

            return actionSequence;
        }

        private static FieldZone CloneFieldZone(FieldZone zone)
        {
            var clone = new FieldZone();
            foreach (var u in zone.Units)
            {
                var unitClone = new Unit
                {
                    HP = u.HP,
                    Attack = u.Attack,
                    TurnsOnField = u.TurnsOnField,
                    CanAttack = u.CanAttack,
                    Keywords = new List<Core.Enums.KeywordAbility>(u.Keywords),
                    InstanceId = u.InstanceId,
                    OwnerPlayerId = u.OwnerPlayerId,
                    IsPartner = u.IsPartner
                };
                clone.Units.Add(unitClone);
            }
            foreach (var t in zone.Totems)
            {
                clone.Totems.Add(new Totem { OwnerPlayerId = t.OwnerPlayerId });
            }
            return clone;
        }

        private static GameState CloneState(GameState state)
        {
            return new GameState
            {
                MyPlayerId = state.MyPlayerId,
                OpponentPlayerId = state.OpponentPlayerId,
                MyHand = new List<Card>(state.MyHand),
                MyField = CloneFieldZone(state.MyField),
                OpponentField = CloneFieldZone(state.OpponentField),
                MyHP = state.MyHP,
                OpponentHP = state.OpponentHP,
                MyMP = state.MyMP,
                OpponentMP = state.OpponentMP
            };
        }
    }
}
