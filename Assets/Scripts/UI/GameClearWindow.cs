using UnityEngine;
using UnityEngine.SceneManagement;

namespace CardBattle.UI
{
    /// <summary>
    /// ゲームクリア時に表示するウィンドウ用UI。
    /// 立ち絵切り替えボタン・シーン遷移ボタンから呼び出すメソッドを提供する。ボタン設定はインスペクターで行う。
    /// </summary>
    public class GameClearWindow : MonoBehaviour
    {
        /// <summary>
        /// シーン遷移先のシーン名。こちらで設定してください。
        /// </summary>
        public const string TargetSceneName = "TitleScene";

        /// <summary>通常立ち絵に切り替える。ボタンの OnClick から呼ぶ。</summary>
        public void SetStandingPictureNormal()
        {
            StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.Normal);
        }

        /// <summary>汗かき立ち絵に切り替える。ボタンの OnClick から呼ぶ。</summary>
        public void SetStandingPictureSweating()
        {
            StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.Sweating);
        }

        /// <summary>屈服立ち絵に切り替える。ボタンの OnClick から呼ぶ。</summary>
        public void SetStandingPictureSubmission()
        {
            StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.Submission);
        }

        /// <summary>拘束立ち絵に切り替える。ボタンの OnClick から呼ぶ。</summary>
        public void SetStandingPictureRestraint()
        {
            StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.Restraint);
        }

        /// <summary>騎乗立ち絵に切り替える。ボタンの OnClick から呼ぶ。</summary>
        public void SetStandingPictureRiding()
        {
            StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.Riding);
        }

        /// <summary>
        /// TargetSceneName で指定したシーンへ遷移する。ボタンの OnClick から呼ぶ。
        /// </summary>
        public void LoadTargetScene()
        {
            SceneManager.LoadScene(TargetSceneName);
        }
    }
}
