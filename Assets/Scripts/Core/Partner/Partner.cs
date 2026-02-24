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

        /// <summary>
        /// アートワーク用のカード名。null/空のときは画像を探さない。
        /// </summary>
        public string CardName { get; set; }

        /// <summary>
        /// カード説明文。マウスオーバー時に画面右側に表示する。
        /// </summary>
        public string Description { get; set; }
    }
}
