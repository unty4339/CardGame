using CardBattle.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace CardBattle.UI
{
    /// <summary>
    /// AI行動のディレイを切り替える高速モードボタン。
    /// Button の On Click () に OnHighSpeedClicked を指定して使用する。
    /// </summary>
    public class HighSpeedModeButton : MonoBehaviour
    {
        private const string LabelNormal = "通常";
        private const string LabelHighSpeed = "高速";

        private Text _labelText;

        private void Awake()
        {
            _labelText = GetComponentInChildren<Text>();
        }

        /// <summary>
        /// ボタンクリック時に呼ぶ。高速モードをトグルする。
        /// </summary>
        public void OnHighSpeedClicked()
        {
            var actionQueueManager = ActionQueueManager.Instance;
            if (actionQueueManager == null) return;

            actionQueueManager.HighSpeedMode = !actionQueueManager.HighSpeedMode;
            UpdateLabel(actionQueueManager.HighSpeedMode);
        }

        private void UpdateLabel(bool isHighSpeed)
        {
            if (_labelText != null)
                _labelText.text = isHighSpeed ? LabelHighSpeed : LabelNormal;
        }

        private void Start()
        {
            var actionQueueManager = ActionQueueManager.Instance;
            if (actionQueueManager != null)
                UpdateLabel(actionQueueManager.HighSpeedMode);
        }
    }
}
