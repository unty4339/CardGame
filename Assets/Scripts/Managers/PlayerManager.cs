using System;
using System.Collections.Generic;
using CardBattle.Core;
using CardBattle.Core.Deck;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.Core.Player;
using UnityEngine;

namespace CardBattle.Managers
{
    /// <summary>
    /// プレイヤー情報の管理とゾーン操作について責任を持つ
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        private static PlayerManager _instance;
        public static PlayerManager Instance => _instance;

        /// <summary>
        /// カードを引いたときに発火する。プレイヤーIDと引いたCardが渡される。
        /// </summary>
        public event Action<int, Card> OnCardDrawn;

        /// <summary>
        /// ユニット召喚に成功したときに発火する。プレイヤーID、プレイしたCard、生成されたUnitが渡される。
        /// </summary>
        public event Action<int, Card, Unit> OnUnitSummoned;

        private readonly Dictionary<int, PlayerData> _players = new();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        public PlayerData GetPlayerData(int playerId)
        {
            return _players.TryGetValue(playerId, out var data) ? data : null;
        }

        /// <summary>
        /// プレイヤーデータを登録する
        /// </summary>
        public void RegisterPlayer(int playerId, PlayerData playerData)
        {
            _players[playerId] = playerData;
        }

        /// <summary>
        /// プレイヤーIDを受け取り、デッキの一番上からカードを手札に加える
        /// </summary>
        public bool DrawCard(int playerId)
        {
            var data = GetPlayerData(playerId);
            if (data == null) return false;

            if (data.Deck.Cards.Count == 0)
            {
                // TODO: 敗北処理へ遷移
                return false;
            }

            var card = data.Deck.Cards[0];
            data.Deck.Cards.RemoveAt(0);
            data.Hand.Cards.Add(card);
            OnCardDrawn?.Invoke(playerId, card);
            return true;
        }

        /// <summary>
        /// プレイヤーIDとカードを受け取り、そのカードをプレイしてユニットを召喚する。成功時は true、不成立時は false を返す。
        /// </summary>
        public bool TryPlayCard(int playerId, Card card)
        {
            var data = GetPlayerData(playerId);
            if (data == null || card == null || card.Template == null) return false;
            if (card.Template.CardType != CardType.Unit) return false;
            if (!data.Hand.Cards.Contains(card)) return false;
            if (data.CurrentMP < card.Template.PlayCost) return false;

            data.Hand.Cards.Remove(card);
            data.CurrentMP -= card.Template.PlayCost;

            var unit = UnitManager.Instance?.SpawnUnitFromCard(card, playerId, data.FieldZone);
            if (unit != null)
                OnUnitSummoned?.Invoke(playerId, card, unit);
            return true;
        }

        /// <summary>
        /// プレイヤーIDとカードを受け取り、そのカードの行動一覧を返す
        /// </summary>
        public List<GameAction> GetCardActions(int playerId, Card card)
        {
            var data = GetPlayerData(playerId);
            if (data == null || card == null) return new List<GameAction>();

            return new List<GameAction>(card.AvailableActions);
        }

        /// <summary>
        /// プレイヤーIDとユニットを受け取り、そのユニットの行動一覧を返す
        /// </summary>
        public List<GameAction> GetUnitActions(int playerId, Unit unit)
        {
            var data = GetPlayerData(playerId);
            if (data == null || unit == null) return new List<GameAction>();

            var actions = new List<GameAction>();
            if (unit.CanAttack)
            {
                var opponentId = playerId == 0 ? 1 : 0;
                var opponentData = GetPlayerData(opponentId);
                if (opponentData != null)
                {
                    var battleManager = Battle.BattleManager.Instance;
                    foreach (var target in opponentData.FieldZone.Units)
                    {
                        if (battleManager != null && battleManager.CanAttackUnit(unit, target, opponentData.FieldZone))
                        {
                            actions.Add(new GameAction
                            {
                                ActionType = ActionType.Attack,
                                SourceUnit = unit,
                                Target = target
                            });
                        }
                    }
                    if (battleManager != null && battleManager.CanAttackPlayer(unit, opponentData.FieldZone))
                    {
                        actions.Add(new GameAction
                        {
                            ActionType = ActionType.Attack,
                            SourceUnit = unit,
                            Target = opponentId
                        });
                    }
                }
            }
            return actions;
        }

        /// <summary>
        /// プレイヤーIDとカードを受け取り、効果で手札に新たなカードを加える
        /// </summary>
        public void AddCardToHand(int playerId, Card card)
        {
            var data = GetPlayerData(playerId);
            if (data != null && card != null)
            {
                data.Hand.Cards.Add(card);
            }
        }

        /// <summary>
        /// 最大MPを+1する
        /// </summary>
        public void IncreaseMaxMP(int playerId)
        {
            var data = GetPlayerData(playerId);
            if (data != null)
            {
                data.MaxMP++;
            }
        }

        /// <summary>
        /// 現在MPを最大MPで全回復する
        /// </summary>
        public void RestoreMP(int playerId)
        {
            var data = GetPlayerData(playerId);
            if (data != null)
            {
                data.CurrentMP = data.MaxMP;
            }
        }

        /// <summary>
        /// フィールドの全ユニットに攻撃権を付与する
        /// </summary>
        public void GrantAttackToAllUnits(int playerId)
        {
            var data = GetPlayerData(playerId);
            if (data == null) return;

            foreach (var unit in data.FieldZone.Units)
            {
                unit.CanAttack = true;
            }
        }

        /// <summary>
        /// 経過ターン数を加算する
        /// </summary>
        public void IncrementTurnsOnField(int playerId)
        {
            var data = GetPlayerData(playerId);
            if (data == null) return;

            foreach (var unit in data.FieldZone.Units)
            {
                unit.TurnsOnField++;
            }
        }

        /// <summary>
        /// マリガン：手札のカードをデッキに戻しシャッフルして引き直す
        /// </summary>
        public void Mulligan(int playerId, List<Card> cardsToReturn)
        {
            var data = GetPlayerData(playerId);
            if (data == null || cardsToReturn == null) return;

            foreach (var card in cardsToReturn)
            {
                data.Hand.Cards.Remove(card);
                data.Deck.Cards.Add(card);
            }

            DeckBuilder.Shuffle(data.Deck.Cards);

            foreach (var _ in cardsToReturn)
            {
                DrawCard(playerId);
            }
        }
    }
}
