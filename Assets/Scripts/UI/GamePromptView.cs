using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardBattle.UI
{
    /// <summary>
    /// プロンプトの表示・非表示・文言切り替え・フェードアウトを担当する。
    /// 対象選択の持続表示と、マナ不足・攻撃権なし・対象なしなどの一時メッセージを一元化する。
    /// </summary>
    public class GamePromptView : MonoBehaviour
    {
        public static GamePromptView Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private CanvasGroup canvasGroup;
        [Tooltip("フェードメッセージの表示時間（秒）")]
        [SerializeField] private float fadingDisplayDuration = 1.5f;
        [Tooltip("フェードアウトにかける時間（秒）")]
        [SerializeField] private float fadingOutDuration = 0.5f;

        private const string TargetSelectionMessage = "効果の対象を選択してください";
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// 対象選択用の持続表示。「効果の対象を選択してください」を表示する。
        /// </summary>
        public void ShowTargetSelectionPrompt()
        {
            StopFadeCoroutineIfRunning();
            if (messageText != null)
                messageText.text = TargetSelectionMessage;
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 対象選択プロンプトを非表示にする。
        /// </summary>
        public void HideTargetSelectionPrompt()
        {
            StopFadeCoroutineIfRunning();
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 指定文言を表示し、一定時間後に少しずつ薄れて消す。
        /// </summary>
        public void ShowFadingMessage(string message)
        {
            Debug.Log("[GamePromptView] ShowFadingMessage: " + message);
            StopFadeCoroutineIfRunning();
            if (messageText != null)
                messageText.text = message;
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
            gameObject.SetActive(true);
            _fadeCoroutine = StartCoroutine(FadeOutCoroutine());
        }

        private void StopFadeCoroutineIfRunning()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }

        private IEnumerator FadeOutCoroutine()
        {
            yield return new WaitForSeconds(fadingDisplayDuration);
            if (canvasGroup == null)
            {
                gameObject.SetActive(false);
                _fadeCoroutine = null;
                yield break;
            }
            float elapsed = 0f;
            while (elapsed < fadingOutDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadingOutDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            _fadeCoroutine = null;
        }
    }
}
