using CardBattle.Core.Effects;
using CardBattle.Core.Enums;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// トーテムカード設計データの基底。
    /// </summary>
    public abstract class TotemCardTemplateBase : CardTemplate
    {
        protected TotemData totemData;

        public override CardType CardType => CardType.Totem;
        public override TotemData TotemData => totemData;

        /// <summary>
        /// 発動時（プレイ時）にペアリング対象選択を行う効果。このテンプレートが IOnPairingEffect を実装していればそれを返す。
        /// </summary>
        public virtual IOnPairingEffect GetOnPlayPairingEffect()
        {
            return this is IOnPairingEffect effect ? effect : null;
        }
    }
}
