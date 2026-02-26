using System.Collections;
using System.Collections.Generic;
using CardBattle.Core;
using CardBattle.Dialogue;
using UnityEngine;

namespace CardBattle.UI
{
    /// <summary>
    /// 複数の台詞ブロックを保持し、画面右側で新しいものを下に追加して並べる。
    /// レイアウトはクラス内で位置・透明度を直接制御し、新規台詞は下から出現・既存は上に移動して少し透明に・一定数超えた古い台詞は消滅する。
    /// </summary>
    public class DialogueLog : MonoBehaviour
    {
        [SerializeField] private RectTransform blockContainer;
        [SerializeField] private DialogueBlock blockPrefab;

        // [Tooltip("1ブロックの幅")]
        private float blockWidth = 350f;

        // [Tooltip("1ブロックの高さ")]
        private float blockHeight = 60f;

        // [Tooltip("ブロック間の余白")]
        private float blockSpacing = 4f;

        // [Tooltip("位置・透明度の補間時間（秒）")]
        private float animationDuration = 0.2f;

        // [Tooltip("保持する最大ブロック数。超えた分は消滅させる")]
        private int maxBlockCount = 6;

        // [Tooltip("1本古いごとの alpha 倍率（例: 0.85 で少しずつ透明）")]
        // [Range(0.5f, 1f)]
        private float alphaDecayPerStep = 0.7f;

        private readonly List<float> _currentY = new List<float>();
        private readonly List<float> _currentAlpha = new List<float>();
        private Coroutine _layoutCoroutine;

        private void OnEnable()
        {
            EnsureLayoutCoroutineRunning();
        }

        private void OnDisable()
        {
            if (_layoutCoroutine != null)
            {
                StopCoroutine(_layoutCoroutine);
                _layoutCoroutine = null;
            }
        }

        /// <summary>
        /// 台詞ブロックを生成し、データで初期化してログの末尾（下）に追加する
        /// </summary>
        public void AddBlock(DialogueBlockData data)
        {
            AddBlock(data, null);
        }

        /// <summary>
        /// 台詞ブロックを生成し、データとターン行動リストで初期化してログの末尾に追加する
        /// </summary>
        public void AddBlock(DialogueBlockData data, IReadOnlyList<TurnActionRecord> turnActions)
        {
            if (blockPrefab == null || blockContainer == null) return;

            var block = Instantiate(blockPrefab, blockContainer);
            block.transform.SetAsLastSibling();
            block.Initialize(data, turnActions);

            SetBlockRectAnchorBottom(block.transform as RectTransform);
            _currentY.Add(-(blockHeight + blockSpacing));
            _currentAlpha.Add(1f);

            EnsureLayoutCoroutineRunning();
        }

        private void SetBlockRectAnchorBottom(RectTransform rect)
        {
            if (rect == null) return;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(blockWidth, blockHeight);
        }

        private void EnsureLayoutCoroutineRunning()
        {
            if (_layoutCoroutine == null && isActiveAndEnabled)
                _layoutCoroutine = StartCoroutine(LayoutCoroutine());
        }

        private float TargetAlphaForIndex(int index, int totalCount)
        {
            var stepsFromNewest = totalCount - 1 - index;
            return Mathf.Clamp01(Mathf.Pow(alphaDecayPerStep, stepsFromNewest));
        }

        private IEnumerator LayoutCoroutine()
        {
            var speed = 1f / Mathf.Max(0.01f, animationDuration);

            while (true)
            {
                var childCount = blockContainer.childCount;
                if (childCount == 0 && _currentY.Count == 0)
                {
                    _layoutCoroutine = null;
                    yield break;
                }

                if (childCount > maxBlockCount)
                {
                    Destroy(blockContainer.GetChild(0).gameObject);
                    _currentY.RemoveAt(0);
                    _currentAlpha.RemoveAt(0);
                    childCount = blockContainer.childCount;
                }

                var n = _currentY.Count;
                if (n == 0)
                {
                    yield return null;
                    continue;
                }

                var step = speed * Time.deltaTime;
                var stepAlpha = Mathf.Clamp01(step * 2f);
                var offset = childCount > n ? 1 : 0;

                for (var i = 0; i < n; i++)
                {
                    var targetY = (n - 1 - i) * (blockHeight + blockSpacing);
                    var targetAlpha = TargetAlphaForIndex(i, n);

                    _currentY[i] = Mathf.Lerp(_currentY[i], targetY, step);
                    _currentAlpha[i] = Mathf.Lerp(_currentAlpha[i], targetAlpha, stepAlpha);

                    var childIndex = i + offset;
                    if (childIndex >= blockContainer.childCount) continue;

                    var child = blockContainer.GetChild(childIndex);
                    var childRect = child as RectTransform;
                    if (childRect != null)
                    {
                        var pos = childRect.anchoredPosition;
                        pos.y = _currentY[i];
                        childRect.anchoredPosition = pos;
                    }

                    var childBlock = child.GetComponent<DialogueBlock>();
                    if (childBlock != null)
                        childBlock.SetAlpha(_currentAlpha[i]);
                }

                yield return null;
            }
        }
    }
}
