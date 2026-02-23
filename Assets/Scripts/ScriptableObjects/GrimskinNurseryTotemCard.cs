using UnityEngine;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// グリンスキンの苗床。トーテム。発動時ペア・ターン開始時等の効果はトーテム用フック拡張が必要。
    /// </summary>
    public class GrimskinNurseryTotemCard : TotemCardTemplateBase
    {
        public GrimskinNurseryTotemCard()
        {
            cardName = "グリンスキンの苗床";
            playCost = 2;
            description = "トーテム\n\n発動時：\nペアリング中でない、自分パートナーか体力1の相手ユニットとペアリングする。\n\nペアリング中：\nペア対象に「攻撃できない」を付与する。\n\nターン開始時：\n「ゴブリン」を2枚手札に加え、ペア対象とこれを破壊する。\n手札を1枚捨てることで、これを破壊する代わりに手札に戻す。\n\nペアリング解除時：\nこのカードを破壊する。";
            totemData = ScriptableObject.CreateInstance<TotemData>();
        }
    }
}
