using System.Collections.Generic;
using CardBattle.Core.Enums;
using CardBattle.ScriptableObjects;

namespace CardBattle.Core.Field
{
    /// <summary>
    /// フィールドに存在するユニットのデータについて責任を持つ
    /// </summary>
    public class Unit
    {
        private static int _nextId;
        public int InstanceId { get; set; }

        public Unit()
        {
            InstanceId = _nextId++;
        }

        /// <summary>
        /// 指定した InstanceId でユニットを生成する。AI シミュレーションと実ゲームの ID を統一するために使用する。
        /// _nextId を id+1 以上に更新し、以降の new Unit() がこの id を再利用しないようにする。
        /// </summary>
        public static Unit CreateWithInstanceId(int id)
        {
            if (_nextId <= id)
                _nextId = id + 1;
            return new Unit(id);
        }

        private Unit(int instanceId)
        {
            InstanceId = instanceId;
        }

        public int HP { get; set; }
        public int Attack { get; set; }
        public int TurnsOnField { get; set; }
        /// <summary>相手ユニットへ攻撃可能か</summary>
        public bool CanAttackUnit { get; set; }
        /// <summary>相手プレイヤーへ攻撃可能か</summary>
        public bool CanAttackPlayer { get; set; }
        public List<KeywordAbility> Keywords { get; set; } = new();
        public List<Effect> Effects { get; set; } = new();
        public Unit PairingTarget { get; set; }

        /// <summary>
        /// ペア対象が「パートナーカード」（ユニットとしてまだ場に出ていない状態）のとき true。
        /// この間は肩代わりは発動しない。
        /// </summary>
        public bool PairingWithPartnerCard { get; set; }

        /// <summary>いずれかのペア状態（ユニット同士 or パートナーカード）にあるとき true。</summary>
        public bool IsPaired => PairingTarget != null || PairingWithPartnerCard;

        /// <summary>ペア対象がフィールド上のユニットのとき true。</summary>
        public bool IsPairedWithUnit => PairingTarget != null;

        /// <summary>ペア対象ユニット。パートナーカードとペアのときは null。</summary>
        public Unit GetPairTargetUnitOrNull() => PairingTarget;

        /// <summary>
        /// ペアリングで加算した攻撃力。解除時に還元するために保持する。
        /// </summary>
        public int PairingAttackBonus { get; set; }

        /// <summary>
        /// ペアリングで加算した体力。解除時に還元するために保持する。
        /// </summary>
        public int PairingHpBonus { get; set; }

        /// <summary>
        /// パートナーであるかどうか
        /// </summary>
        public bool IsPartner { get; set; }

        /// <summary>
        /// トーテムとしてフィールドに出ているかどうか。トーテムは攻撃対象に選べず、攻撃権も付与されない。
        /// </summary>
        public bool IsTotem { get; set; }

        /// <summary>
        /// オーナープレイヤーID
        /// </summary>
        public int OwnerPlayerId { get; set; }

        /// <summary>
        /// このユニットの元になったカードのテンプレート。説明文表示などに使用する。
        /// </summary>
        public CardTemplate SourceCardTemplate { get; set; }

        /// <summary>
        /// SourceCardTemplate が null のときの表示名（パートナーユニット用）。名前・アートワーク表示に使用する。
        /// </summary>
        public string DisplayName { get; set; }
    }
}
