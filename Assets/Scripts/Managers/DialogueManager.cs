using System.Collections.Generic;
using CardBattle.Core;
using CardBattle.Core.Deck;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.Dialogue;
using CardBattle.UI;
using UnityEngine;

namespace CardBattle.Managers
{
    /// <summary>
    /// 台詞のトリガー管理について責任を持つ。トリガーに応じてメソッド内で場合分けし台詞を定義し、台詞ログに追加する。
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        private static DialogueManager _instance;
        public static DialogueManager Instance => _instance;

        [SerializeField] private DialogueLog dialogueLog;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            var pm = PlayerManager.Instance;
            if (pm != null)
                pm.OnUnitDestroyed += HandleUnitDestroyed;
        }

        private void OnDestroy()
        {
            var pm = PlayerManager.Instance;
            if (pm != null)
                pm.OnUnitDestroyed -= HandleUnitDestroyed;
            if (_instance == this)
                _instance = null;
        }

        private void HandleUnitDestroyed(Unit unit)
        {
            if (unit == null) return;
            var data = GetDialogueForUnitDestroyed(unit);
            if (data is { } d)
                AddBlockWithTurnActions(d);
        }

        /// <summary>
        /// カードがプレイされたことをトリガーに台詞を表示する
        /// </summary>
        public void OnCardPlayed(Card card)
        {
            if (card == null) return;
            var data = GetDialogueForCardPlayed(card);
            if (data is { } d)
                AddBlockWithTurnActions(d);
        }

        /// <summary>
        /// ユニットが破壊されたことをトリガーに台詞を表示する
        /// </summary>
        public void OnUnitDestroyed(Unit unit)
        {
            if (unit == null) return;
            var data = GetDialogueForUnitDestroyed(unit);
            if (data is { } d)
                AddBlockWithTurnActions(d);
        }

        /// <summary>
        /// 外部から指定したトリガーIDで台詞を表示する
        /// </summary>
        public void OnCustomTrigger(string triggerId)
        {
            if (string.IsNullOrEmpty(triggerId)) return;
            var data = GetDialogueForCustomTrigger(triggerId);
            if (data is { } d)
                AddBlockWithTurnActions(d);
        }

        /// <summary>
        /// ターン開始時に呼ばれる。台詞を表示する
        /// </summary>
        public void OnTurnStarted(int turnPlayerId)
        {
            var data = GetDialogueForTurnStarted(turnPlayerId);
            if (data is { } d)
                AddBlockWithTurnActions(d);
        }

        /// <summary>
        /// ターン終了時に呼ばれる。台詞を表示する。
        /// </summary>
        public void OnTurnEnded(int turnPlayerId)
        {
            var data = GetDialogueForTurnEnded(turnPlayerId);
            if (data is { } d)
                AddBlockWithTurnActions(d);
        }

        private void AddBlockWithTurnActions(DialogueBlockData data)
        {
            if (dialogueLog == null) return;
            IReadOnlyList<TurnActionRecord> turnActions = TurnActionLog.Instance?.GetCurrentTurnActions();
            if (turnActions == null || turnActions.Count == 0)
                turnActions = TurnActionLog.Instance?.GetLastTurnActions();
            dialogueLog.AddBlock(data, turnActions);
        }

        /// <summary>
        /// カードプレイ時に表示する台詞をカード名・ターン行動・盤面で場合分けして返す。該当なしなら null。
        /// このターンの行動: TurnActionLog.Instance.GetCurrentTurnActions()（空なら GetLastTurnActions()）。
        /// 盤面: turnActions[i].BoardStateAtAction でその行動直後の盤面（Player0HP, Player1HP, Player0MP, Player1MP, Player0HandCount, Player1HandCount, Player0Units, Player1Units）にアクセス可能。
        /// </summary>
        private static DialogueBlockData? GetDialogueForCardPlayed(Card card)
        {
            var cardName = card?.Template?.CardName;
            if (string.IsNullOrEmpty(cardName)) return null;

            IReadOnlyList<TurnActionRecord> turnActions = TurnActionLog.Instance?.GetCurrentTurnActions();
            if (turnActions == null || turnActions.Count == 0)
                turnActions = TurnActionLog.Instance?.GetLastTurnActions();

            if (card.Template?.CardType == CardType.Unit)
            {
                if (turnActions != null && turnActions.Count >= 2)
                {
                    var prev = turnActions[turnActions.Count - 2];
                    if (prev.ActionType == ActionType.Play && prev.CardName == cardName)
                        return null;
                }
                var unitPlayCount = 0;
                if (turnActions != null)
                {
                    foreach (var r in turnActions)
                    {
                        if (r.ActionType == ActionType.Play && r.IsUnitPlay) unitPlayCount++;
                    }
                }
                if (unitPlayCount == 1)
                    return new DialogueBlockData($"{cardName}を召喚！");
                if (unitPlayCount >= 2)
                    return new DialogueBlockData($"さらに{cardName}を召喚！");
                return null;
            }

            return cardName switch
            {
                _ => null
            };
        }

        /// <summary>
        /// ユニット破壊時に表示する台詞を場合分けして返す。該当なしなら null。
        /// </summary>
        private static DialogueBlockData? GetDialogueForUnitDestroyed(Unit unit)
        {
            if (unit == null) return null;
            return new DialogueBlockData("…");
        }

        /// <summary>
        /// カスタムトリガーIDに応じた台詞を場合分けして返す。該当なしなら null。
        /// </summary>
        private static DialogueBlockData? GetDialogueForCustomTrigger(string triggerId)
        {
            if (string.IsNullOrEmpty(triggerId)) return null;
            return triggerId switch
            {
                _ => null
            };
        }

        /// <summary>
        /// ターン開始時に表示する台詞を返す。
        /// </summary>
        private static DialogueBlockData? GetDialogueForTurnStarted(int turnPlayerId)
        {
            return new DialogueBlockData("俺のターン、ドロー");
        }

        /// <summary>
        /// ターン終了時に表示する台詞を返す。
        /// </summary>
        private static DialogueBlockData? GetDialogueForTurnEnded(int turnPlayerId)
        {
            return new DialogueBlockData("ターンエンド！");
        }
    }
}
