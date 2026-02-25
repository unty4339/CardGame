using System.Collections.Generic;
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
        /// プレイ時に発動するペアリング効果のリスト。このテンプレートが IOnPairingEffect を実装していればそれを返す。
        /// </summary>
        public override IReadOnlyList<IOnPairingEffect> GetPairingEffects()
        {
            if (this is IOnPairingEffect effect)
                return new[] { effect };
            return System.Array.Empty<IOnPairingEffect>();
        }
    }
}
