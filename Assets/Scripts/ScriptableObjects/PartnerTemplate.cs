using System.Collections.Generic;
using CardBattle.Core.Enums;
using CardBattle.Core.Partner;
using UnityEngine;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// パートナーの性能データをInspectorで設定するためのScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewPartnerTemplate", menuName = "CardBattle/Partner Template")]
    public class PartnerTemplate : ScriptableObject
    {
        [SerializeField] private int cost = 1;
        [SerializeField] private int baseHP = 1;
        [SerializeField] private int baseAttack = 1;
        [SerializeField] private List<KeywordAbility> keywords = new();

        public int Cost => cost;
        public int BaseHP => baseHP;
        public int BaseAttack => baseAttack;
        public IReadOnlyList<KeywordAbility> Keywords => keywords;

        /// <summary>
        /// 実行時にPartnerインスタンスを生成して返す
        /// </summary>
        public Partner ToPartner()
        {
            var list = keywords != null ? new List<KeywordAbility>(keywords) : new List<KeywordAbility>();
            return new Partner
            {
                Cost = cost,
                BaseHP = baseHP,
                BaseAttack = baseAttack,
                Keywords = list
            };
        }
    }
}
