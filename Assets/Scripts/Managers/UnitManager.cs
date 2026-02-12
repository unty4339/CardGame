using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CardBattle.AI;
using CardBattle.Core.Deck;
using CardBattle.Core.Effects;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
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
        /// </summary>
        public Unit SpawnUnitFromCard(Card card, int ownerPlayerId, FieldZone fieldZone, Action<Unit> onEffectsResolved = null)
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
            var unit = new Unit
            {
                HP = hp,
                Attack = attack,
                TurnsOnField = 0,
                CanAttack = false,
                Keywords = new System.Collections.Generic.List<KeywordAbility>(keywords),
                Effects = new System.Collections.Generic.List<Effect>(),
                PairingTarget = null,
                OwnerPlayerId = ownerPlayerId,
                IsPartner = false
            };

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
                        }

                        myData.HP = state.MyHP;
                        oppData.HP = state.OpponentHP;
                        playerManager.NotifyPlayerDataChanged(ownerPlayerId);
                        playerManager.NotifyPlayerDataChanged(opponentId);

                        foreach (var u in opponentUnitsBefore)
                        {
                            if (!state.OpponentField.Units.Any(x => x.InstanceId == u.InstanceId))
                                playerManager.NotifyUnitDestroyed(u);
                        }
                        foreach (var u in myUnitsBefore)
                        {
                            if (!state.MyField.Units.Any(x => x.InstanceId == u.InstanceId))
                                playerManager.NotifyUnitDestroyed(u);
                        }
                        onEffectsResolved?.Invoke(unit);
                    }
                }
            }
            else
            {
                onEffectsResolved?.Invoke(unit);
            }

            return unit;
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
                    playerManager.NotifyUnitDestroyed(u);
            }
            foreach (var u in myUnitsBefore)
            {
                if (!state.MyField.Units.Any(x => x.InstanceId == u.InstanceId))
                    playerManager.NotifyUnitDestroyed(u);
            }

            onEffectsResolved?.Invoke(unit);
        }

        /// <summary>
        /// カード、オーナーID、フィールドゾーンを受け取り、対応するトーテムを登場させる
        /// </summary>
        public Totem SpawnTotemFromCard(Card card, int ownerPlayerId, FieldZone fieldZone)
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

            var totem = new Totem
            {
                OwnerPlayerId = ownerPlayerId
            };

            fieldZone.Totems.Add(totem);
            return totem;
        }
    }
}
