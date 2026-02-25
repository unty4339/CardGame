using System.Collections;
using UnityEngine;
using CardBattle.UI;

namespace CardBattle.Conversation
{
    /// <summary>
    /// 各種メソッド（Show, SetFocus, JumpAnimation, Comment, WaitClick）を用いたサンプル会話シナリオ。
    /// 会話シーンを Play すると自動で再生される。
    /// </summary>
    public class SampleConversationScenario : ScenarioBase
    {
        public override IEnumerator RunScenario()
        {
            var rysia = Actor("Rysia");
            var ogre = Actor("Ogre");
            if (rysia == null || ogre == null)
                yield break;

            // --- 1人目表示 ---
            yield return rysia.Show(StandingPictureType.Normal);
            rysia.SetFocus(true);
            ogre.SetFocus(false);

            yield return CommentAndWait("リュシア", "こんにちは、司令官！今日もよろしくお願いします。");

            // --- フォーカス＋ジャンプ ---
            yield return rysia.JumpAnimation();
            yield return CommentAndWait("リュシア", "今日はいい天気ですね！");

            // --- 短い間 ---
            yield return new WaitForSeconds(0.3f);

            // --- 2人目表示 ---
            yield return ogre.Show(StandingPictureType.Submission);
            rysia.SetFocus(false);
            ogre.SetFocus(true);

            yield return CommentAndWait("オーク", "グガァ……（腹減った）");

            // --- 話者切り替え（1人目にフォーカス戻す） ---
            rysia.SetFocus(true);
            ogre.SetFocus(false);
            yield return CommentAndWait("リュシア", "ちょ、ちょっと落ち着いて！");

            yield return rysia.JumpAnimation();
            yield return CommentAndWait("リュシア", "……まあ、今日も一緒に頑張りましょう。");
        }
    }
}
