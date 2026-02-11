using UnityEngine;
using UnityEngine.Video;

public class VideoEffectManager : MonoBehaviour
{
    // どこからでもアクセスできるようにする（シングルトン）
    public static VideoEffectManager Instance;

    // UIの親要素（Canvasなど）を指定
    public Transform uiParent;

    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 指定した座標にエフェクトを生成して再生する
    /// </summary>
    /// <param name="prefab">再生したい演出のプレハブ</param>
    /// <param name="screenPosition">再生したい画面座標 (Vector2)</param>
    /// <param name="playbackSpeed">再生速度（デフォルト 1f）</param>
    /// <param name="startTime">再生開始位置（秒、デフォルト 0）</param>
    public void PlayEffect(GameObject prefab, Vector2 screenPosition, float playbackSpeed = 1f, double startTime = 0)
    {
        // プレハブを生成
        GameObject effectGo = Instantiate(prefab, uiParent);
        
        // 座標を設定
        RectTransform rect = effectGo.GetComponent<RectTransform>();
        rect.anchoredPosition = screenPosition;

        // 再生開始
        VideoPlayer vp = effectGo.GetComponent<VideoPlayer>();
        vp.playbackSpeed = playbackSpeed;
        vp.time = startTime;
        vp.Play();

        // 動画終了時に自動で消去する設定（使い捨ての場合）
        vp.loopPointReached += (source) => {
            Destroy(effectGo);
        };
    }
}