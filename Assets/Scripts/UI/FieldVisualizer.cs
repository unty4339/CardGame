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
        private float unitSpacing = 130f;
        private int maxSlots = 7;
        private float layoutLerpSpeed = 8f;

        private readonly List<UnitView> _units = new();
        private readonly List<Vector3> _targetLocalPositions = new();

        /// <summary>
        /// ドロップ判定用のフィールドエリア
        /// </summary>
        public RectTransform FieldAreaRect => fieldAreaRect;

        /// <summary>
        /// 次にユニットが出るべき場所（ローカル座標）を返す。枠が1つ増えた状態でのスロット位置。
        /// </summary>
        public Vector3 GetNextSpawnPosition()
        {
            var index = _units.Count;
            return GetSlotPosition(index, _units.Count + 1);
        }

        /// <summary>
        /// これから1枠増える前提で既存ユニットの目標位置だけ更新する。既存ユニットが時間をかけて移動し始める。
        /// </summary>
        public void NotifyNewSlotWillBeAdded()
        {
            var count = _units.Count;
            if (count == 0) return;
            _targetLocalPositions.Clear();
            for (var i = 0; i < count; i++)
                _targetLocalPositions.Add(GetSlotPosition(i, count + 1));
        }

        /// <summary>
        /// UnitViewを受け取り、フィールドに追加し、並べ直す。目標位置へは時間をかけて補間される。
        /// </summary>
        public void AddUnit(UnitView unitView)
        {
            if (unitView == null) return;
            _units.Add(unitView);
            unitView.transform.SetParent(transform, false);
            var rt = unitView.transform as RectTransform;
            if (rt != null)
                rt.localPosition = GetSlotPosition(_units.Count - 1, _units.Count);
            UpdateLayout();
        }

        /// <summary>
        /// フィールド上の全 UnitView を返す（ターゲット選択時のハイライト用）
        /// </summary>
        public IReadOnlyList<UnitView> GetUnitViews()
        {
            return _units;
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
            var count = _units.Count;
            while (_targetLocalPositions.Count < count)
                _targetLocalPositions.Add(Vector3.zero);
            if (_targetLocalPositions.Count > count)
                _targetLocalPositions.RemoveRange(count, _targetLocalPositions.Count - count);
            for (var i = 0; i < count; i++)
                _targetLocalPositions[i] = GetSlotPosition(i, count);
        }

        private void Update()
        {
            for (var i = 0; i < _units.Count; i++)
            {
                if (i >= _targetLocalPositions.Count) break;
                var view = _units[i];
                if (view == null) continue;
                var rt = view.transform as RectTransform;
                if (rt == null) continue;
                rt.localPosition = Vector3.Lerp(rt.localPosition, _targetLocalPositions[i], Time.deltaTime * layoutLerpSpeed);
            }
        }
    }
}
