using System.Collections.Generic;
using UnityEngine;

namespace CardBattle.UI
{
    /// <summary>
    /// フィールド上のユニット配置の管理について責任を持つ
    /// </summary>
    public class FieldVisualizer : MonoBehaviour
    {
        [SerializeField] private RectTransform fieldAreaRect;
        [SerializeField] private float unitSpacing = 120f;
        [SerializeField] private int maxSlots = 7;

        private readonly List<UnitView> _units = new();

        /// <summary>
        /// ドロップ判定用のフィールドエリア
        /// </summary>
        public RectTransform FieldAreaRect => fieldAreaRect;

        /// <summary>
        /// 次にユニットが出るべき場所（ローカル座標）を返す
        /// </summary>
        public Vector3 GetNextSpawnPosition()
        {
            var index = _units.Count;
            return GetSlotPosition(index);
        }

        /// <summary>
        /// UnitViewを受け取り、フィールドに追加し、並べ直す
        /// </summary>
        public void AddUnit(UnitView unitView)
        {
            if (unitView == null) return;
            _units.Add(unitView);
            unitView.transform.SetParent(transform, false);
            UpdateLayout();
        }

        private Vector3 GetSlotPosition(int index)
        {
            var count = Mathf.Min(index + 1, maxSlots);
            var totalWidth = (count - 1) * unitSpacing;
            var startX = -totalWidth * 0.5f;
            var x = startX + index * unitSpacing;
            return new Vector3(x, 0f, 0f);
        }

        private void UpdateLayout()
        {
            for (var i = 0; i < _units.Count; i++)
            {
                var pos = GetSlotPosition(i);
                var rt = _units[i].transform as RectTransform;
                if (rt != null)
                    rt.localPosition = pos;
            }
        }
    }
}
