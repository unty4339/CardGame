using System;
using CardBattle.Core;
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

        /// <summary>
        /// 現在のゲームフェーズ。ターゲット選択中は通常操作を制限する。
        /// </summary>
        public Core.Enums.GamePhase CurrentPhase => _currentPhase;

        private int _firstPlayer;
        private int _currentTurnPlayer;
        private bool _mulliganDone;
        private Core.Enums.GamePhase _currentPhase = Core.Enums.GamePhase.Normal;

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
        /// ターゲット選択モードに入る。EffectResolver が呼ぶ。
        /// </summary>
        public void EnterTargetSelection()
        {
            _currentPhase = Core.Enums.GamePhase.TargetSelection;
        }

        /// <summary>
        /// ターゲット選択モードを終了する。
        /// </summary>
        public void ExitTargetSelection()
        {
            _currentPhase = Core.Enums.GamePhase.Normal;
        }

        /// <summary>
        /// ダミー用のパートナー（Cost=1, 1/1, キーワードなし）を生成する
        /// </summary>
        private static Partner CreateDummyPartner()
        {
            return new Partner
            {
                Cost = 1,
                BaseHP = 1,
                BaseAttack = 1,
                Keywords = new System.Collections.Generic.List<KeywordAbility>()
            };
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
                var recipe = DeckRecipe.CreateForPlayer(i);
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

                var partner = recipe.PartnerTemplate != null
                    ? recipe.PartnerTemplate.ToPartner()
                    : CreateDummyPartner();
                partnerManager?.PlacePartner(i, partner);
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

            if (turnPlayerId == 1)
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
                    var turnEndAction = new GameAction { ActionType = ActionType.TurnEnd };
                    actionQueueManager?.AddAction(turnEndAction);
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
