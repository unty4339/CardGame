using UnityEngine;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// スペルカードの効果データ。カードタイプがスペルの場合、プレイ時に得る効果を保持する。
    /// 仕様書に詳細定義がないためプレースホルダーとして実装。
    /// </summary>
    [CreateAssetMenu(fileName = "NewSpellEffect", menuName = "CardBattle/Spell Effect")]
    public class SpellEffect : ScriptableObject
    {
        // 効果の詳細は後続の仕様で定義
    }
}
