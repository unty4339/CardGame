using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.UI
{
    /// <summary>
    /// 画面右側にカード説明文を表示するパネル。マウスオーバー中のみ表示し、それ以外は非表示。
    /// </summary>
    public class CardDescriptionPanel : MonoBehaviour
    {
        public static CardDescriptionPanel Instance { get; private set; }

        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private CanvasGroup canvasGroup;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (descriptionText != null)
            {
                descriptionText.enableAutoSizing = true;
                descriptionText.fontSizeMin = 5f;
                descriptionText.fontSizeMax = 12f;
            }
            Hide();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// 説明文を表示する。空の場合は非表示のまま。
        /// </summary>
        public void Show(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Hide();
                return;
            }
            if (descriptionText != null)
                descriptionText.text = text;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = false;
            }
            if (panelRoot != null)
                panelRoot.gameObject.SetActive(true);
        }

        /// <summary>
        /// パネルを非表示にする。マウスオーバーしていないときは常に呼ぶ。
        /// </summary>
        public void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
            }
            if (panelRoot != null)
                panelRoot.gameObject.SetActive(false);
        }
    }
}
