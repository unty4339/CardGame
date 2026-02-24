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
                // 自分のデッキ: パートナー（未来航士、リュシア）以外の追加カードを3枚ずつ
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
                // 相手のデッキ: 騎士団カードを各3枚ずつ（合計30枚）
                const int copies = 3;
                entriesList.Add(new DeckRecipeEntry { Template = new KnightSquireUnitCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new KnightSpearmanUnitCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new KnightShieldUnitCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new KnightArcherUnitCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new KnightCavalryUnitCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new KnightHeavyUnitCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new KnightCaptainUnitCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new KnightCommanderUnitCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new KnightOrderSpellCard(), Count = copies });
                entriesList.Add(new DeckRecipeEntry { Template = new KnightChargeSpellCard(), Count = copies });
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
