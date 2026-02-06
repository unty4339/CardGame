using System;
using System.Collections.Generic;
using CardBattle.Core.Enums;

namespace CardBattle.Core.Partner
{
    /// <summary>
    /// パートナーの性能データについて責任を持つ
    /// </summary>
    [Serializable]
    public class Partner
    {
        public int Cost { get; set; }
        public int BaseHP { get; set; }
        public int BaseAttack { get; set; }
        public List<KeywordAbility> Keywords { get; set; } = new();
    }
}
