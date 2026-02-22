using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.UI
{
    /// <summary>
    /// 2点間を結ぶ白線を描画する。ペアリング中のユニットをホバーしたときに表示する。
    /// </summary>
    public class PairingLineView : MonoBehaviour
    {
        [SerializeField] private RectTransform lineRect;
        [SerializeField] private Image lineImage;
        [SerializeField] private float lineThickness = 2f;

        private Transform _from;
        private Transform _to;
        private bool _visible;
        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            if (lineRect == null)
                lineRect = _rectTransform;
            if (lineImage == null)
                lineImage = GetComponent<Image>();
            if (lineImage != null)
                lineImage.color = new Color(1f, 1f, 1f, 0.9f);
        }

        private void LateUpdate()
        {
            if (!_visible || _from == null || _to == null)
                return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            var fromPos = _from.position;
            var toPos = _to.position;

            if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera != null)
            {
                fromPos = canvas.worldCamera.WorldToScreenPoint(fromPos);
                toPos = canvas.worldCamera.WorldToScreenPoint(toPos);
            }

            var dir = (Vector2)(toPos - fromPos);
            var length = dir.magnitude;
            if (length < 0.1f) length = 0.1f;

            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            if (lineRect == null) return;
            lineRect.pivot = new Vector2(0f, 0.5f);
            lineRect.anchorMin = new Vector2(0f, 0.5f);
            lineRect.anchorMax = new Vector2(0f, 0.5f);
            lineRect.position = fromPos;
            lineRect.sizeDelta = new Vector2(length, lineThickness);
            lineRect.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        /// <summary>
        /// 2つの Transform の間に白線を表示する。
        /// </summary>
        public void Show(Transform from, Transform to)
        {
            _from = from;
            _to = to;
            _visible = true;
            gameObject.SetActive(true);
            if (lineImage != null)
                lineImage.enabled = true;
        }

        /// <summary>
        /// 白線を非表示にする。
        /// </summary>
        public void Hide()
        {
            _visible = false;
            _from = null;
            _to = null;
            gameObject.SetActive(false);
            if (lineImage != null)
                lineImage.enabled = false;
        }
    }
}
