using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CardBattle.UI
{
    public class BezierUIArrow : MonoBehaviour
    {
        public static BezierUIArrow Instance { get; private set; }

        [Header("設定")]
        [SerializeField] private GameObject dotPrefab;
        [SerializeField] private int dotCount = 30;
        
        [Header("膨らみの調整パラメータ")]
        [Tooltip("距離に対する膨らみの強さ")]
        [SerializeField] private float curveIntensity = 0.5f;
        [Tooltip("画面端にいくほど膨らむ最大値の倍率")]
        [SerializeField] private float positionInfluenceFactor = 1.0f;

        private List<RectTransform> _dotPool = new List<RectTransform>();
        private RectTransform _rectTransform;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _rectTransform = GetComponent<RectTransform>();
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < dotCount; i++)
            {
                var go = Instantiate(dotPrefab, transform);
                var rt = go.GetComponent<RectTransform>();
                go.SetActive(false);
                _dotPool.Add(rt);
            }
        }

        public void UpdateArrow(Vector2 start, Vector2 end)
        {
            // 1. 始点と終点の距離を計算
            float distance = Vector2.Distance(start, end);

            // 2. 始点が画面の左右どちらにあるか判定 (-1.0 ～ 1.0)
            // 画面中央なら 0、右端なら 1.0、左端なら -1.0 になる
            float screenHalfWidth = Screen.width / 2f;
            float horizontalFactor = (start.x - screenHalfWidth) / screenHalfWidth;

            // 3. 膨らみ（オフセット量）の計算
            // 「位置の影響」×「距離の影響」×「調整係数」
            float bulgeAmount = horizontalFactor * distance * curveIntensity * positionInfluenceFactor;

            // 4. 制御点の計算
            Vector2 midPoint = (start + end) / 2f;
            // X軸方向に膨らませる（右にあるほど正、左にあるほど負の方向）
            Vector2 controlPoint = midPoint + new Vector2(bulgeAmount, 0);

            // ベジェ曲線に沿ってドットを配置
            for (int i = 0; i < dotCount; i++)
            {
                float t = i / (float)(dotCount - 1);
                Vector2 pos = CalculateBezierPoint(t, start, controlPoint, end);

                _dotPool[i].gameObject.SetActive(true);
                _dotPool[i].position = pos;
                
                // 先端に向かって細くする演出
                _dotPool[i].localScale = Vector3.one * (1f - (t * 0.4f));
            }
        }

        public void Hide()
        {
            foreach (var dot in _dotPool) dot.gameObject.SetActive(false);
        }

        private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            float u = 1 - t;
            return u * u * p0 + 2 * u * t * p1 + t * t * p2;
        }
    }
}