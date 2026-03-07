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

        /// <summary>
        /// ペアリング時に発動する効果。このテンプレートが IOnPairingEffect を実装していればそれを返す。
        /// </summary>
        public virtual IEnumerable<IOnPairingEffect> GetOnPairingEffects()
        {
            if (this is IOnPairingEffect e)
                yield return e;
        }

        /// <summary>
        /// プレイ時／召喚時に発動するペアリング効果のリスト。
        /// </summary>
        public override IReadOnlyList<IOnPairingEffect> GetPairingEffects()
        {
            if (this is IOnPairingEffect e)
                return new[] { e };
            return System.Array.Empty<IOnPairingEffect>();
        }

        /// <summary>
        /// ペアリング解除時に発動する効果。このテンプレートが IOnUnpairEffect を実装していればそれを返す。
        /// </summary>
        public override IReadOnlyList<IOnUnpairEffect> GetOnUnpairEffects()
        {
            if (this is IOnUnpairEffect e)
                return new[] { e };
            return System.Array.Empty<IOnUnpairEffect>();
        }

        /// <summary>
        /// 表示用説明文。ユニットは「カード名＋改行＋コストX / 攻撃 Y / 体力 Z」を文頭に付与する。
        /// </summary>
        public override string GetDisplayDescription() =>
            $"{CardName}\nコスト{PlayCost} / 攻撃 {BaseAttack} / 体力 {BaseHP}\n\n{Description}";
    }
}
