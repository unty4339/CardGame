using System.Collections.Generic;
using CardBattle.Core.Enums;
using UnityEngine;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// ユニットの基本性能データについて責任を持つ
    /// </summary>
    [CreateAssetMenu(fileName = "NewUnitData", menuName = "CardBattle/Unit Data")]
    public class UnitData : ScriptableObject
    {
        [SerializeField] private int baseHP;
        [SerializeField] private int baseAttack;
        [SerializeField] private List<KeywordAbility> keywords = new();

        public int BaseHP => baseHP;
        public int BaseAttack => baseAttack;
        public IReadOnlyList<KeywordAbility> Keywords => keywords;
    }
}
