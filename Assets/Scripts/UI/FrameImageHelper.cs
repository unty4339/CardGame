using System.Collections;
using System.Threading.Tasks;
using CardBattle.Core.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.UI
{
    /// <summary>
    /// カード・ユニットのフレーム枠用スプライトを Addressables で読み込むヘルパー
    /// </summary>
    public static class FrameImageHelper
    {
        private const string ImagesPath = "Assets/Images/";

        private static string GetFrameFileName(FrameType frameType)
        {
            return frameType switch
            {
                FrameType.Unit => "ユニットフレーム.png",
                FrameType.Spell => "スペルフレーム.png",
                FrameType.Partner => "パートナーフレーム.png",
                _ => "ユニットフレーム.png"
            };
        }

        /// <summary>
        /// 指定したフレーム種別のスプライトを非同期で読み込み、target に設定する
        /// </summary>
        public static IEnumerator LoadFrameAsync(FrameType frameType, Image target)
        {
            if (target == null) yield break;

            var fileName = GetFrameFileName(frameType);
            var address = ImagesPath + fileName;
            var am = AddressableManager.Instance;
            if (am == null) yield break;

            var hasTask = am.HasAssetAsync(address);
            yield return new WaitUntil(() => hasTask.IsCompleted);
            if (!hasTask.Result) yield break;

            Task<Sprite> loadTask = null;
            try
            {
                loadTask = am.LoadAssetAsync<Sprite>(address);
            }
            catch
            {
                yield break;
            }

            yield return new WaitUntil(() => loadTask.IsCompleted);
            if (loadTask.Status == TaskStatus.RanToCompletion && loadTask.Result != null && target != null)
                target.sprite = loadTask.Result;
        }
    }
}
