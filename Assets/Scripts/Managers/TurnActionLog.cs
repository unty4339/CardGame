using System.Collections.Generic;
using CardBattle.Core;
using CardBattle.Core.Deck;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.Core.Player;
using UnityEngine;

namespace CardBattle.Managers
{
    /// <summary>
    /// ターン開始から各ターンプレイヤーの行動をリストで記録し、各行動時の盤面スナップショットを保持する。
    /// </summary>
    public class TurnActionLog : MonoBehaviour
    {
        private static TurnActionLog _instance;
        public static TurnActionLog Instance => _instance;

        private readonly List<TurnActionRecord> _currentTurnActions = new();
        private readonly List<TurnActionRecord> _lastTurnActions = new();

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
        /// ターン開始時に呼ぶ。現在ターン用のリストをクリアする。
        /// </summary>
        public void ClearForNewTurn(int turnPlayerId)
        {
            _currentTurnActions.Clear();
        }

        /// <summary>
        /// 行動が実行された直後に呼ぶ。現在の盤面をスナップショットして記録に追加する。
        /// </summary>
        public void RecordAction(GameAction action, int turnPlayerId)
        {
            if (action == null) return;
            if (action.ActionType != ActionType.Play && action.ActionType != ActionType.Attack)
                return;

            var snapshot = BuildBoardStateSnapshot(turnPlayerId);
            var record = new TurnActionRecord
            {
                ActionType = action.ActionType,
                PlayerId = turnPlayerId,
                BoardStateAtAction = snapshot
            };

            if (action.ActionType == ActionType.Play && action.SourceCard != null)
            {
                var template = action.SourceCard.Template;
                record.CardName = template?.CardName ?? "Card";
                record.IsUnitPlay = template != null && template.CardType == Core.Enums.CardType.Unit;
            }

            if (action.ActionType == ActionType.Attack && action.SourceUnit != null)
            {
                record.AttackerInstanceId = action.SourceUnit.InstanceId;
                if (action.Target is Unit targetUnit)
                {
                    record.TargetUnitInstanceId = targetUnit.InstanceId;
                    record.TargetIsLeader = false;
                }
                else
                {
                    record.TargetUnitInstanceId = null;
                    record.TargetIsLeader = true;
                }
            }

            _currentTurnActions.Add(record);
        }

        /// <summary>
        /// ターン終了時に呼ぶ。現在ターンの行動を直前ターン用に確定してからクリアする。
        /// </summary>
        public void FinishTurn()
        {
            _lastTurnActions.Clear();
            _lastTurnActions.AddRange(_currentTurnActions);
            _currentTurnActions.Clear();
        }

        /// <summary>
        /// 現在ターン中に実行された行動一覧（台詞トリガーが行動直後のときに渡す用）
        /// </summary>
        public IReadOnlyList<TurnActionRecord> GetCurrentTurnActions()
        {
            return _currentTurnActions;
        }

        /// <summary>
        /// 直前ターンに実行された行動一覧（ターン終了後の台詞用）
        /// </summary>
        public IReadOnlyList<TurnActionRecord> GetLastTurnActions()
        {
            return _lastTurnActions;
        }

        private static BoardStateSnapshot BuildBoardStateSnapshot(int turnPlayerId)
        {
            var pm = PlayerManager.Instance;
            var gfm = GameFlowManager.Instance;
            if (pm == null) return new BoardStateSnapshot { TurnPlayerId = turnPlayerId };

            var snapshot = new BoardStateSnapshot { TurnPlayerId = gfm != null ? gfm.CurrentTurnPlayerId : turnPlayerId };

            for (var i = 0; i <= 1; i++)
            {
                var data = pm.GetPlayerData(i);
                if (data == null) continue;

                if (i == 0)
                {
                    snapshot.Player0HP = data.HP;
                    snapshot.Player0MP = data.CurrentMP;
                    snapshot.Player0HandCount = data.Hand?.Cards?.Count ?? 0;
                    snapshot.Player0Units = CopyUnits(data.FieldZone?.Units);
                }
                else
                {
                    snapshot.Player1HP = data.HP;
                    snapshot.Player1MP = data.CurrentMP;
                    snapshot.Player1HandCount = data.Hand?.Cards?.Count ?? 0;
                    snapshot.Player1Units = CopyUnits(data.FieldZone?.Units);
                }
            }

            return snapshot;
        }

        private static List<UnitSnapshot> CopyUnits(IList<Unit> units)
        {
            var list = new List<UnitSnapshot>();
            if (units == null) return list;
            foreach (var u in units)
            {
                if (u == null) continue;
                list.Add(new UnitSnapshot
                {
                    InstanceId = u.InstanceId,
                    HP = u.HP,
                    Attack = u.Attack,
                    OwnerPlayerId = u.OwnerPlayerId
                });
            }
            return list;
        }
    }
}
