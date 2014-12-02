namespace Scorch
{
    public static class Constants
    {
        public static class Game
        {
            public const int NumPlayers = 3;
        }
        
        public static class Physics
        {
            public const float GravityAcceleration = 300;
            public const int ExplosionDurationInMilliseconds = 50;
            public const float ExplosionBaseRadius = 64f;
            public const float ExplosionBaseDamage = 100f;
            public const float ExplosionScorchRadiusFactor = 1.1f;
            public const float CollisionDetectionMinDistancePerStep = 3f;
            public const float PlayerPowerVelocityFactor = 8f;
        }

        public static class HUD
        {
            public const bool ConsumeDragGesture = true;
            public const bool AimIndicatorEnabled = false;
            public const float AimOverlayOpacity = 0.75f;
            public const int PowerIndicatorDurationInMilliseconds = 500;

            public const float BackgroundHeightFactor = 0.2f;
            public const float ButtonWidthFactor = 0.25f;
            public const float ScalarButtonWidthFactor = 0.375f;
            public const float ScalarButtonScaleFactor = 16f;

            public const int MinAngleInDegrees = 0;
            public const int MaxAngleInDegrees = 180;
            public const int MinPower = 0;
            public const int MaxPower = 100;
            public const int InitialPower = 50;
        }

        public static class Graphics
        {
            public const float ProjectileScale = 0.075f;
            public const float TankColorizeAmount = 0.7f;
            public const float TankScorchBlackness = 0.8f;
            public const float PowerIndicatorScaleFactor = 190f / 128f; // ratio of aim indicator asset to power indicator asset

            public static class DrawOrder
            {
                public const float FarBack = 0.1f;
                public const float Back = 0.2f;
                public const float TankBack = 0.3f;
                public const float TankMiddle = 0.4f;
                public const float TankFront = 0.5f;
                public const float HudBack = 0.7f;
                public const float HudMiddle = 0.8f;
                public const float HudFront = 0.9f;
                public const float HudTop = 1f;
            }
        }

        public static class Terrain
        {
            public const float PlayerColorRandomness = 0.7f;
            public const float TerrainColorRandomness = 0.7f;
            public const float TopColorWhiteness = 0.3f;
            public const float BottomColorBlackness = 0.5f;
            public const float SpeckleColorBlackness = 0.4f;
            public const float GradientRandomization = 0.04f;
            public const float SpeckleColorRandomization = 0.2f;
            public const float SpeckleChance = 0.001f;
            public const float ScorchBlackness = 0.8f;
        }

        public static class Debug
        {
            public const bool DrawFootprints = false;
            public const bool DrawTerrainHeightMap = true;
            public const bool EnableFrameRateCounter = false;
            public const bool EnableRedrawRegions = true;
            public const bool EnableCacheVisualization = false;
        }
    }
}
