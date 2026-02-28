using System.Collections.Generic;
using System.Linq;
using CardBattle.Core;
using CardBattle.Core.Deck;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.Core.Player;
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

        private Color player0FrameColor = new Color(1f, 0.775f, 0.575f, 0.95f);

        private Color player1FrameColor = new Color(0.825f, 1f, 0.675f, 0.95f);

        private Color partnerFrameColor = new Color(0.6f, 0.75f, 1f, 0.95f);

        /// <summary>ペアリングの効果対象にパートナーが選ばれた回数（台詞の出し分け用）。</summary>
        private int _partnerPairingTargetCount;

        /// <summary>ペアリングの効果対象にパートナーが選ばれた回数（立ち絵切り替え等で参照）。</summary>
        public int PartnerPairingTargetCount => _partnerPairingTargetCount;

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
                pm.OnUnitDestroyedWithReason += HandleUnitDestroyedWithReason;
            var partnerManager = PartnerManager.Instance;
            if (partnerManager != null)
                partnerManager.OnPartnerSummoned += OnPartnerPlayed;
        }

        private void OnDestroy()
        {
            var pm = PlayerManager.Instance;
            if (pm != null)
                pm.OnUnitDestroyedWithReason -= HandleUnitDestroyedWithReason;
            var partnerManager = PartnerManager.Instance;
            if (partnerManager != null)
                partnerManager.OnPartnerSummoned -= OnPartnerPlayed;
            if (_instance == this)
                _instance = null;
        }

        private void HandleUnitDestroyedWithReason(Unit unit, UnitDestroyReason reason)
        {
            if (unit == null) return;
            var data = GetDialogueForUnitDestroyed(unit, reason);
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
        /// ユニットが破壊されたことをトリガーに台詞を表示する（原因付き）。
        /// </summary>
        public void OnUnitDestroyed(Unit unit, UnitDestroyReason reason)
        {
            if (unit == null) return;
            var data = GetDialogueForUnitDestroyed(unit, reason);
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

        /// <summary>
        /// パートナーカードがプレイされたときに呼ばれる。プレイヤー0のときのみ青枠で台詞を表示し、立ち絵を銃構え/銃構え照れに切り替える。
        /// </summary>
        public void OnPartnerPlayed(int playerId, Unit unit, bool usedManaForEffect)
        {
            if (playerId != 0) return;
            var text = GetPartnerPlayLine(_partnerPairingTargetCount, usedManaForEffect);
            AddBlockWithTurnActions(new DialogueBlockData(text, frameColorOverride: partnerFrameColor));
            if (PartnerPairingTargetCount == 0)
                StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.GunStance);
            else
                StandingPictureManager.Instance?.SetStandingPicture(StandingPictureType.GunStanceEmbarrassed);
        }

        /// <summary>
        /// ペアリングの効果対象にパートナーカードが選ばれたときに呼ばれる。青枠で台詞を表示し、回数をインクリメントする。
        /// </summary>
        public void OnPartnerChosenAsPairingTarget()
        {
            var text = GetPartnerChosenAsPairingTargetLine(_partnerPairingTargetCount);
            AddBlockWithTurnActions(new DialogueBlockData(text, frameColorOverride: partnerFrameColor));
            _partnerPairingTargetCount++;
        }

        /// <summary>
        /// ペアの相手ユニットが破壊され、パートナー（プレイヤー0）がペアリング対象から外れたときに呼ばれる。青枠で台詞を表示する。
        /// </summary>
        /// <param name="destroyedUnitCardName">破壊されたユニットのカード名（ペアの相手）</param>
        /// <param name="reason">破壊理由（呼び出し元で渡す用。台詞出し分けに使用）</param>
        public void OnPartnerRemovedFromPairing(string destroyedUnitCardName, UnitDestroyReason reason)
        {
            var text = GetPartnerRemovedFromPairingLine(_partnerPairingTargetCount, destroyedUnitCardName);
            if (string.IsNullOrEmpty(text)) return;
            AddBlockWithTurnActions(new DialogueBlockData(text, frameColorOverride: partnerFrameColor));
        }

        private void AddBlockWithTurnActions(DialogueBlockData data)
        {
            if (dialogueLog == null) return;
            IReadOnlyList<TurnActionRecord> turnActions = TurnActionLog.Instance?.GetCurrentTurnActions();
            if (turnActions == null || turnActions.Count == 0)
                turnActions = TurnActionLog.Instance?.GetLastTurnActions();
            dialogueLog.AddBlock(data, turnActions);
        }

        private Color GetFrameColorForPlayer(int playerId)
        {
            return playerId == 1 ? player1FrameColor : player0FrameColor;
        }

        /// <summary>
        /// カードプレイ時に表示する台詞をカード名・ターン行動・盤面で場合分けして返す。該当なしなら null。
        /// このターンの行動: TurnActionLog.Instance.GetCurrentTurnActions()（空なら GetLastTurnActions()）。
        /// 盤面: turnActions[i].BoardStateAtAction でその行動直後の盤面（Player0HP, Player1HP, Player0MP, Player1MP, Player0HandCount, Player1HandCount, Player0Units, Player1Units）にアクセス可能。
        /// </summary>
        private DialogueBlockData? GetDialogueForCardPlayed(Card card)
        {
            var cardName = card?.Template?.CardName;
            if (string.IsNullOrEmpty(cardName)) return null;

            var turnPlayerId = GameFlowManager.Instance != null ? GameFlowManager.Instance.CurrentTurnPlayerId : 0;
            var frameColor = GetFrameColorForPlayer(turnPlayerId);

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
                    return new DialogueBlockData($"{cardName}を召喚！", frameColorOverride: frameColor);
                if (unitPlayCount >= 2)
                    return new DialogueBlockData($"さらに{cardName}を召喚！", frameColorOverride: frameColor);
                return null;
            }

            if (card.Template?.CardType == CardType.Spell)
                return new DialogueBlockData($"{cardName}を発動！", frameColorOverride: frameColor);

            return null;
        }

        /// <summary>
        /// ユニット破壊時に表示する台詞を場合分けして返す。該当なしなら null。
        /// </summary>
        private DialogueBlockData? GetDialogueForUnitDestroyed(Unit unit, UnitDestroyReason reason)
        {
            if (unit == null) return null;
            var gfm = GameFlowManager.Instance;
            var turnPlayerId = gfm != null ? gfm.CurrentTurnPlayerId : 0;

            // パートナー（プレイヤー0）が破壊されたときは原因・回数別の台詞
            if (unit.IsPartner && unit.OwnerPlayerId == 0)
            {
                var text = GetPartnerDestroyedLine(_partnerPairingTargetCount, reason);
                return new DialogueBlockData(text, frameColorOverride: partnerFrameColor);
            }

            // ターンプレイヤーのユニットが破壊されたときは一般台詞を出さない（パートナーは上で処理済み）
            if (unit.OwnerPlayerId == turnPlayerId)
                return null;

            // 相手ユニットが破壊された＝ターンプレイヤーが撃破。攻撃者がパートナーならパートナー台詞
            var partnerKilledLine = GetPartnerKilledEnemyLineIfApplicable(unit);
            if (partnerKilledLine != null)
                return new DialogueBlockData(partnerKilledLine, frameColorOverride: partnerFrameColor);

            var unitName = unit.SourceCardTemplate?.CardName ?? unit.DisplayName ?? "ユニット";
            var destroyerIsSelf = (turnPlayerId == 0);
            var line = destroyerIsSelf ? $"{unitName}を撃破！" : $"私が{unitName}を撃破！";
            return new DialogueBlockData(line, frameColorOverride: GetFrameColorForPlayer(turnPlayerId));
        }

        private string GetPartnerPlayLine(int pairingCount, bool usedManaForEffect)
        {
            if (usedManaForEffect)
            {
                return pairingCount >= 2 ? "フォトンシュート！♥"
                    : pairingCount == 1 ? "フォトンシュート！"
                    : "食らえっ フォトンシュート！";
            }
            return pairingCount >= 2 ? "うう…♥　ま、任せて！♥"
                : pairingCount == 1 ? "ぼ、ボクの出番だね！"
                : "ボクの出番だね！";
        }

        private static string GetPartnerChosenAsPairingTargetLine(int pairingCount)
        {
            return pairingCount >= 2 ? "うう～…♥"
                : pairingCount == 1 ? "まっ、また…！？♥"
                : "やっ、やっぱりボクなの！？";
        }

        /// <summary>
        /// ペアリング対象から外れたときに表示する台詞を、対象回数と破壊されたユニットのカード名で場合分けして返す。表示しない場合は null。
        /// </summary>
        private static string GetPartnerRemovedFromPairingLine(int pairingCount, string destroyedUnitCardName)
        {
            if (pairingCount == 0) return null;
            if (pairingCount >= 2) return "はあ、はあっ…♥";
            // pairingCount == 1
            if (destroyedUnitCardName == "グリンスキンの苗床") return "お、お…っ♥♥";
            if (destroyedUnitCardName == "ゴブリンの騎兵" || destroyedUnitCardName == "肉鎧のオーク") return "うぎゅっ！？♥";
            return "ううっ…♥";
        }

        private string GetPartnerDestroyedLine(int pairingCount, UnitDestroyReason reason)
        {
            switch (reason)
            {
                case UnitDestroyReason.Battle:
                    return pairingCount >= 2 ? "くうっ…！♥"
                        : pairingCount == 1 ? "くうっ…！"
                        : "くっ…！";
                case UnitDestroyReason.Nursery:
                    return pairingCount >= 2 ? "くうっ…！♥"
                        : pairingCount == 1 ? "お、お…っ♥♥"
                        : "くっ…！";
                case UnitDestroyReason.Substitution:
                    return "うぎゅっ！？♥";
                default:
                    return pairingCount >= 2 ? "くうっ…！♥"
                        : pairingCount == 1 ? "くうっ…！"
                        : "くっ…！";
            }
        }

        private string GetPartnerKilledEnemyLineIfApplicable(Unit destroyedUnit)
        {
            var turnActions = TurnActionLog.Instance?.GetCurrentTurnActions();
            if (turnActions == null || turnActions.Count == 0)
                turnActions = TurnActionLog.Instance?.GetLastTurnActions();
            if (turnActions == null || turnActions.Count == 0) return null;
            var last = turnActions[turnActions.Count - 1];
            if (last.ActionType != ActionType.Attack || last.TargetUnitInstanceId != destroyedUnit.InstanceId)
                return null;
            var attackerId = last.AttackerInstanceId;
            if (attackerId == null) return null;
            var pm = PlayerManager.Instance;
            if (pm == null) return null;
            var turnPlayerId = GameFlowManager.Instance != null ? GameFlowManager.Instance.CurrentTurnPlayerId : 0;
            var data = pm.GetPlayerData(turnPlayerId);
            var attacker = data?.FieldZone?.Units?.FirstOrDefault(u => u.InstanceId == attackerId.Value);
            if (attacker == null || !attacker.IsPartner || attacker.OwnerPlayerId != 0)
                return null;
            var c = _partnerPairingTargetCount;
            return c >= 2 ? "よ、よしっ！♥" : c == 1 ? "よしっ倒した…！" : "よしっ！ 敵を倒したよ！";
        }

        /// <summary>
        /// カスタムトリガーIDに応じた台詞を場合分けして返す。該当なしなら null。
        /// </summary>
        private DialogueBlockData? GetDialogueForCustomTrigger(string triggerId)
        {
            if (string.IsNullOrEmpty(triggerId)) return null;
            return triggerId switch
            {
                _ => null
            };
        }

        /// <summary>
        /// ターン開始時に表示する台詞を返す。自分は「俺」、相手は「私」。
        /// </summary>
        private DialogueBlockData? GetDialogueForTurnStarted(int turnPlayerId)
        {
            var text = turnPlayerId == 1 ? "私のターン、ドロー" : "俺のターン、ドロー";
            return new DialogueBlockData(text, frameColorOverride: GetFrameColorForPlayer(turnPlayerId));
        }

        /// <summary>
        /// ターン終了時に表示する台詞を返す。
        /// </summary>
        private DialogueBlockData? GetDialogueForTurnEnded(int turnPlayerId)
        {
            var text = turnPlayerId == 1 ? "私のターンエンド！" : "ターンエンド！";
            return new DialogueBlockData(text, frameColorOverride: GetFrameColorForPlayer(turnPlayerId));
        }
    }
}
