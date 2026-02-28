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

        /// <summary>通常に切り替える。ボタンの OnClick から呼ぶ。</summary>
        public void SetStandingPictureNormal()
        {
            StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.Normal);
        }

        /// <summary>照れに切り替える。ボタンの OnClick から呼ぶ。</summary>
        public void SetStandingPictureEmbarrassed()
        {
            StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.Embarrassed);
        }

        /// <summary>焦りに切り替える。ボタンの OnClick から呼ぶ。</summary>
        public void SetStandingPictureAnxious()
        {
            StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.Anxious);
        }

        /// <summary>銃構えに切り替える。ボタンの OnClick から呼ぶ。</summary>
        public void SetStandingPictureGunStance()
        {
            StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.GunStance);
        }

        /// <summary>銃構え照れに切り替える。ボタンの OnClick から呼ぶ。</summary>
        public void SetStandingPictureGunStanceEmbarrassed()
        {
            StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.GunStanceEmbarrassed);
        }

        /// <summary>ペアリング用・バックに切り替える。ボタンの OnClick から呼ぶ。</summary>
        public void SetStandingPictureBack()
        {
            StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.Back);
        }

        /// <summary>ペアリング用・騎乗に切り替える。ボタンの OnClick から呼ぶ。</summary>
        public void SetStandingPictureRiding()
        {
            StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.Riding);
        }

        /// <summary>ペアリング用・オークに切り替える。ボタンの OnClick から呼ぶ。</summary>
        public void SetStandingPictureOgre()
        {
            StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.Ogre);
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
