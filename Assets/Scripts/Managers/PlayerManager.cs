using System;
using System.Collections.Generic;
using System.Linq;
using CardBattle.AI;
using CardBattle.Core;
using CardBattle.Core.Deck;
using CardBattle.Core.Effects;
using CardBattle.Core.Enums;
using CardBattle.Core.Field;
using CardBattle.Core.Player;
using CardBattle.ScriptableObjects;
using CardBattle.UI;
using UnityEngine;

namespace CardBattle.Managers
{
    /// <summary>
    /// プレイヤー情報の管理とゾーン操作について責任を持つ
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        private static PlayerManager _instance;
        public static PlayerManager Instance => _instance;

        /// <summary>
        /// カードを引いたときに発火する。プレイヤーIDと引いたCardが渡される。
        /// </summary>
        public event Action<int, Card> OnCardDrawn;

        /// <summary>
        /// ユニット召喚に成功したときに発火する。プレイヤーID、プレイしたCard、生成されたUnitが渡される。
        /// </summary>
        public event Action<int, Card, Unit> OnUnitSummoned;

        /// <summary>
        /// プレイヤーのHP/MPなどが変更されたときに発火する。変更されたプレイヤーIDが渡される。
        /// </summary>
        public event Action<int> OnPlayerDataChanged;

        /// <summary>
        /// ユニットのHPが変更されたときに発火する。UnitView の表示更新用。
        /// </summary>
        public event Action<Unit> OnUnitHpChanged;

        /// <summary>
        /// ユニットが破壊されたときに発火する。UnitView の削除用。
        /// </summary>
        public event Action<Unit> OnUnitDestroyed;

        /// <summary>
        /// ユニットの攻撃力（表示・実効）が変わったときに発火する。UnitView の表示更新用。
        /// </summary>
        public event Action<Unit> OnUnitAttackChanged;

        /// <summary>
        /// 呪文を手札から使用したときに発火する。プレイヤーIDと使用したCardが渡される。手札UIの更新用。
        /// </summary>
        public event Action<int, Card> OnSpellPlayed;

        /// <summary>
        /// 呪文プレイを通知する（ActionQueueManager の Spell 処理から呼ぶ）
        /// </summary>
        public void NotifySpellPlayed(int playerId, Card card)
        {
            OnSpellPlayed?.Invoke(playerId, card);
        }

        /// <summary>
        /// ユニット召喚を通知する（トーテムプレイ時など、Unit がフィールドに登場したときに呼ぶ）
        /// </summary>
        public void NotifyUnitSummoned(int playerId, Card card, Unit unit)
        {
            OnUnitSummoned?.Invoke(playerId, card, unit);
        }

        private readonly Dictionary<int, PlayerData> _players = new();

        /// <summary>
        /// プレイヤーデータ変更を通知する（HP/MPを変更した外部から呼ぶ）
        /// </summary>
        public void NotifyPlayerDataChanged(int playerId)
        {
            OnPlayerDataChanged?.Invoke(playerId);
        }

        /// <summary>
        /// ユニットのHP変更を通知する（攻撃解決などで呼ぶ）
        /// </summary>
        public void NotifyUnitHpChanged(Unit unit)
        {
            OnUnitHpChanged?.Invoke(unit);
        }

        /// <summary>
        /// ユニット破壊を通知する（攻撃解決などで呼ぶ）。ペアリング解除は UnpairIfNeededAndNotifyDestroyed で行うこと。
        /// </summary>
        public void NotifyUnitDestroyed(Unit unit)
        {
            OnUnitDestroyed?.Invoke(unit);
            if (unit != null && unit.IsPartner)
                PartnerManager.Instance?.ReturnPartnerToZone(unit, unit.OwnerPlayerId);
        }

        /// <summary>
        /// ユニットの攻撃力（表示）変更を通知する（ペアリング適用後などで呼ぶ）
        /// </summary>
        public void NotifyUnitAttackChanged(Unit unit)
        {
            OnUnitAttackChanged?.Invoke(unit);
        }

        /// <summary>
        /// ユニットのHPを増減させ、表示更新を通知する。
        /// </summary>
        public void AddUnitHp(Unit unit, int delta)
        {
            if (unit == null) return;
            unit.HP += delta;
            NotifyUnitHpChanged(unit);
        }

        /// <summary>
        /// ユニットのHPを指定値に設定し、表示更新を通知する。
        /// </summary>
        public void SetUnitHp(Unit unit, int value)
        {
            if (unit == null) return;
            unit.HP = value;
            NotifyUnitHpChanged(unit);
        }

        /// <summary>
        /// ユニットの攻撃力を増減させ、表示更新を通知する。
        /// </summary>
        public void AddUnitAttack(Unit unit, int delta)
        {
            if (unit == null) return;
            unit.Attack += delta;
            NotifyUnitAttackChanged(unit);
        }

        /// <summary>
        /// ユニットの攻撃力を指定値に設定し、表示更新を通知する。
        /// </summary>
        public void SetUnitAttack(Unit unit, int value)
        {
            if (unit == null) return;
            unit.Attack = value;
            NotifyUnitAttackChanged(unit);
        }

        /// <summary>
        /// ユニットの実効攻撃力を返す。IAddsPairAttackToSelf の場合は自身＋ペアの攻撃力。ICopiesAttackFromPairTarget の場合はペア対象の攻撃力（パートナーカードの場合は BaseAttack）。
        /// </summary>
        public int GetEffectiveAttack(Unit unit)
        {
            if (unit == null) return 0;
            if (unit.SourceCardTemplate is IAddsPairAttackToSelf)
            {
                if (unit.IsPairedWithUnit)
                    return unit.Attack + unit.GetPairTargetUnitOrNull().Attack;
                if (unit.PairingWithPartnerCard)
                {
                    var data = GetPlayerData(unit.OwnerPlayerId);
                    var partnerAttack = data?.PartnerZone?.Partner?.BaseAttack ?? 0;
                    return unit.Attack + partnerAttack;
                }
                return unit.Attack;
            }
            if (unit.SourceCardTemplate is ICopiesAttackFromPairTarget)
            {
                if (unit.IsPairedWithUnit)
                    return unit.GetPairTargetUnitOrNull().Attack;
                if (unit.PairingWithPartnerCard)
                {
                    var data = GetPlayerData(unit.OwnerPlayerId);
                    var partnerAttack = data?.PartnerZone?.Partner?.BaseAttack;
                    return partnerAttack ?? unit.Attack;
                }
                return unit.Attack;
            }
            return unit.Attack;
        }

        /// <summary>
        /// ペアリングで加算した攻撃・体力ボーナスを還元し、保存値をクリアする。
        /// </summary>
        public void ApplyAndClearPairingBonus(Unit unit) => PairingService.ApplyAndClearPairingBonus(unit);

        /// <summary>
        /// パートナーカードとのペアリングのみ解除する（破壊通知は行わない）。身代わり効果でパートナーカードをペア対象にしていた場合に使用する。
        /// </summary>
        public void UnpairPartnerCardOnly(Unit unit) => PairingService.UnpairPartnerCardOnly(unit);

        /// <summary>
        /// ユニットが場を離れる前にペアリング解除（OnUnpair 発動・参照クリア）を行い、続けて破壊通知する。
        /// 呼び出し元はこのメソッドの後に Units.Remove(unit) を行うこと。
        /// </summary>
        public void UnpairIfNeededAndNotifyDestroyed(Unit unit) => PairingService.UnpairAndNotifyDestroyed(unit);

        /// <summary>
        /// 指定プレイヤー視点の GameState を組み立てる。ペアリング解除時の OnUnpair などで使用する。
        /// </summary>
        public GameState GetGameStateForPlayer(int myPlayerId)
        {
            var myData = GetPlayerData(myPlayerId);
            var oppId = myPlayerId == 0 ? 1 : 0;
            var oppData = GetPlayerData(oppId);
            if (myData == null || oppData == null)
                return new GameState();

            return new GameState
            {
                MyPlayerId = myPlayerId,
                OpponentPlayerId = oppId,
                MyHand = new List<Card>(myData.Hand.Cards),
                MyField = myData.FieldZone,
                OpponentField = oppData.FieldZone,
                MyHP = myData.HP,
                OpponentHP = oppData.HP,
                MyMP = myData.CurrentMP,
                OpponentMP = oppData.CurrentMP
            };
        }

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

        public PlayerData GetPlayerData(int playerId)
        {
            return _players.TryGetValue(playerId, out var data) ? data : null;
        }

        /// <summary>
        /// 指定プレイヤーのフィールドから、指定した InstanceId の Unit を返す。見つからなければ null。
        /// AI のクローン Unit を本物の Unit に解決するときに使う。
        /// </summary>
        public Unit GetUnitByInstanceId(int playerId, int instanceId)
        {
            var data = GetPlayerData(playerId);
            if (data?.FieldZone?.Units == null) return null;
            return data.FieldZone.Units.Find(u => u.InstanceId == instanceId);
        }

        /// <summary>
        /// プレイヤーデータを登録する
        /// </summary>
        public void RegisterPlayer(int playerId, PlayerData playerData)
        {
            _players[playerId] = playerData;
        }

        /// <summary>
        /// 指定テンプレートのカードを1枚生成して手札に加える。効果で「〇〇を手札に加える」を行うときに使用する。
        /// </summary>
        public bool AddCardToHand(int playerId, CardTemplate template)
        {
            var data = GetPlayerData(playerId);
            if (data == null || template == null) return false;
            var card = DeckBuilder.CreateCardFromTemplate(template);
            data.Hand.Cards.Add(card);
            OnCardDrawn?.Invoke(playerId, card);
            NotifyPlayerDataChanged(playerId);
            return true;
        }

        /// <summary>
        /// プレイヤーIDを受け取り、デッキの一番上からカードを手札に加える
        /// </summary>
        public bool DrawCard(int playerId)
        {
            var data = GetPlayerData(playerId);
            if (data == null) return false;

            if (data.Deck.Cards.Count == 0)
            {
                // TODO: 敗北処理へ遷移
                return false;
            }

            var card = data.Deck.Cards[0];
            data.Deck.Cards.RemoveAt(0);
            data.Hand.Cards.Add(card);
            OnCardDrawn?.Invoke(playerId, card);
            return true;
        }

        /// <summary>
        /// プレイヤーIDとカードを受け取り、そのカードをプレイしてユニットを召喚する。成功時は true、不成立時は false を返す。
        /// 召喚時効果の解決（対象選択含む）が完了したときに onPlayComplete を呼ぶ。非同期の場合は効果解決後に呼ばれる。
        /// preferredInstanceId が指定された場合、召喚ユニットにその InstanceId を付与する（AI シミュレーションとの統一用）。
        /// </summary>
        public bool TryPlayCard(int playerId, Card card, Action onPlayComplete = null, int? preferredInstanceId = null)
        {
            var data = GetPlayerData(playerId);
            if (data == null || card == null || card.Template == null) return false;
            if (card.Template.CardType != CardType.Unit) return false;
            if (!data.Hand.Cards.Contains(card)) return false;
            if (data.CurrentMP < card.Template.PlayCost) return false;

            data.Hand.Cards.Remove(card);
            data.CurrentMP -= card.Template.PlayCost;
            NotifyPlayerDataChanged(playerId);

            void OnEffectsResolved(Unit unit)
            {
                if (unit != null)
                {
                    // このターンに召喚したユニットに攻撃権を付与（速攻・神速のみ即攻撃可能）
                    var gfm = GameFlowManager.Instance;
                    if (gfm != null && playerId == gfm.CurrentTurnPlayerId)
                    {
                        var ownerData = GetPlayerData(playerId);
                        if (ownerData != null && ownerData.FieldZone.Units.Contains(unit) && unit.Keywords != null)
                        {
                            if (unit.Keywords.Contains(KeywordAbility.Rush) || unit.Keywords.Contains(KeywordAbility.DivineSpeed))
                                unit.CanAttackUnit = true;
                            if (unit.Keywords.Contains(KeywordAbility.DivineSpeed))
                                unit.CanAttackPlayer = true;
                        }
                    }
                    OnUnitSummoned?.Invoke(playerId, card, unit);
                }
                onPlayComplete?.Invoke();
            }

            var unit = UnitManager.Instance?.SpawnUnitFromCard(card, playerId, data.FieldZone, OnEffectsResolved, preferredInstanceId);
            return true;
        }

        /// <summary>
        /// プレイヤーIDとカードを受け取り、そのカードの行動一覧を返す
        /// </summary>
        public List<GameAction> GetCardActions(int playerId, Card card)
        {
            var data = GetPlayerData(playerId);
            if (data == null || card == null) return new List<GameAction>();

            return new List<GameAction>(card.AvailableActions);
        }

        /// <summary>
        /// プレイヤーIDとユニットを受け取り、そのユニットの行動一覧を返す
        /// </summary>
        public List<GameAction> GetUnitActions(int playerId, Unit unit)
        {
            var data = GetPlayerData(playerId);
            if (data == null || unit == null) return new List<GameAction>();

            var actions = new List<GameAction>();
            var opponentId = playerId == 0 ? 1 : 0;
            var opponentData = GetPlayerData(opponentId);
            if (opponentData != null)
            {
                var battleManager = Battle.BattleManager.Instance;
                if (unit.CanAttackUnit)
                {
                    foreach (var target in opponentData.FieldZone.Units)
                    {
                        if (battleManager != null && battleManager.CanAttackUnit(unit, target, opponentData.FieldZone))
                        {
                            actions.Add(new GameAction
                            {
                                ActionType = ActionType.Attack,
                                SourceUnit = unit,
                                Target = target
                            });
                        }
                    }
                }
                if (unit.CanAttackPlayer && battleManager != null && battleManager.CanAttackPlayer(unit, opponentData.FieldZone))
                {
                    actions.Add(new GameAction
                    {
                        ActionType = ActionType.Attack,
                        SourceUnit = unit,
                        Target = opponentId
                    });
                }
            }
            return actions;
        }

        /// <summary>
        /// プレイヤーIDとカードを受け取り、効果で手札に新たなカードを加える
        /// </summary>
        public void AddCardToHand(int playerId, Card card)
        {
            var data = GetPlayerData(playerId);
            if (data != null && card != null)
            {
                data.Hand.Cards.Add(card);
            }
        }

        /// <summary>
        /// 最大MPを+1する
        /// </summary>
        public void IncreaseMaxMP(int playerId)
        {
            var data = GetPlayerData(playerId);
            if (data != null)
            {
                data.MaxMP++;
                NotifyPlayerDataChanged(playerId);
            }
        }

        /// <summary>
        /// 現在MPを最大MPで全回復する
        /// </summary>
        public void RestoreMP(int playerId)
        {
            var data = GetPlayerData(playerId);
            if (data != null)
            {
                data.CurrentMP = data.MaxMP;
                NotifyPlayerDataChanged(playerId);
            }
        }

        /// <summary>
        /// フィールドの全ユニットに攻撃権を付与する（トーテムは除く）。ペア対象に「攻撃できない」を付与する効果とペア中のユニットには付与しない。
        /// </summary>
        public void GrantAttackToAllUnits(int playerId)
        {
            var data = GetPlayerData(playerId);
            if (data == null) return;

            foreach (var unit in data.FieldZone.GetAttackableUnits())
            {
                // ゴブリンの騎兵・肉鎧のオーク・グリンスキンの苗床などとペア中のユニットは攻撃権を付与しない
                if (unit.GetPairTargetUnitOrNull()?.SourceCardTemplate is IGrantsCannotAttackToPairTarget)
                    continue;
                unit.CanAttackUnit = true;
                unit.CanAttackPlayer = true;
            }
        }

        /// <summary>
        /// 経過ターン数を加算する
        /// </summary>
        public void IncrementTurnsOnField(int playerId)
        {
            var data = GetPlayerData(playerId);
            if (data == null) return;

            foreach (var unit in data.FieldZone.Units)
            {
                unit.TurnsOnField++;
            }
        }

        /// <summary>
        /// ターン開始時効果を実行する。フィールドの各ユニット（トーテム含む）の SourceCardTemplate が ITurnStartEffect を持つ場合に発動する。
        /// </summary>
        public void RunTurnStartEffects(int turnPlayerId)
        {
            var data = GetPlayerData(turnPlayerId);
            if (data == null) return;

            var unitsToProcess = data.FieldZone.Units
                .Where(u => u.SourceCardTemplate?.GetTurnStartEffect() != null)
                .ToList();

            foreach (var unit in unitsToProcess)
            {
                var effect = unit.SourceCardTemplate.GetTurnStartEffect();
                if (effect != null)
                    effect.Resolve(unit, turnPlayerId);
            }
        }

        /// <summary>
        /// マリガン：手札のカードをデッキに戻しシャッフルして引き直す
        /// </summary>
        public void Mulligan(int playerId, List<Card> cardsToReturn)
        {
            var data = GetPlayerData(playerId);
            if (data == null || cardsToReturn == null) return;

            foreach (var card in cardsToReturn)
            {
                data.Hand.Cards.Remove(card);
                data.Deck.Cards.Add(card);
            }

            DeckBuilder.Shuffle(data.Deck.Cards);

            foreach (var _ in cardsToReturn)
            {
                DrawCard(playerId);
            }
        }
    }
}
