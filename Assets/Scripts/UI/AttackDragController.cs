using System.Collections.Generic;
using CardBattle.Core;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.Managers;
using UnityEngine;

namespace CardBattle.UI
{
    /// <summary>
    /// プレイヤー0の攻撃ドラッグ（自分ユニット→相手ユニット/相手プレイヤーゾーン）の開始・終了とドロップ先判定を管理する。
    /// </summary>
    public class AttackDragController : MonoBehaviour
    {
        public static AttackDragController Instance { get; private set; }

        [SerializeField] private FieldVisualizer opponentFieldVisualizer;
        [SerializeField] private RectTransform opponentPlayerZoneRect;

        private UnitView _currentSource;
        private readonly List<GameAction> _attackActions = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        /// <summary>
        /// 攻撃ドラッグを開始可能か試し、可能なら攻撃アクションリストを保持して true を返す。
        /// </summary>
        public bool TryStartAttackDrag(UnitView source)
        {
            if (source == null || source.Unit == null || source.Unit.OwnerPlayerId != 0)
                return false;

            var gameFlow = GameFlowManager.Instance;
            var actionQueue = ActionQueueManager.Instance;
            var playerManager = PlayerManager.Instance;
            if (gameFlow == null || actionQueue == null || playerManager == null)
                return false;
            if (gameFlow.CurrentTurnPlayerId != 0 || actionQueue.IsBusy)
                return false;

            var actions = playerManager.GetUnitActions(0, source.Unit);
            _attackActions.Clear();
            foreach (var a in actions)
            {
                if (a != null && a.ActionType == ActionType.Attack)
                    _attackActions.Add(a);
            }

            if (_attackActions.Count == 0)
                return false;

            _currentSource = source;
            return true;
        }

        /// <summary>
        /// 攻撃ドラッグ終了時。ドロップ位置からターゲットを判定し、該当する攻撃アクションをキューに追加する。
        /// </summary>
        public void OnAttackDragEnded(UnitView source, Vector2 screenPosition, Camera cam)
        {
            if (_currentSource != source || _attackActions.Count == 0)
            {
                ClearDragState();
                return;
            }

            var actionQueue = ActionQueueManager.Instance;
            if (actionQueue == null)
            {
                ClearDragState();
                return;
            }

            GameAction actionToAdd = null;

            // 相手ユニットにドロップした場合
            if (opponentFieldVisualizer != null)
            {
                // TODO:「この FieldVisualizer が属している Canvas が Overlay のときは、RectangleContainsScreenPoint に渡す camera を null にする」
                var targetView = opponentFieldVisualizer.GetUnitViewAtScreenPoint(screenPosition, null);
                if (targetView != null && targetView.Unit != null)
                {
                    foreach (var a in _attackActions)
                    {
                        if (a.Target == targetView.Unit)
                        {
                            actionToAdd = a;
                            break;
                        }
                    }
                }
            }

            // 相手プレイヤーゾーンにドロップした場合
            if (actionToAdd == null && opponentPlayerZoneRect != null)
            {
                var camForRect = cam != null ? cam : Camera.main;
                if (camForRect != null && RectTransformUtility.RectangleContainsScreenPoint(opponentPlayerZoneRect, screenPosition, camForRect))
                {
                    foreach (var a in _attackActions)
                    {
                        if (a.Target is int playerId && playerId == 1)
                        {
                            actionToAdd = a;
                            break;
                        }
                    }
                }
            }

            if (actionToAdd != null)
                actionQueue.AddAction(actionToAdd);

            ClearDragState();
        }

        private void ClearDragState()
        {
            _currentSource = null;
            _attackActions.Clear();
        }
    }
}
