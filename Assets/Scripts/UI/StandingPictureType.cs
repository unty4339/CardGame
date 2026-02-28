namespace CardBattle.UI
{
    /// <summary>
    /// 立ち絵の種類を網羅的に定義するクラス定数。
    /// 各定数値は Addressables のアドレス（パス）として使用する。
    /// </summary>
    public static class StandingPictureType
    {
        public const string None = "";

        /// <summary>通常</summary>
        public const string Normal = "Assets/Images/Stand/通常.png";

        /// <summary>照れ</summary>
        public const string Embarrassed = "Assets/Images/Stand/照れ.png";

        /// <summary>焦り</summary>
        public const string Anxious = "Assets/Images/Stand/焦り.png";

        /// <summary>銃構え</summary>
        public const string GunStance = "Assets/Images/Stand/銃構え.png";

        /// <summary>銃構え照れ</summary>
        public const string GunStanceEmbarrassed = "Assets/Images/Stand/銃構え照れ.png";

        /// <summary>ペアリング用：バック</summary>
        public const string Back = "Assets/Images/Stand/バック.png";

        /// <summary>ペアリング用：騎乗</summary>
        public const string Riding = "Assets/Images/Stand/騎乗.png";

        /// <summary>ペアリング用：オーク</summary>
        public const string Ogre = "Assets/Images/Stand/オーク.png";
    }
}
