using System;
using System.Collections.Generic;
using CardBattle.Dialogue;
using UnityEngine;

namespace CardBattle.ScriptableObjects
{
    [Serializable]
    public class DialogueDatabaseEntry
    {
        [Tooltip("トリガーID（カード名・UnitDestroyed・カスタムIDなど）")]
        public string triggerId;

        public DialogueBlockData blockData;
    }

    /// <summary>
    /// トリガーIDごとの台詞データを保持し、検索する
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueDatabase", menuName = "CardBattle/Dialogue Database")]
    public class DialogueDatabase : ScriptableObject
    {
        [SerializeField] private List<DialogueDatabaseEntry> entries = new();

        /// <summary>
        /// triggerId に紐づく台詞データを取得する。見つからなければ false。
        /// </summary>
        public bool TryGetEntry(string triggerId, out DialogueBlockData data)
        {
            data = default;
            if (string.IsNullOrEmpty(triggerId) || entries == null) return false;

            foreach (var entry in entries)
            {
                if (entry == null) continue;
                if (string.Equals(entry.triggerId, triggerId, StringComparison.Ordinal))
                {
                    data = entry.blockData;
                    return true;
                }
            }

            return false;
        }
    }
}
