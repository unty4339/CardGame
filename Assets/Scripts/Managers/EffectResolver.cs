using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CardBattle.Core.Effects;
using CardBattle.UI;
using UnityEngine;

namespace CardBattle.Managers
{
    /// <summary>
    /// 効果のターゲット選択を非同期で解決する。人間プレイヤー時は UI で選択、AI 時は即時返却。
    /// </summary>
    public class EffectResolver : MonoBehaviour
    {
        private static EffectResolver _instance;
        public static EffectResolver Instance => _instance;

        private TaskCompletionSource<EffectTarget> _currentTcs;
        private int _currentActingPlayerId;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        /// <summary>
        /// 候補から1つターゲットを選択する。actingPlayerId が 0 かつ候補が2件以上のときは UI で選択。
        /// </summary>
        public Task<EffectTarget> RequestTargetAsync(IList<EffectTarget> choices, int actingPlayerId, CancellationToken cancellation = default)
        {
            if (choices == null || choices.Count == 0)
                return Task.FromResult(EffectTarget.None());
            if (actingPlayerId != 0)
                return Task.FromResult(choices[0]);
            if (choices.Count == 1)
                return Task.FromResult(choices[0]);

            _currentActingPlayerId = actingPlayerId;
            _currentTcs = new TaskCompletionSource<EffectTarget>();

            var gameFlow = GameFlowManager.Instance;
            gameFlow?.EnterTargetSelection();

            var opponentId = actingPlayerId == 0 ? 1 : 0;
            var gvm = GameVisualManager.Instance;
            if (gvm != null)
            {
                var myField = gvm.GetFieldVisualizer(actingPlayerId);
                var oppField = gvm.GetFieldVisualizer(opponentId);

                foreach (var choice in choices)
                {
                    if (choice.Kind != EffectTargetKind.Unit || choice.UnitInstanceId == null) continue;
                    var view = gvm.GetUnitViewByInstanceId(opponentId, choice.UnitInstanceId.Value);
                    if (view != null)
                    {
                        view.SetHighlight(true);
                        view.SetSelectableForEffect(true);
                    }
                }

                if (myField != null)
                {
                    foreach (var view in myField.GetUnitViews())
                    {
                        if (view == null) continue;
                        view.SetGrayedOut(true);
                        view.SetSelectableForEffect(false);
                    }
                }
                if (oppField != null)
                {
                    foreach (var view in oppField.GetUnitViews())
                    {
                        if (view == null || view.Unit == null) continue;
                        var isHighlighted = false;
                        foreach (var c in choices)
                        {
                            if (c.Kind == EffectTargetKind.Unit && c.UnitInstanceId == view.Unit.InstanceId)
                            {
                                isHighlighted = true;
                                break;
                            }
                        }
                        if (!isHighlighted)
                        {
                            view.SetGrayedOut(true);
                            view.SetSelectableForEffect(false);
                        }
                    }
                }
            }

            return _currentTcs.Task;
        }

        /// <summary>
        /// 従来のシグネチャ（actingPlayerId なし）。呼び出し元が 0 を渡す想定で、UnitManager から渡す場合は ownerPlayerId を渡すこと。
        /// </summary>
        public Task<EffectTarget> RequestTargetAsync(IList<EffectTarget> choices, CancellationToken cancellation = default)
        {
            return RequestTargetAsync(choices, 0, cancellation);
        }

        /// <summary>
        /// ユニットをクリックしてターゲットとして確定したときに UnitView から呼ばれる
        /// </summary>
        public void ConfirmTarget(EffectTarget target)
        {
            ClearTargetSelectionHighlights();
            GameFlowManager.Instance?.ExitTargetSelection();
            _currentTcs?.TrySetResult(target);
            _currentTcs = null;
        }

        /// <summary>
        /// キャンセルボタンなどでターゲット選択をやめるとき
        /// </summary>
        public void CancelTargetSelection()
        {
            ClearTargetSelectionHighlights();
            GameFlowManager.Instance?.ExitTargetSelection();
            _currentTcs?.TrySetResult(EffectTarget.None());
            _currentTcs = null;
        }

        private void ClearTargetSelectionHighlights()
        {
            var gvm = GameVisualManager.Instance;
            if (gvm == null) return;
            for (var p = 0; p <= 1; p++)
            {
                var fv = gvm.GetFieldVisualizer(p);
                if (fv == null) continue;
                foreach (var view in fv.GetUnitViews())
                {
                    if (view == null) continue;
                    view.SetHighlight(false);
                    view.SetGrayedOut(false);
                    view.SetSelectableForEffect(false);
                }
            }
        }
    }
}
