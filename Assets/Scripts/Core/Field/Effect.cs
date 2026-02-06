using System;
using CardBattle.Core.Enums;

namespace CardBattle.Core.Field
{
    /// <summary>
    /// 効果の定義について責任を持つ
    /// </summary>
    [Serializable]
    public class Effect
    {
        private EffectTriggerTiming triggerTiming;
        private Action effectLogic;

        public EffectTriggerTiming TriggerTiming
        {
            get => triggerTiming;
            set => triggerTiming = value;
        }

        public Action EffectLogic
        {
            get => effectLogic;
            set => effectLogic = value;
        }
    }
}
