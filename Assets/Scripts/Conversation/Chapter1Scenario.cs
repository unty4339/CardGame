using System.Collections;
using UnityEngine;
using CardBattle.UI;

namespace CardBattle.Conversation
{
    /// <summary>
    /// 第1章のサンプルシナリオ。リュシアとオークの会話例。
    /// Show の引数は Addressables のアドレス文字列。StandingPictureType の定数か、
    /// ノベル用に追加したアドレス（例: "Assets/Images/リュシア.png"）を指定する。
    /// </summary>
    public class Chapter1Scenario : ScenarioBase
    {
        public override IEnumerator RunScenario()
        {
            var rysia = Actor("Rysia");
            var ogre = Actor("Ogre");
            if (rysia == null || ogre == null)
                yield break;

            // --- 1クリック目 ---
            yield return rysia.Show(StandingPictureType.Normal);

            yield return CommentAndWait("リュシア", "こんにちは、司令官！");

            // --- 2クリック目 ---
            rysia.SetFocus(true);
            ogre.SetFocus(false);
            yield return rysia.JumpAnimation();

            yield return CommentAndWait("リュシア", "今日はいい天気ですね！");

            // --- 3クリック目 ---
            yield return ogre.Show(StandingPictureType.Submission);

            rysia.SetFocus(false);
            ogre.SetFocus(true);

            yield return CommentAndWait("オーク", "グガァ……（腹減った）");
        }
    }
}
