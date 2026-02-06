using System.Collections.Generic;
using CardBattle.Core.Enums;

namespace CardBattle.Core.Field
{
    /// <summary>
    /// フィールドに存在するユニットのデータについて責任を持つ
    /// </summary>
    public class Unit
    {
        private static int _nextId;
        public int InstanceId { get; set; }

        public Unit()
        {
            InstanceId = _nextId++;
        }

        public int HP { get; set; }
        public int Attack { get; set; }
        public int TurnsOnField { get; set; }
        public bool CanAttack { get; set; }
        public List<KeywordAbility> Keywords { get; set; } = new();
        public List<Effect> Effects { get; set; } = new();
        public Unit PairingTarget { get; set; }

        /// <summary>
        /// パートナーであるかどうか
        /// </summary>
        public bool IsPartner { get; set; }

        /// <summary>
        /// オーナープレイヤーID
        /// </summary>
        public int OwnerPlayerId { get; set; }
    }
}
