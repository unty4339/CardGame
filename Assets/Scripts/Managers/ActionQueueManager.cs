using System;
using CardBattle.Core;
using UnityEngine;

namespace CardBattle.Managers
{
    /// <summary>
    /// 行動キューの受け取りと消化、アニメーション再生について責任を持つ
    /// </summary>
    public class ActionQueueManager : MonoBehaviour
    {
        private static ActionQueueManager _instance;
        public static ActionQueueManager Instance => _instance;

        private readonly ActionQueue _actionQueue = new();
        private bool _isBusy;

        /// <summary>
        /// アニメーション再生側が完了時に呼ぶ。次の行動の消化が可能になる。
        /// </summary>
        public void NotifyActionAnimationCompleted()
        {
            _isBusy = false;
        }

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

        private void Update()
        {
            if (_isBusy || _actionQueue.IsEmpty()) return;
            ProcessNextAction();
        }

        /// <summary>
        /// 行動を受け取り、ActionQueueに追加する
        /// </summary>
        public void AddAction(GameAction action)
        {
            if (action != null)
            {
                _actionQueue.Enqueue(action);
            }
        }

        /// <summary>
        /// キューから1つ行動を取り出し、アニメーションを流しつつオブジェクトを操作する
        /// </summary>
        public void ProcessNextAction()
        {
            if (_actionQueue.IsEmpty()) return;

            var action = _actionQueue.Dequeue();
            if (action == null)
                throw new InvalidOperationException("Dequeued action was null.");

            _isBusy = true;

            switch (action.ActionType)
            {
                case Core.Enums.ActionType.Play:
                    ProcessPlayAction(action);
                    break;
                case Core.Enums.ActionType.Attack:
                    ProcessAttackAction(action);
                    break;
                case Core.Enums.ActionType.TurnEnd:
                    ProcessTurnEndAction();
                    break;
            }
        }

        private void ProcessTurnEndAction()
        {
            var gameFlowManager = GameFlowManager.Instance;
            if (gameFlowManager != null)
                gameFlowManager.EndTurn(gameFlowManager.CurrentTurnPlayerId);
            NotifyActionAnimationCompleted();
        }

        private void ProcessPlayAction(GameAction action)
        {
            if (action.SourceCard != null)
            {
                var playerManager = PlayerManager.Instance;
                if (playerManager == null)
                    throw new InvalidOperationException("PlayerManager.Instance is null.");

                var ownerId = 0;
                for (var i = 0; i <= 1; i++)
                {
                    var data = playerManager.GetPlayerData(i);
                    if (data?.Hand?.Cards != null && data.Hand.Cards.Contains(action.SourceCard))
                    {
                        ownerId = i;
                        break;
                    }
                }

                var ownerData = playerManager.GetPlayerData(ownerId);
                if (ownerData == null)
                    throw new InvalidOperationException($"Owner data not found for card. OwnerId={ownerId}");

                var template = action.SourceCard.Template;
                if (ownerData.CurrentMP < template.PlayCost)
                {
                    NotifyActionAnimationCompleted();
                    return;
                }

                ownerData.CurrentMP -= template.PlayCost;
                playerManager.NotifyPlayerDataChanged(ownerId);
                ownerData.Hand.Cards.Remove(action.SourceCard);

                if (template.CardType == Core.Enums.CardType.Unit)
                {
                    var unitManager = UnitManager.Instance;
                    unitManager?.SpawnUnitFromCard(action.SourceCard, ownerId, ownerData.FieldZone);
                }
                else if (template.CardType == Core.Enums.CardType.Totem)
                {
                    var unitManager = UnitManager.Instance;
                    unitManager?.SpawnTotemFromCard(action.SourceCard, ownerId, ownerData.FieldZone);
                }
                else if (template.CardType == Core.Enums.CardType.Spell)
                {
                }

                var dialogueManager = DialogueManager.Instance;
                dialogueManager?.OnCardPlayed(action.SourceCard);
            }
            NotifyActionAnimationCompleted();
        }

        private void ProcessAttackAction(GameAction action)
        {
            if (action.SourceUnit != null)
            {
                var battleManager = Battle.BattleManager.Instance;
                battleManager?.ExecuteAttack(action.SourceUnit, action.Target);
            }
            NotifyActionAnimationCompleted();
        }
    }
}
