using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardBattle.AI;
using CardBattle.Core.Deck;
using CardBattle.Core.Effects;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.Core.Player;
using CardBattle.ScriptableObjects;
using CardBattle.UI;
using UnityEngine;

namespace CardBattle.Managers
{
    /// <summary>
    /// ユニットとトーテムの登場・管理について責任を持つ
    /// </summary>
    public class UnitManager : MonoBehaviour
    {
        private static UnitManager _instance;
        public static UnitManager Instance => _instance;

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
        /// カード、オーナーID、フィールドゾーンを受け取り、対応するユニットを登場させる。
        /// 召喚時効果で対象選択が必要な場合はコルーチンで非同期に解決し、完了時に onEffectsResolved(unit) を呼ぶ。
        /// preferredInstanceId が指定された場合、その InstanceId でユニットを生成する（AI シミュレーションとの統一用）。
        /// </summary>
        public Unit SpawnUnitFromCard(Card card, int ownerPlayerId, FieldZone fieldZone, Action<Unit> onEffectsResolved = null, int? preferredInstanceId = null)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));
            if (card.Template == null)
                throw new ArgumentException("Card.Template is null.", nameof(card));
            if (fieldZone == null)
                throw new ArgumentNullException(nameof(fieldZone));
            if (card.Template.CardType != CardType.Unit) return null;

            var unitBase = card.Template as UnitCardTemplateBase;
            if (unitBase == null)
                throw new ArgumentException("Unit card template is not UnitCardTemplateBase.", nameof(card));

            var (hp, attack, keywords) = unitBase.GetUnitStats();
            Unit unit;
            if (preferredInstanceId.HasValue)
            {
                unit = Unit.CreateWithInstanceId(preferredInstanceId.Value);
                unit.HP = hp;
                unit.Attack = attack;
                unit.TurnsOnField = 0;
                unit.CanAttackUnit = false;
                unit.CanAttackPlayer = false;
                unit.Keywords = new System.Collections.Generic.List<KeywordAbility>(keywords);
                unit.Effects = new System.Collections.Generic.List<Effect>();
                unit.PairingTarget = null;
                unit.PairingWithPartnerCard = false;
                unit.OwnerPlayerId = ownerPlayerId;
                unit.IsPartner = false;
                unit.SourceCardTemplate = card.Template;
            }
            else
            {
                unit = new Unit
                {
                    HP = hp,
                    Attack = attack,
                    TurnsOnField = 0,
                    CanAttackUnit = false,
                    CanAttackPlayer = false,
                    Keywords = new System.Collections.Generic.List<KeywordAbility>(keywords),
                    Effects = new System.Collections.Generic.List<Effect>(),
                    PairingTarget = null,
                    PairingWithPartnerCard = false,
                    OwnerPlayerId = ownerPlayerId,
                    IsPartner = false,
                    SourceCardTemplate = card.Template
                };
            }

            fieldZone.Units.Add(unit);

            var onSummonEffects = unitBase.GetOnSummonEffects()?.ToList() ?? new List<Core.Effects.IOnSummonEffect>();
            if (onSummonEffects.Count > 0)
            {
                var playerManager = PlayerManager.Instance;
                if (playerManager != null)
                {
                    var myData = playerManager.GetPlayerData(ownerPlayerId);
                    var opponentId = ownerPlayerId == 0 ? 1 : 0;
                    var oppData = playerManager.GetPlayerData(opponentId);
                    if (myData != null && oppData != null)
                    {
                        var state = new GameState
                        {
                            MyPlayerId = ownerPlayerId,
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

                        var resolver = EffectResolver.Instance;
                        var needTargetSelection = resolver != null && ownerPlayerId == 0
                            && onSummonEffects.Any(e => (e.GetAvailableTargets(state, unit)?.Count ?? 0) > 1);

                        if (needTargetSelection)
                        {
                            StartCoroutine(ResolveSummonEffectsCoroutine(
                                onSummonEffects, unit, state, ownerPlayerId, opponentId,
                                opponentUnitsBefore, myUnitsBefore, onEffectsResolved));
                            return unit;
                        }

                        var gvm = GameVisualManager.Instance;
                        foreach (var effect in onSummonEffects)
                        {
                            var choices = effect.GetAvailableTargets(state, unit);
                            var target = resolver != null
                                ? resolver.RequestTargetAsync(choices, ownerPlayerId).GetAwaiter().GetResult()
                                : (choices != null && choices.Count > 0 ? choices[0] : EffectTarget.None());
                            if (target.Kind == EffectTargetKind.Unit && target.UnitInstanceId != null)
                                gvm?.PlayEffectAtUnit(opponentId, target.UnitInstanceId.Value);
                            effect.Resolve(target, state, unit);
                            NotifyUnitHpChangedIfStillOnField(playerManager, state, target);
                        }

                        myData.HP = state.MyHP;
                        oppData.HP = state.OpponentHP;
                        playerManager.NotifyPlayerDataChanged(ownerPlayerId);
                        playerManager.NotifyPlayerDataChanged(opponentId);

                        foreach (var u in opponentUnitsBefore)
                        {
                            if (!state.OpponentField.Units.Any(x => x.InstanceId == u.InstanceId))
                                playerManager.UnpairIfNeededAndNotifyDestroyed(u);
                        }
                        foreach (var u in myUnitsBefore)
                        {
                            if (!state.MyField.Units.Any(x => x.InstanceId == u.InstanceId))
                                playerManager.UnpairIfNeededAndNotifyDestroyed(u);
                        }

                        TryResolvePairingSync(unit, unit.SourceCardTemplate, ownerPlayerId, opponentId, myData, oppData, state, onEffectsResolved);
                    }
                }
            }
            else
            {
                if (unit != null && unit.SourceCardTemplate != null)
                {
                    var playerManager = PlayerManager.Instance;
                    var myData = playerManager?.GetPlayerData(ownerPlayerId);
                    var oppData = playerManager?.GetPlayerData(ownerPlayerId == 0 ? 1 : 0);
                    if (myData != null && oppData != null)
                    {
                        var opponentId = ownerPlayerId == 0 ? 1 : 0;
                        var state = new GameState
                        {
                            MyPlayerId = ownerPlayerId,
                            OpponentPlayerId = opponentId,
                            MyField = myData.FieldZone,
                            OpponentField = oppData.FieldZone,
                            MyHand = new List<Card>(myData.Hand.Cards),
                            MyHP = myData.HP,
                            OpponentHP = oppData.HP,
                            MyMP = myData.CurrentMP,
                            OpponentMP = oppData.CurrentMP
                        };
                        var onPairingEffects = unit.SourceCardTemplate.GetPairingEffects();
                        var needAsyncPairing = onPairingEffects != null && onPairingEffects.Count > 0
                            && EffectResolver.Instance != null
                            && ownerPlayerId == 0;
                        if (needAsyncPairing)
                        {
                            var isPartnerOnField = myData.PartnerZone?.IsPartnerOnField ?? false;
                            var choices = onPairingEffects[0].GetAvailableTargets(state, unit, isPartnerOnField);
                            if (choices != null && choices.Count > 1)
                            {
                                StartCoroutine(ResolvePairingCoroutine(unit, unit.SourceCardTemplate, ownerPlayerId, opponentId, state, playerManager, onEffectsResolved));
                                return unit;
                            }
                        }
                        TryResolvePairingSync(unit, unit.SourceCardTemplate, ownerPlayerId, opponentId, myData, oppData, state, onEffectsResolved);
                        return unit;
                    }
                }
                onEffectsResolved?.Invoke(unit);
            }

            return unit;
        }

        private static void TryResolvePairingSync(
            Unit unit,
            CardTemplate template,
            int ownerPlayerId,
            int opponentId,
            PlayerData myData,
            PlayerData oppData,
            GameState state,
            Action<Unit> onEffectsResolved)
        {
            var onPairingEffects = template?.GetPairingEffects();
            if (onPairingEffects == null || onPairingEffects.Count == 0)
            {
                onEffectsResolved?.Invoke(unit);
                return;
            }

            var isPartnerOnField = myData.PartnerZone?.IsPartnerOnField ?? false;
            var choices = onPairingEffects[0].GetAvailableTargets(state, unit, isPartnerOnField);
            if (choices == null || choices.Count == 0)
            {
                onEffectsResolved?.Invoke(unit);
                return;
            }

            var resolver = EffectResolver.Instance;
            var needTargetSelection = resolver != null && ownerPlayerId == 0 && choices.Count >= 1;
            EffectTarget target;
            if (needTargetSelection)
                target = resolver.RequestTargetAsync(choices, ownerPlayerId).GetAwaiter().GetResult();
            else
                target = choices[0];

            PairingService.ApplyPairingResult(unit, target, onPairingEffects, state, myData, oppData);
            onEffectsResolved?.Invoke(unit);
        }

        /// <summary>
        /// 召喚時効果でユニット対象にダメージ等を適用した後、そのユニットがまだ場に残っていれば HP 表示更新を通知する。
        /// </summary>
        private static void NotifyUnitHpChangedIfStillOnField(PlayerManager playerManager, GameState state, EffectTarget target)
        {
            if (playerManager == null || state?.MyField?.Units == null || state?.OpponentField?.Units == null) return;
            if (target.Kind != EffectTargetKind.Unit || target.UnitInstanceId == null) return;
            var u = state.MyField.Units.Find(x => x.InstanceId == target.UnitInstanceId.Value)
                ?? state.OpponentField.Units.Find(x => x.InstanceId == target.UnitInstanceId.Value);
            if (u != null)
                playerManager.NotifyUnitHpChanged(u);
        }

        private IEnumerator ResolveSummonEffectsCoroutine(
            List<Core.Effects.IOnSummonEffect> onSummonEffects,
            Unit unit,
            GameState state,
            int ownerPlayerId,
            int opponentId,
            List<Unit> opponentUnitsBefore,
            List<Unit> myUnitsBefore,
            Action<Unit> onEffectsResolved)
        {
            var resolver = EffectResolver.Instance;
            var gvm = GameVisualManager.Instance;
            var playerManager = PlayerManager.Instance;
            if (playerManager == null)
            {
                onEffectsResolved?.Invoke(unit);
                yield break;
            }

            foreach (var effect in onSummonEffects)
            {
                var choices = effect.GetAvailableTargets(state, unit);
                var needUiSelection = resolver != null && ownerPlayerId == 0 && choices != null && choices.Count > 1;
                EffectTarget target;
                if (needUiSelection)
                {
                    var task = resolver.RequestTargetAsync(choices, ownerPlayerId);
                    while (!task.IsCompleted)
                        yield return null;
                    target = task.GetAwaiter().GetResult();
                }
                else
                {
                    target = resolver != null && choices != null && choices.Count > 0
                        ? resolver.RequestTargetAsync(choices, ownerPlayerId).GetAwaiter().GetResult()
                        : (choices != null && choices.Count > 0 ? choices[0] : EffectTarget.None());
                }
                if (target.Kind == EffectTargetKind.Unit && target.UnitInstanceId != null)
                    gvm?.PlayEffectAtUnit(opponentId, target.UnitInstanceId.Value);
                effect.Resolve(target, state, unit);
                NotifyUnitHpChangedIfStillOnField(playerManager, state, target);
            }

            var myData = playerManager.GetPlayerData(ownerPlayerId);
            var oppData = playerManager.GetPlayerData(opponentId);
            if (myData != null) myData.HP = state.MyHP;
            if (oppData != null) oppData.HP = state.OpponentHP;
            playerManager.NotifyPlayerDataChanged(ownerPlayerId);
            playerManager.NotifyPlayerDataChanged(opponentId);

            foreach (var u in opponentUnitsBefore)
            {
                if (!state.OpponentField.Units.Any(x => x.InstanceId == u.InstanceId))
                    playerManager.UnpairIfNeededAndNotifyDestroyed(u);
            }
            foreach (var u in myUnitsBefore)
            {
                if (!state.MyField.Units.Any(x => x.InstanceId == u.InstanceId))
                    playerManager.UnpairIfNeededAndNotifyDestroyed(u);
            }

            var template = unit.SourceCardTemplate;
            if (template != null && template.GetPairingEffects().Count > 0)
            {
                yield return ResolvePairingCoroutine(unit, template, ownerPlayerId, opponentId, state, playerManager, onEffectsResolved);
                yield break;
            }

            onEffectsResolved?.Invoke(unit);
        }

        private IEnumerator ResolvePairingCoroutine(
            Unit unit,
            CardTemplate template,
            int ownerPlayerId,
            int opponentId,
            GameState state,
            PlayerManager playerManager,
            Action<Unit> onEffectsResolved)
        {
            var onPairingEffects = template?.GetPairingEffects();
            if (onPairingEffects == null || onPairingEffects.Count == 0)
            {
                onEffectsResolved?.Invoke(unit);
                yield break;
            }

            var myData = playerManager.GetPlayerData(ownerPlayerId);
            var oppData = playerManager.GetPlayerData(opponentId);
            if (myData == null || oppData == null)
            {
                onEffectsResolved?.Invoke(unit);
                yield break;
            }

            var isPartnerOnField = myData.PartnerZone?.IsPartnerOnField ?? false;
            var choices = onPairingEffects[0].GetAvailableTargets(state, unit, isPartnerOnField);
            if (choices == null || choices.Count == 0)
            {
                onEffectsResolved?.Invoke(unit);
                yield break;
            }

            var resolver = EffectResolver.Instance;
            var needTargetSelection = resolver != null && ownerPlayerId == 0 && choices.Count >= 1;
            EffectTarget target;
            if (needTargetSelection)
            {
                var task = resolver.RequestTargetAsync(choices, ownerPlayerId);
                while (!task.IsCompleted)
                    yield return null;
                target = task.GetAwaiter().GetResult();
            }
            else
            {
                target = choices[0];
            }

            PairingService.ApplyPairingResult(unit, target, onPairingEffects, state, myData, oppData);
            onEffectsResolved?.Invoke(unit);
        }

        /// <summary>
        /// プレイ時／召喚時のペアリングを解決する。効果がある場合は対象取得ののち必要なら非同期でプレイヤーに選択させ、適用後に onComplete を呼ぶ。ユニット・トーテム共通。
        /// </summary>
        public void RunOnPlayPairing(Unit unit, Card card, int ownerId, Action onComplete)
        {
            if (unit == null || card?.Template == null || onComplete == null)
            {
                onComplete?.Invoke();
                return;
            }

            var effects = card.Template.GetPairingEffects();
            if (effects == null || effects.Count == 0)
            {
                onComplete();
                return;
            }

            var playerManager = PlayerManager.Instance;
            var opponentId = ownerId == 0 ? 1 : 0;
            var myData = playerManager?.GetPlayerData(ownerId);
            var oppData = playerManager?.GetPlayerData(opponentId);
            if (myData == null || oppData == null)
            {
                onComplete();
                return;
            }

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

            var isPartnerOnField = myData.PartnerZone?.IsPartnerOnField ?? false;
            var choices = effects[0].GetAvailableTargets(state, unit, isPartnerOnField);
            if (choices == null || choices.Count == 0)
            {
                onComplete();
                return;
            }

            var resolver = EffectResolver.Instance;
            var needTargetSelection = resolver != null && ownerId == 0 && choices.Count >= 1;

            if (needTargetSelection)
            {
                StartCoroutine(OnPlayPairingCoroutine(unit, effects, ownerId, opponentId, state, myData, oppData, choices, onComplete));
                return;
            }

            var target = choices[0];
            PairingService.ApplyPairingResult(unit, target, effects, state, myData, oppData);
            onComplete();
        }

        private IEnumerator OnPlayPairingCoroutine(
            Unit unit,
            IReadOnlyList<IOnPairingEffect> effects,
            int ownerId,
            int opponentId,
            GameState state,
            PlayerData myData,
            PlayerData oppData,
            IList<EffectTarget> choices,
            Action onComplete)
        {
            yield return null;
            var resolver = EffectResolver.Instance;
            var task = resolver.RequestTargetAsync(choices, ownerId);
            while (!task.IsCompleted)
                yield return null;
            var target = task.GetAwaiter().GetResult();
            PairingService.ApplyPairingResult(unit, target, effects, state, myData, oppData);
            onComplete();
        }

        /// <summary>
        /// カード、オーナーID、フィールドゾーンを受け取り、トーテムをユニットとしてフィールドに登場させる
        /// </summary>
        public Unit SpawnTotemFromCard(Card card, int ownerPlayerId, FieldZone fieldZone)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));
            if (card.Template == null)
                throw new ArgumentException("Card.Template is null.", nameof(card));
            if (fieldZone == null)
                throw new ArgumentNullException(nameof(fieldZone));
            if (card.Template.CardType != CardType.Totem) return null;

            var totemData = card.Template.TotemData;
            if (totemData == null)
                throw new ArgumentException("CardTemplate.TotemData is null for Totem card.", nameof(card));

            var unit = new Unit
            {
                HP = 0,
                Attack = 0,
                TurnsOnField = 0,
                CanAttackUnit = false,
                CanAttackPlayer = false,
                OwnerPlayerId = ownerPlayerId,
                SourceCardTemplate = card.Template,
                IsTotem = true
            };

            fieldZone.Units.Add(unit);
            return unit;
        }
    }
}
