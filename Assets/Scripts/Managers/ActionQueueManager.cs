using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardBattle.AI;
using CardBattle.Core;
using CardBattle.Core.Deck;
using CardBattle.Core.Effects;
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
                    playerManager.TryPlayCard(ownerId, action.SourceCard, () =>
                    {
                        var dialogue = DialogueManager.Instance;
                        dialogue?.OnCardPlayed(action.SourceCard);
                        NotifyActionAnimationCompleted();
                    });
                    return;
                }
                else
                {
                    ownerData.CurrentMP -= template.PlayCost;
                    playerManager.NotifyPlayerDataChanged(ownerId);
                    ownerData.Hand.Cards.Remove(action.SourceCard);

                    if (template.CardType == Core.Enums.CardType.Spell)
                    {
                        var spellEffect = template as ISpellEffect;
                        if (spellEffect != null)
                        {
                            var opponentId = ownerId == 0 ? 1 : 0;
                            var myData = ownerData;
                            var oppData = playerManager.GetPlayerData(opponentId);
                            if (oppData != null)
                            {
                                var state = new GameState
                                {
                                    MyPlayerId = ownerId,
                                    OpponentPlayerId = opponentId,
                                    MyField = myData.FieldZone,
                                    OpponentField = oppData.FieldZone,
                                    MyHand = new List<Card>(myData.Hand.Cards),
                                    MyHP = myData.HP,
                                    OpponentHP = oppData.HP,
                                    MyMP = myData.CurrentMP,
                                    OpponentMP = oppData.CurrentMP
                                };
                                var opponentUnitsBefore = new List<Unit>(state.OpponentField.Units);
                                var myUnitsBefore = new List<Unit>(state.MyField.Units);

                                var choices = spellEffect.GetAvailableTargets(state);
                                var needTargetSelection = EffectResolver.Instance != null
                                    && ownerId == 0
                                    && choices != null
                                    && choices.Count > 1;

                                if (needTargetSelection)
                                {
                                    var ctx = new SpellTargetSelectionContext
                                    {
                                        Action = action,
                                        OwnerId = ownerId,
                                        OpponentId = opponentId,
                                        State = state,
                                        SpellEffect = spellEffect,
                                        OpponentUnitsBefore = opponentUnitsBefore,
                                        MyUnitsBefore = myUnitsBefore
                                    };
                                    StartCoroutine(SpellWithTargetSelectionCoroutine(ctx, choices));
                                    return;
                                }

                                var target = EffectResolver.Instance != null
                                    ? EffectResolver.Instance.RequestTargetAsync(choices, ownerId).GetAwaiter().GetResult()
                                    : (choices != null && choices.Count > 0 ? choices[0] : EffectTarget.None());

                                ApplySpellResolution(ownerId, opponentId, state, spellEffect, target,
                                    opponentUnitsBefore, myUnitsBefore);
                            }
                            playerManager.NotifySpellPlayed(ownerId, action.SourceCard);
                        }
                    }
                    else if (template.CardType == Core.Enums.CardType.Totem)
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

        private struct SpellTargetSelectionContext
        {
            public GameAction Action;
            public int OwnerId;
            public int OpponentId;
            public GameState State;
            public ISpellEffect SpellEffect;
            public List<Unit> OpponentUnitsBefore;
            public List<Unit> MyUnitsBefore;
        }

        private IEnumerator SpellWithTargetSelectionCoroutine(SpellTargetSelectionContext ctx, IList<EffectTarget> choices)
        {
            var task = EffectResolver.Instance.RequestTargetAsync(choices, ctx.OwnerId);
            while (!task.IsCompleted)
                yield return null;

            var target = task.GetAwaiter().GetResult();
            var playerManager = PlayerManager.Instance;
            ApplySpellResolution(ctx.OwnerId, ctx.OpponentId, ctx.State, ctx.SpellEffect, target,
                ctx.OpponentUnitsBefore, ctx.MyUnitsBefore);
            playerManager?.NotifySpellPlayed(ctx.OwnerId, ctx.Action.SourceCard);
            var dialogueManager = DialogueManager.Instance;
            dialogueManager?.OnCardPlayed(ctx.Action.SourceCard);
            NotifyActionAnimationCompleted();
        }

        private static void ApplySpellResolution(
            int ownerId,
            int opponentId,
            GameState state,
            ISpellEffect spellEffect,
            EffectTarget target,
            List<Unit> opponentUnitsBefore,
            List<Unit> myUnitsBefore)
        {
            var playerManager = PlayerManager.Instance;
            if (playerManager == null) return;

            if (target.Kind == EffectTargetKind.Unit && target.UnitInstanceId != null)
                GameVisualManager.Instance?.PlayEffectAtUnit(opponentId, target.UnitInstanceId.Value);
            spellEffect.Resolve(target, state);

            var myData = playerManager.GetPlayerData(ownerId);
            var oppData = playerManager.GetPlayerData(opponentId);
            if (myData != null) myData.HP = state.MyHP;
            if (oppData != null) oppData.HP = state.OpponentHP;
            playerManager.NotifyPlayerDataChanged(ownerId);
            playerManager.NotifyPlayerDataChanged(opponentId);

            foreach (var u in opponentUnitsBefore)
            {
                if (!state.OpponentField.Units.Any(x => x.InstanceId == u.InstanceId))
                    playerManager.NotifyUnitDestroyed(u);
            }
            if (target.Kind == EffectTargetKind.Unit && target.UnitInstanceId != null)
            {
                var damagedUnit = state.OpponentField.Units.Find(x => x.InstanceId == target.UnitInstanceId.Value);
                if (damagedUnit != null)
                    playerManager.NotifyUnitHpChanged(damagedUnit);
            }
            foreach (var u in myUnitsBefore)
            {
                if (!state.MyField.Units.Any(x => x.InstanceId == u.InstanceId))
                    playerManager.NotifyUnitDestroyed(u);
            }
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
