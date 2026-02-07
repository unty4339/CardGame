using System;
using System.Reflection;
using CardBattle.Core.Deck;
using CardBattle.Core.Enums;
using CardBattle.Core.Partner;
using CardBattle.Core.Player;
using CardBattle.ScriptableObjects;
using UnityEngine;

namespace CardBattle.Managers
{
    /// <summary>
    /// ゲーム全体のフロー管理について責任を持つ
    /// </summary>
    public class GameFlowManager : MonoBehaviour
    {
        private static GameFlowManager _instance;
        public static GameFlowManager Instance => _instance;

        /// <summary>
        /// 現在ターン中のプレイヤーID
        /// </summary>
        public int CurrentTurnPlayerId => _currentTurnPlayer;

        [SerializeField] private DeckRecipe player0DeckRecipe;
        [SerializeField] private DeckRecipe player1DeckRecipe;
        [SerializeField] private Partner player0Partner;
        [SerializeField] private Partner player1Partner;

        private int _firstPlayer;
        private int _currentTurnPlayer;
        private bool _mulliganDone;

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

        /// <summary>
        /// コスト1・1/1・効果なしのユニットカード30枚のダミーデッキレシピをランタイムで生成する
        /// </summary>
        private static DeckRecipe CreateDummyDeckRecipe()
        {
            var unitData = ScriptableObject.CreateInstance<UnitData>();
            SetPrivateField(unitData, "baseHP", 1);
            SetPrivateField(unitData, "baseAttack", 1);

            var cardTemplate = ScriptableObject.CreateInstance<CardTemplate>();
            SetPrivateField(cardTemplate, "cardType", CardType.Unit);
            SetPrivateField(cardTemplate, "playCost", 1);
            SetPrivateField(cardTemplate, "unitData", unitData);

            var deckRecipe = ScriptableObject.CreateInstance<DeckRecipe>();
            var entries = GetPrivateField<System.Collections.Generic.List<DeckRecipeEntry>>(deckRecipe, "entries");
            entries.Add(new DeckRecipeEntry { Template = cardTemplate, Count = 30 });

            return deckRecipe;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field?.GetValue(target);
        }

        /// <summary>
        /// 戦闘を初期化する
        /// </summary>
        public void InitializeBattle()
        {
            _firstPlayer = UnityEngine.Random.Range(0, 2);
            _mulliganDone = false;

            var playerManager = PlayerManager.Instance;
            var partnerManager = PartnerManager.Instance;
            if (playerManager == null)
                throw new InvalidOperationException("PlayerManager.Instance is null.");

            for (var i = 0; i < 2; i++)
            {
                var recipe = i == 0 ? player0DeckRecipe : player1DeckRecipe;
                if (recipe == null)
                {
                    recipe = CreateDummyDeckRecipe();
                }
                var deck = DeckBuilder.BuildDeck(recipe);

                var playerData = new PlayerData
                {
                    HP = 15,
                    MaxMP = 0,
                    CurrentMP = 0,
                    Deck = deck,
                    Hand = new Hand(),
                    FieldZone = new Core.Field.FieldZone(),
                    PartnerZone = new Core.Partner.PartnerZone()
                };

                playerManager.RegisterPlayer(i, playerData);

                for (var j = 0; j < 5; j++)
                {
                    playerManager.DrawCard(i);
                }

                var partner = i == 0 ? player0Partner : player1Partner;
                if (partner != null)
                {
                    partnerManager?.PlacePartner(i, partner);
                }
            }

            playerManager.NotifyPlayerDataChanged(0);
            playerManager.NotifyPlayerDataChanged(1);

            // TODO: マリガン処理
            _mulliganDone = true;
        }

        /// <summary>
        /// 戦闘を初期化し、先攻プレイヤーのターンを開始する
        /// </summary>
        public void StartGame()
        {
            InitializeBattle();
            StartTurn(_firstPlayer);
        }

        /// <summary>
        /// ターンプレイヤーIDを受け取り、ターン開始処理を行う
        /// </summary>
        public void StartTurn(int turnPlayerId)
        {
            var playerManager = PlayerManager.Instance;
            if (playerManager == null)
                throw new InvalidOperationException("PlayerManager.Instance is null.");

            if (!playerManager.DrawCard(turnPlayerId))
            {
                return;
            }

            playerManager.IncreaseMaxMP(turnPlayerId);
            playerManager.RestoreMP(turnPlayerId);
            playerManager.GrantAttackToAllUnits(turnPlayerId);
            playerManager.IncrementTurnsOnField(turnPlayerId);

            _currentTurnPlayer = turnPlayerId;

            if (turnPlayerId == _firstPlayer)
            {
                var aiController = AI.AIController.Instance;
                if (aiController != null)
                {
                    var actions = aiController.DecideActions(turnPlayerId);
                    var actionQueueManager = ActionQueueManager.Instance;
                    foreach (var action in actions)
                    {
                        actionQueueManager?.AddAction(action);
                    }
                }
            }
        }

        /// <summary>
        /// ターンプレイヤーIDを受け取り、ターン終了処理を行う
        /// </summary>
        public void EndTurn(int turnPlayerId)
        {
            var nextPlayer = turnPlayerId == 0 ? 1 : 0;
            StartTurn(nextPlayer);
        }

        /// <summary>
        /// 終了条件を判定し、勝敗があれば返す
        /// </summary>
        public int? CheckGameEnd()
        {
            var playerManager = PlayerManager.Instance;
            if (playerManager == null)
                throw new InvalidOperationException("PlayerManager.Instance is null.");

            var p0 = playerManager.GetPlayerData(0);
            var p1 = playerManager.GetPlayerData(1);
            if (p0 == null || p1 == null)
                throw new InvalidOperationException($"Player data is null. p0={p0 != null}, p1={p1 != null}");

            if (p0.HP <= 0) return 1;
            if (p1.HP <= 0) return 0;

            return null;
        }
    }
}
