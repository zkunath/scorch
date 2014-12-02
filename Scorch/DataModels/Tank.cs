using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scorch.Graphics;
using Scorch.Physics;
using System;
using System.Collections.Generic;

namespace Scorch.DataModels
{
    public class Tank : FieldObject
    {
        private GraphicsDevice GraphicsDevice;
        private Texture2D TankTexture;
        private Texture2D BarrelTexture;
        private Color Color;

        private int _BarrelAngleInDegrees;
        public int BarrelAngleInDegrees
        {
            get
            {
                return _BarrelAngleInDegrees;
            }
            set
            {
                _BarrelAngleInDegrees = MathHelper.Clamp(
                    value,
                    Constants.HUD.MinAngleInDegrees,
                    Constants.HUD.MaxAngleInDegrees);
                ChildObjects["barrel"].RotationInRadians = BarrelAngleInRadians;
            }
        }
        public float BarrelAngleInRadians
        {
            get
            {
                return MathHelper.ToRadians(-BarrelAngleInDegrees);
            }
        }
        public Vector2 BarrelOriginPosition
        {
            get
            {
                return Position + ChildObjects["barrel"].Position;
            }
        }

        public Vector2 BarrelEndPosition
        {
            get
            {
                var barrel = ChildObjects["barrel"];
                return BarrelOriginPosition +
                    barrel.Size.X * barrel.Scale.X * new Vector2(
                        (float)Math.Cos(BarrelAngleInRadians),
                        (float)Math.Sin(BarrelAngleInRadians));
            }
        }

        private int _Power;
        public int Power
        {
            get
            {
                return _Power;
            }
            set
            {
                _Power = MathHelper.Clamp(
                    value,
                    Constants.HUD.MinPower,
                    Constants.HUD.MaxPower);
            }
        }

        public Tank(
            string name,
            GraphicsDevice graphicsDevice,
            Dictionary<string, Texture2D> textureAssets,
            Color color)
            : base(
                PhysicsType.Tank,
                "player_" + name,
                textureAssets["Tank"],
                Vector2.Zero
            )
        {
            GraphicsDevice = graphicsDevice;
            Depth = Constants.Graphics.DrawOrder.TankMiddle;
            TankTexture = textureAssets["Tank"];
            BarrelTexture = textureAssets["TankBarrel"];
            var barrelPosition = new Vector2(16f, 4.5f);

            var barrel = new FieldObject(
                PhysicsType.Tank,
                "barrel",
                BarrelTexture,
                barrelPosition);

            barrel.Origin = new Vector2(1f, 3.5f);
            barrel.Depth = Constants.Graphics.DrawOrder.TankBack;
            barrel.RotationInRadians = BarrelAngleInRadians;
            ChildObjects.Add(barrel.Id, barrel);

            PhysicalProperties |= Physics.PhysicalProperties.AffectedByGravity;
        }

        public void SetColor(Color color)
        {
            Color = color;
            Texture = GraphicsUtility.ColorizeTexture(GraphicsDevice, TankTexture, color, Constants.Graphics.TankColorizeAmount);
            ChildObjects["barrel"].Texture = GraphicsUtility.ColorizeTexture(GraphicsDevice, BarrelTexture, color, Constants.Graphics.TankColorizeAmount);
        }

        public void SetAngleAndPowerByAimVector(Vector2 aim, float minLength, float maxLength)
        {
            if (aim.Y > 0)
            {
                var normalizedAim = new Vector2(aim.X, aim.Y);
                normalizedAim.Normalize();
                BarrelAngleInDegrees = (int)MathHelper.ToDegrees((float)Math.Acos(-normalizedAim.X));
            }
            else
            {
                BarrelAngleInDegrees = (aim.X > 0 ? 180 : 0);
            }

            float aimLength = Math.Min(aim.Length(), maxLength);
            aimLength = (aimLength <= minLength ? 0 : aimLength);
            Power = (int)(aimLength / maxLength * 100);
        }

        public override void HandleCollision(ScorchGame game, Collision collision)
        {
            if (collision.CollisionObjectPhysicsType == PhysicsType.Terrain)
            {
                // TODO: player damage based on impact velocity
                game.PhysicsEngine.StopFallingObjectOnTerrain(this, (Terrain)collision.CollisionObject);
            }
            else if (collision.CollisionObjectPhysicsType == PhysicsType.Projectile)
            {
                SetColor(GraphicsUtility.Blacken(Color, Constants.Graphics.TankScorchBlackness));
            }
        }
    }
}
