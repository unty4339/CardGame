using System.Collections.Generic;
using CardBattle.Core.Deck;
using UnityEngine;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// 各プレイヤーのデッキの元になるカード情報について責任を持つ
    /// </summary>
    [CreateAssetMenu(fileName = "NewDeckRecipe", menuName = "CardBattle/Deck Recipe")]
    public class DeckRecipe : ScriptableObject
    {
        [SerializeField] private List<DeckRecipeEntry> entries = new();
        [SerializeField] private PartnerTemplate partnerTemplate;

        public IReadOnlyList<DeckRecipeEntry> Entries => entries;
        public PartnerTemplate PartnerTemplate => partnerTemplate;
    }
}
