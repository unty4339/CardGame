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
        /// 指定プレイヤー用のデッキレシピを動的に作成する。
        /// playerId==0（自分）: パートナー以外の追加カードを各3枚ずつ（合計27枚）。
        /// playerId==1（相手）: VanillaUnitCard / DealOneDamageSpellCard / DealOneDamageUnitCard を各10枚ずつ（合計30枚）。
        /// </summary>
        public static DeckRecipe CreateForPlayer(int playerId)
        {
            var deckRecipe = ScriptableObject.CreateInstance<DeckRecipe>();
            var entriesList = GetPrivateField<List<DeckRecipeEntry>>(deckRecipe, "entries");

            if (playerId == 0)
            {
                // 自分のデッキ: パートナー（未来航路の番人、クーリア）以外の追加カードを3枚ずつ
                const int copies = 3;
                entriesList.Add(new DeckRecipeEntry { Template = new GoblinUnitCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new GoblinSpearmanUnitCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new GoblinCavalryUnitCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new GoblinCommanderUnitCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new ForestBarbarianOgreUnitCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new FleshArmorOgreUnitCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new GreenLightHeraldUnitCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new GrimskinWhistleSpellCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new GrimskinNurseryTotemCard(), Count = copies });
            }
            else
            {
                // 相手のデッキ: 変更不要（従来どおり）
                entriesList.Add(new DeckRecipeEntry { Template = new VanillaUnitCard(), Count = 10 });
                entriesList.Add(new DeckRecipeEntry { Template = new DealOneDamageSpellCard(), Count = 10 });
                entriesList.Add(new DeckRecipeEntry { Template = new DealOneDamageUnitCard(), Count = 10 });
            }

            return deckRecipe;
        }

        private static T GetPrivateField<T>(object target, string fieldName)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)field?.GetValue(target);
        }
    }
}
