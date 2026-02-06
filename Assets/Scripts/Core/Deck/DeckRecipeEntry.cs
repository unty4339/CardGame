using CardBattle.ScriptableObjects;

namespace CardBattle.Core.Deck
{
    /// <summary>
    /// デッキレシピの1エントリについて責任を持つ
    /// </summary>
    [System.Serializable]
    public class DeckRecipeEntry
    {
        public CardTemplate Template;
        public int Count;
    }
}
