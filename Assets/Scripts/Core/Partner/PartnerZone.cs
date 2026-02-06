namespace CardBattle.Core.Partner
{
    /// <summary>
    /// パートナーを配置するゾーンについて責任を持つ
    /// </summary>
    public class PartnerZone
    {
        public Partner Partner { get; set; }

        /// <summary>
        /// パートナーがユニットとしてフィールドに登場中かどうか
        /// </summary>
        public bool IsPartnerOnField { get; set; }
    }
}
