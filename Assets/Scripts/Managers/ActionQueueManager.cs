using System;
using CardBattle.Core;
using CardBattle.Core.Field;
using CardBattle.UI;
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
        /// 現在アクション処理中かどうか。攻撃ドラッグ開始可否の判定に使用する。
        /// </summary>
        public bool IsBusy => _isBusy;

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

                if (template.CardType == Core.Enums.CardType.Unit)
                {
                    playerManager.TryPlayCard(ownerId, action.SourceCard);
                }
                else
                {
                    ownerData.CurrentMP -= template.PlayCost;
                    playerManager.NotifyPlayerDataChanged(ownerId);
                    ownerData.Hand.Cards.Remove(action.SourceCard);

                    if (template.CardType == Core.Enums.CardType.Totem)
                    {
                        var unitManager = UnitManager.Instance;
                        unitManager?.SpawnTotemFromCard(action.SourceCard, ownerId, ownerData.FieldZone);
                    }
                }

                var dialogueManager = DialogueManager.Instance;
                dialogueManager?.OnCardPlayed(action.SourceCard);
            }
            NotifyActionAnimationCompleted();
        }

        private void ProcessAttackAction(GameAction action)
        {
            if (action.SourceUnit == null)
            {
                NotifyActionAnimationCompleted();
                return;
            }

            var playerManager = PlayerManager.Instance;
            var battleManager = Battle.BattleManager.Instance;

            // AI の GameAction はクローン Unit を指すことがあるため、InstanceId で本物の Unit に解決する
            var rawAttacker = action.SourceUnit;
            var attacker = playerManager.GetUnitByInstanceId(rawAttacker.OwnerPlayerId, rawAttacker.InstanceId);

            object target;
            if (action.Target is Unit targetUnit)
            {
                target = playerManager.GetUnitByInstanceId(targetUnit.OwnerPlayerId, targetUnit.InstanceId);
            }
            else
            {
                target = action.Target;
            }

            var gameVisual = GameVisualManager.Instance;
            if (gameVisual != null)
            {
                gameVisual.PlayAttackAndResolve(attacker, target, () =>
                {
                    battleManager?.ExecuteAttack(attacker, target);
                    NotifyActionAnimationCompleted();
                });
            }
            else
            {
                battleManager?.ExecuteAttack(attacker, target);
                NotifyActionAnimationCompleted();
            }
        }
    }
}
