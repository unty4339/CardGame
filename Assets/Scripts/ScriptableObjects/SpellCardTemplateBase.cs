using CardBattle.Core.Enums;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 呪文カード設計データの基底。具象クラスが ISpellEffect を実装して効果を持つ。
    /// </summary>
    public abstract class SpellCardTemplateBase : CardTemplate
    {
        public override CardType CardType => CardType.Spell;

        /// <summary>
        /// 表示用説明文。スペルは「カード名＋改行＋コストX」を文頭に付与する。
        /// </summary>
        public override string GetDisplayDescription() =>
            $"{CardName}\nコスト{PlayCost}\n\n{Description}";
    }
}
