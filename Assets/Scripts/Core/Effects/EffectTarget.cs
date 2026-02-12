namespace CardBattle.Core.Effects
{
    /// <summary>
    /// 効果の対象（ユニット・プレイヤー・なし）を表す構造体
    /// </summary>
    public struct EffectTarget
    {
        public EffectTargetKind Kind { get; private set; }
        public int? UnitInstanceId { get; private set; }
        public int? PlayerId { get; private set; }

        private EffectTarget(EffectTargetKind kind, int? unitInstanceId, int? playerId)
        {
            Kind = kind;
            UnitInstanceId = unitInstanceId;
            PlayerId = playerId;
        }

        public static EffectTarget None()
        {
            return new EffectTarget(EffectTargetKind.None, null, null);
        }

        public static EffectTarget Unit(int instanceId)
        {
            return new EffectTarget(EffectTargetKind.Unit, instanceId, null);
        }

        public static EffectTarget Player(int playerId)
        {
            return new EffectTarget(EffectTargetKind.Player, null, playerId);
        }
    }
}
