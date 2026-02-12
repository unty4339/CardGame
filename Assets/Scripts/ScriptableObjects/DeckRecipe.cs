using System.Collections.Generic;
using System.Reflection;
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

        /// <summary>
        /// 指定プレイヤー用のデッキレシピを動的に作成する。VanillaUnitCard / DealOneDamageSpellCard / DealOneDamageUnitCard を各10枚ずつ（合計30枚）含む。
        /// </summary>
        public static DeckRecipe CreateForPlayer(int playerId)
        {
            var deckRecipe = ScriptableObject.CreateInstance<DeckRecipe>();
            var entriesList = GetPrivateField<List<DeckRecipeEntry>>(deckRecipe, "entries");
            entriesList.Add(new DeckRecipeEntry { Template = new VanillaUnitCard(), Count = 10 });
            entriesList.Add(new DeckRecipeEntry { Template = new DealOneDamageSpellCard(), Count = 10 });
            entriesList.Add(new DeckRecipeEntry { Template = new DealOneDamageUnitCard(), Count = 10 });

            return deckRecipe;
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field?.GetValue(target);
        }
    }
}
