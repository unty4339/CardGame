using System.Collections.Generic;
using CardBattle.Core;
using CardBattle.Dialogue;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.UI
{
    /// <summary>
    /// 台詞1件を表示するUI。与えられた DialogueBlockData で自身の表示を初期化する。
    /// </summary>
    public class DialogueBlock : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textLabel;
        [SerializeField] private Image bubbleImage;
        [SerializeField] private Image speakerIconImage;
        [SerializeField] private CanvasGroup canvasGroup;

        private static readonly Color NormalBubbleColor = new Color(1f, 1f, 1f, 0.95f);
        private static readonly Color ThoughtBubbleColor = new Color(0.95f, 0.95f, 1f, 0.9f);
        private static readonly Color ShoutBubbleColor = new Color(1f, 0.9f, 0.85f, 0.95f);

        /// <summary>台詞作成時に渡されたターン行動リスト（表示・テキスト生成で利用可能）</summary>
        private IReadOnlyList<TurnActionRecord> _turnActions;

        /// <summary>
        /// 与えられたデータで台詞テキスト・吹き出し・発言者アイコン・透明度を設定する
        /// </summary>
        public void Initialize(DialogueBlockData data)
        {
            Initialize(data, null);
        }

        /// <summary>
        /// 与えられたデータとターン行動リストで台詞を初期化する。行動リストは保持し、表示やテキスト生成で利用可能。
        /// </summary>
        public void Initialize(DialogueBlockData data, IReadOnlyList<TurnActionRecord> turnActions)
        {
            _turnActions = turnActions;
            if (textLabel != null)
                textLabel.text = data.content ?? string.Empty;

            if (bubbleImage != null)
            {
                bubbleImage.color = data.bubbleType switch
                {
                    DialogueBubbleType.Thought => ThoughtBubbleColor,
                    DialogueBubbleType.Shout => ShoutBubbleColor,
                    _ => NormalBubbleColor
                };
            }

            if (speakerIconImage != null)
            {
                speakerIconImage.gameObject.SetActive(!string.IsNullOrEmpty(data.speakerId));
                // 将来的に speakerId からスプライトを引く場合はここで設定
            }

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Clamp01(data.alpha);
        }

        /// <summary>
        /// 透明度を後から更新する。古いブロックほど薄くする際に使用。
        /// </summary>
        public void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Clamp01(alpha);
        }

        /// <summary>
        /// 台詞作成時に渡されたターン行動リストを返す。表示やテキスト生成で利用可能。
        /// </summary>
        public IReadOnlyList<TurnActionRecord> GetTurnActions() => _turnActions;
    }
}
