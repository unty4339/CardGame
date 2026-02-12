using System.Collections.Generic;
using CardBattle.Core.Effects;
using CardBattle.Core.Enums;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// ユニットカード設計データの基底。具象クラスが IOnSummonEffect を実装すると召喚時効果を持つ。
    /// </summary>
    public abstract class UnitCardTemplateBase : CardTemplate
    {
        protected int baseHP;
        protected int baseAttack;
        protected List<KeywordAbility> keywords = new();

        public override CardType CardType => CardType.Unit;
        public int BaseHP => baseHP;
        public int BaseAttack => baseAttack;
        public IReadOnlyList<KeywordAbility> Keywords => keywords;

        /// <summary>
        /// ユニット作成に必要な情報を返す。UnitManager 等が Unit を組み立てる際に使用する。
        /// </summary>
        public virtual (int hp, int attack, IReadOnlyList<KeywordAbility> keywords) GetUnitStats()
        {
            return (baseHP, baseAttack, keywords);
        }

        /// <summary>
        /// 召喚時に発動する効果。このテンプレートが IOnSummonEffect を実装していればそれを返す。
        /// </summary>
        public virtual IEnumerable<IOnSummonEffect> GetOnSummonEffects()
        {
            if (this is IOnSummonEffect e)
                yield return e;
        }
    }
}
