using System.Collections.Generic;
using CardBattle.AI;
using CardBattle.Core.Effects;
using CardBattle.Core.Field;

namespace CardBattle.ScriptableObjects
{
    /// <summary>
    /// ゴブリンの騎兵。登場時ペア。ペアリング中：攻撃できない付与・攻撃力コピー・戦闘破壊時ペア肩代わりは拡張が必要。
    /// </summary>
    public class GoblinCavalryUnitCard : UnitCardTemplateBase, IOnPairingEffect
    {
        public GoblinCavalryUnitCard()
        {
            cardName = "ゴブリンの騎兵";
            playCost = 3;
            baseHP = 1;
            baseAttack = 2;
            description = "登場時：\nペアリング中でない、自分パートナーか体力1の相手ユニットとペアリングする。\n\nペアリング中：\nペア対象に「攻撃できない」を付与する。\nこのカードの攻撃はペア対象の攻撃と等しくなる。\nこのカードが戦闘で破壊されるとき、代わりにペア対象を破壊する。";
        }

        public IList<EffectTarget> GetAvailableTargets(GameState state, Unit sourceUnit, bool isPartnerOnField)
        {
            var list = new List<EffectTarget>();
            if (state?.OpponentField?.Units != null)
            {
                foreach (var u in state.OpponentField.Units)
                {
                    if (u.HP == 1)
                        list.Add(EffectTarget.Unit(u.InstanceId));
                }
            }
            if (state?.MyField?.Units != null && isPartnerOnField)
            {
                foreach (var u in state.MyField.Units)
                {
                    if (u.IsPartner)
                        list.Add(EffectTarget.Unit(u.InstanceId));
                }
            }
            if (!isPartnerOnField && state != null)
                list.Add(EffectTarget.PartnerCard(state.MyPlayerId));
            return list;
        }

        public void Resolve(EffectTarget target, GameState state, Unit sourceUnit, Unit pairTargetUnitOrNull)
        {
            // ペアリングは呼び出し側で双方向に設定済み。攻撃できない付与・攻撃力コピー・戦闘破壊肩代わりは拡張で対応。
        }
    }
}
