using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CardBattle.Conversation
{
    /// <summary>
    /// ノベル会話シーン全体を管理する。UIテキストの更新、立ち絵アクターの取得・生成、クリック待ち、背景表示を提供する。
    /// </summary>
    public class ConversationManager : MonoBehaviour
    {
        private const string BackgroundImagesPath = "Assets/Images/";
        private const string BackgroundExtension = ".jpg";

        public static ConversationManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Transform actorRoot;
        [SerializeField] private StandingPictureActor actorPrefab;
        [SerializeField] private Image backgroundImage;

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
        /// 背景キー（例: "背景1"）を Addressables のアドレス（例: "Assets/Images/背景1.jpg"）に変換する。
        /// </summary>
        private static string ToBackgroundAddress(string key)
        {
            return string.IsNullOrEmpty(key) ? "" : BackgroundImagesPath + key.Trim() + BackgroundExtension;
        }

        /// <summary>
        /// 指定したキーの背景を Addressables でロードして表示する。完了まで待機可能。
        /// キー例: "背景1" → "Assets/Images/背景1.jpg" をロードする。
        /// </summary>
        public IEnumerator SetBackgroundAndWait(string key)
        {
            if (backgroundImage == null)
                yield break;

            if (string.IsNullOrEmpty(key))
            {
                backgroundImage.sprite = null;
                backgroundImage.enabled = false;
                yield break;
            }

            string address = ToBackgroundAddress(key);
            Sprite loadedSprite = null;
            var am = AddressableManager.Instance;
            if (am != null)
            {
                Task<Sprite> spriteTask = null;
                try
                {
                    spriteTask = am.LoadAssetAsync<Sprite>(address);
                }
                catch (Exception)
                {
                    // Sprite で見つからない場合は Texture2D で試す
                }

                if (spriteTask != null)
                {
                    yield return new WaitUntil(() => spriteTask.IsCompleted);
                    if (spriteTask.Status == TaskStatus.RanToCompletion && spriteTask.Result != null)
                        loadedSprite = spriteTask.Result;
                }

                if (loadedSprite == null)
                {
                    Task<Texture2D> textureTask = null;
                    try
                    {
                        textureTask = am.LoadAssetAsync<Texture2D>(address);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[ConversationManager] 背景のロードに失敗しました: {address}. {ex.Message}");
                    }
                    if (textureTask != null)
                    {
                        yield return new WaitUntil(() => textureTask.IsCompleted);
                        try
                        {
                            if (textureTask.Status == TaskStatus.RanToCompletion && textureTask.Result != null)
                            {
                                var tex = textureTask.Result;
                                loadedSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[ConversationManager] 背景の処理に失敗しました: {address}. {ex.Message}");
                        }
                    }
                }
            }

            if (loadedSprite != null)
            {
                backgroundImage.sprite = loadedSprite;
                backgroundImage.enabled = true;
            }
        }

        /// <summary>
        /// クリック待ち用（離したタイミングで反応）。yield return Manager.WaitForClick で1回離すまで待機できる。
        /// </summary>
        public CustomYieldInstruction WaitForClick => new WaitUntil(() => Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame);
    }
}
