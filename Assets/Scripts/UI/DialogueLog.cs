using System.Collections.Generic;
using CardBattle.Core;
using CardBattle.Dialogue;
using UnityEngine;

namespace CardBattle.UI
{
    /// <summary>
    /// 複数の台詞ブロックを保持し、画面右側で新しいものを下に追加して並べる
    /// </summary>
    public class DialogueLog : MonoBehaviour
    {
        [SerializeField] private RectTransform blockContainer;
        [SerializeField] private DialogueBlock blockPrefab;

        /// <summary>
        /// 台詞ブロックを生成し、データで初期化してログの末尾（下）に追加する
        /// </summary>
        public void AddBlock(DialogueBlockData data)
        {
            AddBlock(data, null);
        }

        /// <summary>
        /// 台詞ブロックを生成し、データとターン行動リストで初期化してログの末尾に追加する
        /// </summary>
        public void AddBlock(DialogueBlockData data, IReadOnlyList<TurnActionRecord> turnActions)
        {
            if (blockPrefab == null || blockContainer == null) return;

            var block = Instantiate(blockPrefab, blockContainer);
            block.transform.SetAsLastSibling();
            block.Initialize(data, turnActions);

            var n = blockContainer.childCount;
            for (var i = 0; i < n; i++)
            {
                var childBlock = blockContainer.GetChild(i).GetComponent<DialogueBlock>();
                if (childBlock != null)
                {
                    var alpha = Mathf.Clamp01(1f - (n - 1 - i) * 0.2f);
                    childBlock.SetAlpha(alpha);
                }
            }
        }
    }
}
