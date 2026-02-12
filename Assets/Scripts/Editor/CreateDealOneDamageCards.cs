using UnityEngine;
using UnityEditor;

namespace CardBattle.Editor
{
    /// <summary>
    /// UnitData は廃止済み。ユニット性能はカードテンプレート（UnitCardTemplateBase）に設定する。
    /// </summary>
    public static class CreateDealOneDamageCards
    {
        [MenuItem("Window/CardBattle/Create Deal One Damage Card Assets")]
        public static void CreateAssets()
        {
            Debug.Log("UnitData は廃止済みです。ユニットの性能（HP/攻撃力/キーワード）はカードテンプレートに設定してください。");
        }
    }
}
