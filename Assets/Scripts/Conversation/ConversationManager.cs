using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CardBattle.Conversation
{
    /// <summary>
    /// ノベル会話シーン全体を管理する。UIテキストの更新、立ち絵アクターの取得・生成、クリック待ちを提供する。
    /// </summary>
    public class ConversationManager : MonoBehaviour
    {
        public static ConversationManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Transform actorRoot;
        [SerializeField] private StandingPictureActor actorPrefab;

        [Header("Typewriter")]
        [SerializeField] private float secondsPerCharacter = 0.06f;

        private Dictionary<string, StandingPictureActor> _actors = new Dictionary<string, StandingPictureActor>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// 指定したIDの立ち絵アクターを取得する。なければプレハブから生成して登録する。
        /// </summary>
        public StandingPictureActor GetActor(string actorId)
        {
            if (_actors.TryGetValue(actorId, out var actor))
                return actor;

            if (actorPrefab == null || actorRoot == null)
            {
                Debug.LogWarning("[ConversationManager] actorPrefab or actorRoot is not assigned.");
                return null;
            }

            var newActor = Instantiate(actorPrefab, actorRoot);
            newActor.name = $"Actor_{actorId}";
            _actors[actorId] = newActor;
            return newActor;
        }

        /// <summary>
        /// 台詞エリアに話者名と本文を表示する。
        /// </summary>
        public void SetDialogue(string speaker, string text)
        {
            if (speakerNameText != null)
                speakerNameText.text = speaker ?? string.Empty;
            if (bodyText != null)
                bodyText.text = text ?? string.Empty;
        }

        /// <summary>
        /// 本文のみ更新する。タイプライター表示用。
        /// </summary>
        public void SetBodyText(string text)
        {
            if (bodyText != null)
                bodyText.text = text ?? string.Empty;
        }

        /// <summary>
        /// 台詞を1文字ずつ表示する。離したタイミングで反応：表示中に離すと全文表示、表示完了後に離すと次へ進む。
        /// 表示中に押したまま表示が終わった場合は、その後の1回目の離しでは進まず、もう一度離すと次へ進む。
        /// </summary>
        public IEnumerator ShowDialogueAnimated(string speaker, string text)
        {
            if (speakerNameText != null)
                speakerNameText.text = speaker ?? string.Empty;
            SetBodyText("");

            text ??= string.Empty;
            float interval = Mathf.Max(0.01f, secondsPerCharacter);

            for (int i = 0; i <= text.Length; i++)
            {
                SetBodyText(text.Substring(0, i));
                if (i >= text.Length)
                    break;

                float elapsed = 0f;
                while (elapsed < interval)
                {
                    if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
                    {
                        SetBodyText(text);
                        yield return null; // スキップした離しを消費し、次の離し待ちで同じ離しが使われないようにする
                        yield return WaitForClick;
                        yield break;
                    }
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            // 表示完了時点でボタンが押されたままなら、その後の1回目の離しでは進めず、2回目の離しで進む
            bool ignoreNextRelease = Mouse.current != null && Mouse.current.leftButton.isPressed;
            while (true)
            {
                yield return WaitForClick;
                if (!ignoreNextRelease)
                    break;
                ignoreNextRelease = false;
            }
        }

        /// <summary>
        /// クリック待ち用（離したタイミングで反応）。yield return Manager.WaitForClick で1回離すまで待機できる。
        /// </summary>
        public CustomYieldInstruction WaitForClick => new WaitUntil(() => Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame);
    }
}
