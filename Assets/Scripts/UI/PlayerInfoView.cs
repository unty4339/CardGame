using CardBattle.Core.Player;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardBattle.UI
{
    /// <summary>
    /// プレイヤーのHP、MP、PP（プレイポイント）の表示更新について責任を持つ
    /// </summary>
    public class PlayerInfoView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI mpText;

        /// <summary>
        /// プレイヤーデータを受け取り、スライダーやテキストを最新の値にする
        /// </summary>
        public void UpdateState(PlayerData data)
        {
            if (data == null) return;
            if (hpText != null) hpText.text = data.HP.ToString();
            if (mpText != null) mpText.text = $"{data.CurrentMP}/{data.MaxMP}";
        }
    }
}
