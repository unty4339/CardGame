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
    }
}
