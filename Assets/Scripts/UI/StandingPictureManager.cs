using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace CardBattle.UI
{
    /// <summary>
    /// 戦闘中に画面左側に立ち絵を表示し、種類の切り替え・バウンスアニメ・マウス接近時の半透明を管理する。
    /// 立ち絵は本オブジェクトの直下に UI Image として生成され、種類は Addressables のアドレス（StandingPictureType 定数）でロードする。
    /// </summary>
    public class StandingPictureManager : MonoBehaviour
    {
        public static StandingPictureManager Instance { get; private set; }

        private const float BounceOffsetY = -30f;
        private const float BounceDuration = 0.12f;
        private const float CursorTransparentZoneWidth = 500f;
        private const float TransparentAlpha = 0.5f;

        private string _currentType = StandingPictureType.None;
        private RectTransform _activeStandingPictureRect;
        private CanvasGroup _activeStandingPictureCanvasGroup;

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

        private void Update()
        {
            UpdateTransparencyByCursor();
        }

        /// <summary>
        /// 指定した種類の立ち絵に切り替える。既に対応オブジェクトがあればそれをアクティブにし、なければ Addressables でロードして作成する。他は非アクティブにする。
        /// </summary>
        /// <param name="typeId">StandingPictureType の定数（Addressables のアドレスとして使用）。None の場合は表示を消す。</param>
        public void SetStandingPicture(string typeId)
        {
            if (string.IsNullOrEmpty(typeId))
            {
                SetAllChildrenActive(false);
                _currentType = StandingPictureType.None;
                _activeStandingPictureRect = null;
                _activeStandingPictureCanvasGroup = null;
                return;
            }

            var childName = GetChildNameFromAddress(typeId);
            var existing = transform.Find(childName);
            if (existing != null)
            {
                SetAllChildrenActive(false);
                existing.gameObject.SetActive(true);
                _currentType = typeId;
                var rect = existing.GetComponent<RectTransform>();
                _activeStandingPictureRect = rect;
                _activeStandingPictureCanvasGroup = existing.GetComponent<CanvasGroup>();
                if (_activeStandingPictureCanvasGroup == null)
                    _activeStandingPictureCanvasGroup = existing.gameObject.AddComponent<CanvasGroup>();
                StartCoroutine(BounceCoroutine(rect));
                return;
            }

            StartCoroutine(CreateAndShowStandingPictureCoroutine(typeId));
        }

        /// <summary>Addressables のパスから、子オブジェクト名（拡張子なしファイル名）を取得する。</summary>
        private static string GetChildNameFromAddress(string address)
        {
            return string.IsNullOrEmpty(address) ? "" : Path.GetFileNameWithoutExtension(address);
        }

        private void SetAllChildrenActive(bool active)
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(active);
        }

        private IEnumerator CreateAndShowStandingPictureCoroutine(string typeId)
        {
            var am = AddressableManager.Instance;
            if (am == null)
            {
                Debug.LogWarning("[StandingPictureManager] AddressableManager not found.");
                yield break;
            }

            Task<Sprite> loadTask = null;
            try
            {
                loadTask = am.LoadAssetAsync<Sprite>(typeId);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[StandingPictureManager] Load failed for {typeId}: {ex.Message}");
                yield break;
            }

            yield return new WaitUntil(() => loadTask.IsCompleted);

            if (loadTask.Status != TaskStatus.RanToCompletion || loadTask.Result == null)
            {
                Debug.LogWarning($"[StandingPictureManager] Sprite load failed or null for {typeId}");
                yield break;
            }

            var sprite = loadTask.Result;
            var go = new GameObject(GetChildNameFromAddress(typeId));
            go.transform.SetParent(transform, false);

            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0f);

            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            image.preserveAspect = true;

            var cg = go.AddComponent<CanvasGroup>();

            SetAllChildrenActive(false);
            go.SetActive(true);
            _currentType = typeId;
            _activeStandingPictureRect = rect;
            _activeStandingPictureCanvasGroup = cg;

            yield return BounceCoroutine(rect);
        }

        private IEnumerator BounceCoroutine(RectTransform target)
        {
            if (target == null) yield break;
            var startY = target.anchoredPosition.y;
            var downY = startY + BounceOffsetY;

            float elapsed = 0f;
            while (elapsed < BounceDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / BounceDuration);
                target.anchoredPosition = new Vector2(target.anchoredPosition.x, Mathf.Lerp(startY, downY, t));
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < BounceDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / BounceDuration);
                target.anchoredPosition = new Vector2(target.anchoredPosition.x, Mathf.Lerp(downY, startY, t));
                yield return null;
            }

            target.anchoredPosition = new Vector2(target.anchoredPosition.x, startY);
        }

        private void UpdateTransparencyByCursor()
        {
            if (_activeStandingPictureCanvasGroup == null) return;

            float mouseX = GetMouseScreenPosition().x;
            _activeStandingPictureCanvasGroup.alpha = mouseX < CursorTransparentZoneWidth ? TransparentAlpha : 1f;
        }

        private static Vector2 GetMouseScreenPosition()
        {
            if (Mouse.current != null)
                return Mouse.current.position.ReadValue();
            return new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
    }
}
