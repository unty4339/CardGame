using System.Collections.Generic;
using CardBattle.Core.Field;
using UnityEngine;

namespace CardBattle.UI
{
    /// <summary>
    /// フィールド上のユニット配置の管理について責任を持つ
    /// </summary>
    public class FieldVisualizer : MonoBehaviour
    {
        [SerializeField] private RectTransform fieldAreaRect;
        [SerializeField] private float unitSpacing = 90f;
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
            return GetSlotPosition(index, _units.Count);
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

        /// <summary>
        /// Unitに対応するUnitViewを返す。無ければ null。
        /// </summary>
        public UnitView GetViewByUnit(Unit unit)
        {
            if (unit == null) return null;
            foreach (var view in _units)
            {
                if (view != null && view.Unit == unit)
                    return view;
            }
            return null;
        }

        /// <summary>
        /// 指定スクリーン座標に重なっている UnitView を返す。無ければ null。
        /// </summary>
        public UnitView GetUnitViewAtScreenPoint(Vector2 screenPoint, Camera eventCamera)
        {
            for (var i = 0; i < _units.Count; i++)
            {
                var view = _units[i];
                if (view == null) continue;
                var rt = view.GetComponent<RectTransform>();
                if (rt != null && RectTransformUtility.RectangleContainsScreenPoint(rt, screenPoint, eventCamera))
                    return view;
            }
            return null;
        }

        /// <summary>
        /// UnitViewをフィールドから除去し、並べ直す。GameObject の Destroy は呼び出し側で行う。
        /// </summary>
        public void RemoveUnit(UnitView unitView)
        {
            if (unitView == null) return;
            _units.Remove(unitView);
            UpdateLayout();
        }

        private Vector3 GetSlotPosition(int index, int totalCount)
        {
            var totalWidth = (totalCount - 1) * unitSpacing;
            var startX = -totalWidth * 0.5f;
            var x = startX + index * unitSpacing;
            return new Vector3(x, 0f, 0f);
        }

        private void UpdateLayout()
        {
            for (var i = 0; i < _units.Count; i++)
            {
                var pos = GetSlotPosition(i, _units.Count);
                var rt = _units[i].transform as RectTransform;
                if (rt != null)
                    rt.localPosition = pos;
            }
        }
    }
}
