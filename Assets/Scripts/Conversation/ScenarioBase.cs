using System.Collections;
using UnityEngine;

namespace CardBattle.Conversation
{
    /// <summary>
    /// 全ての会話シナリオはこのクラスを継承して作る。yield return で進行・待機を直感的に記述できる。
    /// </summary>
    public abstract class ScenarioBase : MonoBehaviour
    {
        protected ConversationManager Manager => ConversationManager.Instance;

        /// <summary>
        /// クリック待ち。yield return WaitClick; と書ける。
        /// </summary>
        protected object WaitClick => Manager.WaitForClick;

        /// <summary>
        /// 台詞を表示する（話者名と本文を設定）。即時表示。
        /// </summary>
        protected void Comment(string talker, string text)
        {
            Manager.SetDialogue(talker, text);
        }

        /// <summary>
        /// 台詞を1文字ずつ表示し、表示中クリックで全文表示・表示完了後のクリックで次へ進む。
        /// </summary>
        protected IEnumerator CommentAndWait(string talker, string text)
        {
            yield return Manager.ShowDialogueAnimated(talker, text);
        }

        /// <summary>
        /// 立ち絵アクターを取得する。取得したオブジェクトで Show / SetFocus / JumpAnimation などを呼ぶ。
        /// </summary>
        protected StandingPictureActor Actor(string id)
        {
            return Manager.GetActor(id);
        }

        /// <summary>
        /// 背景を指定キーで表示する。キー例: "背景1" → Assets/Images/背景1.jpg を Addressables でロードする。完了まで待機可能。
        /// </summary>
        protected IEnumerator SetBackgroundAndWait(string key)
        {
            yield return Manager.SetBackgroundAndWait(key);
        }

        /// <summary>
        /// シナリオのメイン処理。継承先で yield return を使って会話の流れを記述する。
        /// </summary>
        public abstract IEnumerator RunScenario();

        private void Start()
        {
            StartCoroutine(RunScenario());
        }
    }
}
