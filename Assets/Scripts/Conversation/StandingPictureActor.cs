using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.Conversation
{
    /// <summary>
    /// ノベル用の立ち絵1体を制御する。表示・フォーカス（明暗）・ジャンプアニメを担当する。
    /// </summary>
    public class StandingPictureActor : MonoBehaviour
    {
        [SerializeField] private Image uiImage;
        [SerializeField] private CanvasGroup canvasGroup;

        private RectTransform _rectTransform;
        private Vector2 _defaultPosition;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _defaultPosition = _rectTransform.anchoredPosition;
        }

        /// <summary>
        /// 画像を Addressables からロードして表示する。フェードイン完了まで待つ。
        /// </summary>
        /// <param name="spriteKey">Addressables のアドレス（StandingPictureType 定数やノベル用アドレス）</param>
        public IEnumerator Show(string spriteKey)
        {
            gameObject.SetActive(true);

            Sprite loadedSprite = null;
            var am = AddressableManager.Instance;
            if (am != null && !string.IsNullOrEmpty(spriteKey))
            {
                Task<Sprite> loadTask = null;
                try
                {
                    loadTask = am.LoadAssetAsync<Sprite>(spriteKey);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[StandingPictureActor] Load failed for {spriteKey}: {ex.Message}");
                }

                if (loadTask != null)
                {
                    yield return new WaitUntil(() => loadTask.IsCompleted);
                    if (loadTask.Status == TaskStatus.RanToCompletion && loadTask.Result != null)
                        loadedSprite = loadTask.Result;
                    else
                        Debug.LogWarning($"[StandingPictureActor] Sprite load failed or null for {spriteKey}");
                }
            }

            if (uiImage != null)
                uiImage.sprite = loadedSprite;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                float time = 0f;
                while (time < 0.3f)
                {
                    time += Time.deltaTime;
                    canvasGroup.alpha = time / 0.3f;
                    yield return null;
                }
                canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// 明るさを変更する（フォーカス状態の表現）。話者を強調するときに使用する。
        /// </summary>
        public void SetFocus(bool isFocused)
        {
            if (uiImage == null) return;
            uiImage.color = isFocused ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f);
        }

        /// <summary>
        /// ぴょんと跳ねるアニメーション。完了まで待機可能。
        /// </summary>
        public IEnumerator JumpAnimation()
        {
            if (_rectTransform == null) yield break;

            float startY = _defaultPosition.y;
            float jumpHeight = 30f;
            float duration = 0.2f;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;
                float yOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;
                _rectTransform.anchoredPosition = new Vector2(_defaultPosition.x, startY + yOffset);
                yield return null;
            }
            _rectTransform.anchoredPosition = _defaultPosition;
        }
    }
}
