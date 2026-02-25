using System.Collections.Generic;
using CardBattle.Core.Deck;
using CardBattle.Core.Field;

namespace CardBattle.AI
{
    /// <summary>
    /// 現在の自分から見た状況を一意に表現する状態データについて責任を持つ
    /// </summary>
    public class GameState
    {
        public int MyPlayerId { get; set; }
        public int OpponentPlayerId { get; set; }
        public List<Card> MyHand { get; set; } = new();
        public FieldZone MyField { get; set; } = new();
        public FieldZone OpponentField { get; set; } = new();
        public int MyHP { get; set; }
        public int OpponentHP { get; set; }
        public int MyMP { get; set; }
        public int OpponentMP { get; set; }

        /// <summary>
        /// 指定ユニットが既に他のユニットのペアリング対象になっている場合に true。
        /// プレイ時ペアリングの選択候補から除外するために使用する。
        /// </summary>
        public bool IsAlreadySomeonesPairingTarget(Unit u)
        {
            if (u == null) return false;
            if (MyField?.Units != null)
            {
                foreach (var v in MyField.Units)
                    if (v.PairingTarget == u) return true;
            }
            if (OpponentField?.Units != null)
            {
                foreach (var v in OpponentField.Units)
                    if (v.PairingTarget == u) return true;
            }
            return false;
        }

        /// <summary>
        /// 自プレイヤーのパートナーカードが既にいずれかのユニットのペアリング対象になっている場合に true。
        /// プレイ時ペアリングでパートナーカードを候補に出すかどうかの判定に使用する。
        /// </summary>
        public bool IsPartnerCardAlreadyPairingTarget()
        {
            if (MyField?.Units == null) return false;
            foreach (var u in MyField.Units)
                if (u.PairingWithPartnerCard) return true;
            return false;
        }
    }
}
