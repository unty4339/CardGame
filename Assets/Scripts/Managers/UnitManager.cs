using System;
using CardBattle.Core.Deck;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.ScriptableObjects;
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
        /// カード、オーナーID、フィールドゾーンを受け取り、対応するユニットを登場させる
        /// </summary>
        public Unit SpawnUnitFromCard(Card card, int ownerPlayerId, FieldZone fieldZone)
        {
            if (card == null)
                throw new ArgumentNullException(nameof(card));
            if (card.Template == null)
                throw new ArgumentException("Card.Template is null.", nameof(card));
            if (fieldZone == null)
                throw new ArgumentNullException(nameof(fieldZone));
            if (card.Template.CardType != CardType.Unit) return null;

            var unitData = card.Template.UnitData;
            if (unitData == null)
                throw new ArgumentException("CardTemplate.UnitData is null for Unit card.", nameof(card));

            var unit = new Unit
            {
                HP = unitData.BaseHP,
                Attack = unitData.BaseAttack,
                TurnsOnField = 0,
                CanAttack = false,
                Keywords = new System.Collections.Generic.List<KeywordAbility>(unitData.Keywords),
                Effects = new System.Collections.Generic.List<Effect>(),
                PairingTarget = null,
                OwnerPlayerId = ownerPlayerId,
                IsPartner = false
            };

            fieldZone.Units.Add(unit);

            foreach (var effect in unit.Effects)
            {
                if (effect.TriggerTiming == EffectTriggerTiming.OnSummon)
                {
                    effect.EffectLogic?.Invoke();
                }
            }

            return unit;
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
