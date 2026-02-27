using System;
using System.Collections;
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
            Debug.Log($"[EffectResolver.RequestTargetAsync] entry: choices.Count={choices?.Count ?? -1}, actingPlayerId={actingPlayerId}");

            if (choices == null || choices.Count == 0)
            {
                Debug.Log("[EffectResolver] early return: no choices");
                return Task.FromResult(EffectTarget.None());
            }
            if (actingPlayerId != 0)
            {
                Debug.Log("[EffectResolver] early return: not player 0");
                return Task.FromResult(choices[0]);
            }

            Debug.Log("[EffectResolver] entering UI target selection");
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
                    if (choice.Kind == EffectTargetKind.Unit && choice.UnitInstanceId != null)
                    {
                        var view = gvm.GetUnitViewByInstanceId(actingPlayerId, choice.UnitInstanceId.Value)
                            ?? gvm.GetUnitViewByInstanceId(opponentId, choice.UnitInstanceId.Value);
                        if (view != null)
                        {
                            view.SetSelectableForEffect(true);
                        }
                    }
                    else if (choice.Kind == EffectTargetKind.PartnerCard && choice.PlayerId == actingPlayerId)
                    {
                        gvm.SetPartnerCardSelectableForEffect(true, actingPlayerId);
                    }
                }

                if (myField != null)
                {
                    foreach (var view in myField.GetUnitViews())
                    {
                        if (view == null || view.Unit == null) continue;
                        var isInChoices = false;
                        foreach (var c in choices)
                        {
                            if (c.Kind == EffectTargetKind.Unit && c.UnitInstanceId == view.Unit.InstanceId)
                            {
                                isInChoices = true;
                                break;
                            }
                        }
                        if (!isInChoices)
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
                            view.SetSelectableForEffect(false);
                        }
                    }
                }
            }

            GameVisualManager.Instance?.ShowTargetSelectionPrompt();
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
        /// 対象選択をコルーチンで行い、完了時に onSelected を呼ぶ。人間プレイヤー時は UI で選択（メインスレッドをブロックしない）。
        /// 候補が空・AI の場合は即座に onSelected を呼んで yield break する。
        /// </summary>
        public IEnumerator RunTargetSelectionCoroutine(IList<EffectTarget> choices, int actingPlayerId, Action<EffectTarget> onSelected)
        {
            if (choices == null || choices.Count == 0)
            {
                onSelected(EffectTarget.None());
                yield break;
            }
            if (actingPlayerId != 0)
            {
                onSelected(choices[0]);
                yield break;
            }
            var task = RequestTargetAsync(choices, actingPlayerId);
            while (!task.IsCompleted)
                yield return null;
            onSelected(task.GetAwaiter().GetResult());
        }

        /// <summary>
        /// ユニットをクリックしてターゲットとして確定したときに UnitView から呼ばれる
        /// </summary>
        public void ConfirmTarget(EffectTarget target)
        {
            Debug.Log($"[EffectResolver.ConfirmTarget] called, target.UnitInstanceId={target.UnitInstanceId?.ToString() ?? "null"}");
            ClearTargetSelectionHighlights();
            GameVisualManager.Instance?.HideTargetSelectionPrompt();
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
            GameVisualManager.Instance?.HideTargetSelectionPrompt();
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
                gvm.SetPartnerCardSelectableForEffect(false, p);
            }
        }
    }
}
