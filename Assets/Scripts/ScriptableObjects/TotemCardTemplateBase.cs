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

        /// <summary>
        /// 表示用説明文。トーテムは「カード名＋改行＋コストX」を文頭に付与する。
        /// </summary>
        public override string GetDisplayDescription() =>
            $"{CardName}\nコスト{PlayCost}\n\n{Description}";
    }
}
