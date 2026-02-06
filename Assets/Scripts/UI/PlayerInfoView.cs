using CardBattle.Core.Player;
using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.UI
{
    /// <summary>
    /// プレイヤーのHP、MP、PP（プレイポイント）の表示更新について責任を持つ
    /// </summary>
    public class PlayerInfoView : MonoBehaviour
    {
        [SerializeField] private Slider hpSlider;
        [SerializeField] private Text hpText;
        [SerializeField] private Slider mpSlider;
        [SerializeField] private Text mpText;
        [SerializeField] private Text ppText;
        [SerializeField] private int maxHpForSlider = 20;

        /// <summary>
        /// プレイヤーデータを受け取り、スライダーやテキストを最新の値にする
        /// </summary>
        public void UpdateState(PlayerData data)
        {
            if (data == null) return;

            if (hpSlider != null)
            {
                hpSlider.maxValue = Mathf.Max(1, maxHpForSlider);
                hpSlider.value = data.HP;
            }
            if (hpText != null) hpText.text = data.HP.ToString();

            if (mpSlider != null)
            {
                mpSlider.maxValue = Mathf.Max(1, data.MaxMP);
                mpSlider.value = data.CurrentMP;
            }
            if (mpText != null) mpText.text = $"{data.CurrentMP}/{data.MaxMP}";
            if (ppText != null) ppText.text = data.CurrentMP.ToString();
        }
    }
}
