using System;
using CardBattle.Core.Field;
using CardBattle.Core.Partner;
using CardBattle.Core.Player;
using UnityEngine;

namespace CardBattle.Managers
{
    /// <summary>
    /// パートナーの配置とユニット登場・戻しについて責任を持つ
    /// </summary>
    public class PartnerManager : MonoBehaviour
    {
        private static PartnerManager _instance;
        public static PartnerManager Instance => _instance;

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
        /// プレイヤーIDとパートナーを受け取り、パートナーゾーンに表側で配置する
        /// </summary>
        public void PlacePartner(int playerId, Partner partner)
        {
            var playerManager = PlayerManager.Instance;
            if (playerManager == null)
                throw new InvalidOperationException("PlayerManager.Instance is null.");

            var data = playerManager.GetPlayerData(playerId);
            if (data == null)
                throw new ArgumentException($"Player data not found for playerId={playerId}.", nameof(playerId));
            if (data.PartnerZone == null)
                throw new InvalidOperationException($"Player {playerId} has no PartnerZone.");

            data.PartnerZone.Partner = partner;
            data.PartnerZone.IsPartnerOnField = false;
        }

        /// <summary>
        /// プレイヤーIDを受け取り、コストを支払ってパートナーをユニットとしてフィールドに登場させる
        /// </summary>
        public bool SpawnPartnerAsUnit(int playerId)
        {
            var playerManager = PlayerManager.Instance;
            if (playerManager == null)
                throw new InvalidOperationException("PlayerManager.Instance is null.");

            var data = playerManager.GetPlayerData(playerId);
            if (data == null || data.PartnerZone == null || data.PartnerZone.Partner == null)
                throw new InvalidOperationException("Player data, PartnerZone or Partner is null.");
            if (data.PartnerZone.IsPartnerOnField) return false;

            var partner = data.PartnerZone.Partner;
            if (data.CurrentMP < partner.Cost) return false;

            data.CurrentMP -= partner.Cost;
            playerManager.NotifyPlayerDataChanged(playerId);

            var unit = new Unit
            {
                HP = partner.BaseHP,
                Attack = partner.BaseAttack,
                TurnsOnField = 0,
                CanAttack = false,
                Keywords = new System.Collections.Generic.List<Core.Enums.KeywordAbility>(partner.Keywords),
                Effects = new System.Collections.Generic.List<Effect>(),
                PairingTarget = null,
                OwnerPlayerId = playerId,
                IsPartner = true
            };

            data.FieldZone.Units.Add(unit);
            data.PartnerZone.IsPartnerOnField = true;

            return true;
        }

        /// <summary>
        /// パートナーユニットとオーナーIDを受け取り、パートナーゾーンに戻す
        /// </summary>
        public void ReturnPartnerToZone(Unit partnerUnit, int ownerPlayerId)
        {
            if (partnerUnit == null || !partnerUnit.IsPartner) return;

            var playerManager = PlayerManager.Instance;
            if (playerManager == null)
                throw new InvalidOperationException("PlayerManager.Instance is null.");

            var data = playerManager.GetPlayerData(ownerPlayerId);
            if (data == null)
                throw new ArgumentException($"Player data not found for ownerPlayerId={ownerPlayerId}.", nameof(ownerPlayerId));
            if (data.PartnerZone == null)
                throw new InvalidOperationException($"Player {ownerPlayerId} has no PartnerZone.");

            data.FieldZone.Units.Remove(partnerUnit);
            data.PartnerZone.IsPartnerOnField = false;
        }
    }
}
