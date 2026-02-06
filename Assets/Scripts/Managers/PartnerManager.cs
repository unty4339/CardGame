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
            if (playerManager == null) return;

            var data = playerManager.GetPlayerData(playerId);
            if (data?.PartnerZone == null) return;

            data.PartnerZone.Partner = partner;
            data.PartnerZone.IsPartnerOnField = false;
        }

        /// <summary>
        /// プレイヤーIDを受け取り、コストを支払ってパートナーをユニットとしてフィールドに登場させる
        /// </summary>
        public bool SpawnPartnerAsUnit(int playerId)
        {
            var playerManager = PlayerManager.Instance;
            if (playerManager == null) return false;

            var data = playerManager.GetPlayerData(playerId);
            if (data?.PartnerZone?.Partner == null) return false;
            if (data.PartnerZone.IsPartnerOnField) return false;

            var partner = data.PartnerZone.Partner;
            if (data.CurrentMP < partner.Cost) return false;

            data.CurrentMP -= partner.Cost;

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
            if (playerManager == null) return;

            var data = playerManager.GetPlayerData(ownerPlayerId);
            if (data?.PartnerZone == null) return;

            data.FieldZone.Units.Remove(partnerUnit);
            data.PartnerZone.IsPartnerOnField = false;
        }
    }
}
