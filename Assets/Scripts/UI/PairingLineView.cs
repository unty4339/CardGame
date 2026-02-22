using UnityEngine;
using Radishmouse;

namespace CardBattle.UI
{
    /// <summary>
    /// 2点間を結ぶ白線を描画する。ペアリング中のユニットをホバーしたときに表示する。
    /// UILineRenderer を使用。同一 GameObject に UILineRenderer をアタッチし、RectTransform はフルスクリーンオーバーレイにすること。
    /// </summary>
    public class PairingLineView : MonoBehaviour
    {
        [SerializeField] private UILineRenderer lineRenderer;
        [SerializeField] private float lineThickness = 2f;

        private Transform _from;
        private Transform _to;
        private bool _visible;

        private void Awake()
        {
            if (lineRenderer == null)
                lineRenderer = GetComponent<UILineRenderer>();
            if (lineRenderer != null)
            {
                lineRenderer.color = new Color(1f, 1f, 1f, 0.9f);
                lineRenderer.thickness = lineThickness;
            }
        }

        private void LateUpdate()
        {
            if (!_visible || _from == null || _to == null || lineRenderer == null)
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

            var rect = lineRenderer.rectTransform;
            Camera cam = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, fromPos, cam, out var localFrom))
                return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, toPos, cam, out var localTo))
                return;

            lineRenderer.points = new Vector2[] { localFrom, localTo };
            lineRenderer.SetVerticesDirty();
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
            if (lineRenderer != null)
                lineRenderer.enabled = true;
        }

        /// <summary>
        /// 白線を非表示にする。
        /// </summary>
        public void Hide()
        {
            _visible = false;
            _from = null;
            _to = null;
            if (lineRenderer != null)
            {
                lineRenderer.points = new Vector2[0];
                lineRenderer.SetVerticesDirty();
                lineRenderer.enabled = false;
            }
            gameObject.SetActive(false);
        }
    }
}
