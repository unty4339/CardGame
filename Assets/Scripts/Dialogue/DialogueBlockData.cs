using System;
using UnityEngine;

namespace CardBattle.Dialogue
{
    /// <summary>
    /// 吹き出しの種類
    /// </summary>
    public enum DialogueBubbleType
    {
        Normal,
        Thought,
        Shout
    }

    /// <summary>
    /// 台詞1件の表示用データ（内容・吹き出し・発言者・透明度）
    /// </summary>
    [Serializable]
    public struct DialogueBlockData
    {
        [Tooltip("台詞の内容の文字列")]
        public string content;

        [Tooltip("吹き出しの種類")]
        public DialogueBubbleType bubbleType;

        [Tooltip("発言者ID（顔アイコン用）")]
        public string speakerId;

        [Tooltip("透明度 (0-1)")]
        [Range(0f, 1f)]
        public float alpha;

        [Tooltip("枠色を上書きするか（true のとき frameColor を使用）")]
        public bool useFrameColorOverride;

        [Tooltip("上書きする枠色（useFrameColorOverride が true のとき有効）")]
        public Color frameColor;

        public DialogueBlockData(string content, DialogueBubbleType bubbleType = DialogueBubbleType.Normal, string speakerId = null, float alpha = 1f, Color? frameColorOverride = null)
        {
            this.content = content ?? string.Empty;
            this.bubbleType = bubbleType;
            this.speakerId = speakerId ?? string.Empty;
            this.alpha = Mathf.Clamp01(alpha);
            useFrameColorOverride = frameColorOverride.HasValue;
            frameColor = frameColorOverride ?? Color.white;
        }
    }
}
