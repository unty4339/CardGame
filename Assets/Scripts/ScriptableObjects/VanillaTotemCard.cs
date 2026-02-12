namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// トーテムカードの具象クラス。必要に応じてコードで new してデッキに追加する。
    /// </summary>
    public class VanillaTotemCard : TotemCardTemplateBase
    {
        public VanillaTotemCard()
        {
            cardName = "基本のトーテム";
        }
    }
}
