using CardBattle.Core.Enums;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// カード設計データの共通基底。種別ごとの基底（UnitCardTemplateBase 等）が継承する。
    /// </summary>
    public abstract class CardTemplate
    {
        protected int playCost;
        protected string cardName;

        public virtual int PlayCost => playCost;
        public virtual string CardName => !string.IsNullOrEmpty(cardName) ? cardName : GetType().Name;
        public abstract CardType CardType { get; }
        public virtual TotemData TotemData => null;
    }
}
