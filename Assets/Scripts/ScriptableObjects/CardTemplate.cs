using CardBattle.Core.Enums;
using UnityEngine;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// カードそれ自体の性能データについて責任を持つ
    /// </summary>
    [CreateAssetMenu(fileName = "NewCardTemplate", menuName = "CardBattle/Card Template")]
    public class CardTemplate : ScriptableObject
    {
        [SerializeField] private CardType cardType;
        [SerializeField] private int playCost;
        [SerializeField] private UnitData unitData;
        [SerializeField] private TotemData totemData;
        [SerializeField] private SpellEffect spellEffect;

        public CardType CardType => cardType;
        public int PlayCost => playCost;
        public UnitData UnitData => unitData;
        public TotemData TotemData => totemData;
        public SpellEffect SpellEffect => spellEffect;
    }
}
