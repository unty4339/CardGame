using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using CardBattle.UI;

namespace CardBattle.Conversation
{
    /// <summary>
    /// 会話.md に従うオープニング会話シナリオ。背景1→ナレーション→背景2→ユウ・リュシアの会話→TitleScene へ遷移。
    /// シーンをプレイすると自動で再生される。
    /// </summary>
    public class OpeningScenario : ScenarioBase
    {
        private const string YuuSpritePath = "Assets/Images/Stand/ユウ.png";
        private const string TitleSceneName = "TitleScene";

        public override IEnumerator RunScenario()
        {
            var rysia = Actor("リュシア");
            var yuu = Actor("ユウ");
            if (rysia == null || yuu == null)
                yield break;

            // --- 背景1・ナレーション（話者空欄） ---
            yield return SetBackgroundAndWait("背景1");

            yield return CommentAndWait("", "新作カードゲーム「スレイバース」の流行から早数年。\nその勢いは留まるところを知らず、今やスレバは日常の一部になっている。");
            yield return CommentAndWait("", "そんな中、ひとつの噂がスレバ界隈の猛者たちを賑わせていた。\nカードに選ばれし一握りのマスターだけは精霊の姿が見えるというのだ。");
            yield return CommentAndWait("", "荒唐無稽な噂を信じる者はそう多くないが、夢のある話には違いない。\nカードの精霊の伝説はゆっくりと、しかし着実に人々の間に広まっていった。");

            // --- 背景2 ---
            yield return SetBackgroundAndWait("背景2");

            // --- ユウ・リュシアの会話 ---
            yield return yuu.Show(YuuSpritePath);
            yuu.SetFocus(true);
            rysia.SetFocus(false);
            yield return CommentAndWait("ユウ", "このカードは4枚入れて……こっちは2枚でいいかな。");

            yield return rysia.Show(StandingPictureType.Normal);
            rysia.SetFocus(true);
            yuu.SetFocus(false);
            yield return CommentAndWait("リュシア", "ユウくん、またデッキを調整しているの？");

            yield return yuu.Show(YuuSpritePath);
            yuu.SetFocus(true);
            rysia.SetFocus(false);
            yield return CommentAndWait("ユウ", "今日は近所のショップで友達とスレバ対決する約束だからな！\nなるべく家を出る直前までデッキは弄っておきたいんだ。");

            yield return rysia.Show(StandingPictureType.Embarrassed);
            yield return SetBackgroundAndWait("背景3");
            rysia.SetFocus(true);
            yuu.SetFocus(false);
            yield return CommentAndWait("リュシア", "う～ん、さすがは未来のスレバマスターだね。\n……ちなみにボクも入ってるんだよね？");

            yield return yuu.Show(YuuSpritePath);
            yuu.SetFocus(true);
            rysia.SetFocus(false);
            yield return CommentAndWait("ユウ", "ああ、もちろん！\nリュシアを軸に相性のいいカードで固めた編成を考えてるからな！");

            yield return rysia.Show(StandingPictureType.Normal);
            rysia.SetFocus(true);
            yuu.SetFocus(false);
            yield return CommentAndWait("リュシア", "（スレイバースのことになるとすぐ夢中になるんだから。\n　でも……とても頼りになる子だよね。この子と出会えてよかった）");

            yield return yuu.Show(YuuSpritePath);
            yield return SetBackgroundAndWait("背景4");
            yuu.SetFocus(true);
            rysia.SetFocus(false);
            yield return CommentAndWait("ユウ", "　ゴブリンとオークを混ぜたグリーンスキンデッキの完成だ～！\n　今日のバトルはこいつを回すぜ！");

            yield return rysia.Show(StandingPictureType.Normal);
            rysia.SetFocus(true);
            yuu.SetFocus(false);
            yield return CommentAndWait("リュシア", "　……。");

            yield return rysia.Show(StandingPictureType.Anxious);
            rysia.SetFocus(true);
            yuu.SetFocus(false);
            yield return CommentAndWait("リュシア", "　……え～と、ユウくん？　ちょっとボクと世界観の方向性が違くない？\n　ホントに相性いいのかな～なんて。もっとこう、SFっぽいほうが……。");

            yield return rysia.Show(StandingPictureType.Embarrassed);
            rysia.SetFocus(true);
            yuu.SetFocus(false);
            yield return CommentAndWait("リュシア", "　あっいや、文句とかじゃないけどね。\n　もちろんユウくんがいいならボクは構わないけれど。一応の確認ね。");

            yield return yuu.Show(YuuSpritePath);
            yuu.SetFocus(true);
            rysia.SetFocus(false);
            yield return CommentAndWait("ユウ", "　んー……オレはあまりイラストとか気にしないタイプだからな～。\n　もちろん拘りは大切だけど、まずは勝てるデッキじゃねーとな！");

            yield return rysia.Show(StandingPictureType.Embarrassed);
            rysia.SetFocus(true);
            yuu.SetFocus(false);
            yield return CommentAndWait("リュシア", "　そ……そっか、そうだよね、うん！\n　今日は一緒に勝とうね！　ふたりの絆のパワーで！！");

            yield return yuu.Show(YuuSpritePath);
            yield return SetBackgroundAndWait("背景2");
            yuu.SetFocus(true);
            rysia.SetFocus(false);
            yield return CommentAndWait("ユウ", "　おう！　じゃあ出かける準備をするぜ！");

            yield return rysia.Show(StandingPictureType.Anxious);
            rysia.SetFocus(true);
            yuu.SetFocus(false);
            yield return CommentAndWait("リュシア", "（……何事もなければいいけど……）");

            // --- TitleScene に遷移 ---
            SceneManager.LoadScene(TitleSceneName);
        }
    }
}
