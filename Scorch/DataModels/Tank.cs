using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scorch.Graphics;
using System;

namespace Scorch.DataModels
{
    public class Tank : FieldObject
    {
        private GraphicsDevice GraphicsDevice;
        private Texture2D TankTexture;
        private Texture2D BarrelTexture;

        private int _BarrelAngleInDegrees;
        public int BarrelAngleInDegrees
        {
            get
            {
                return _BarrelAngleInDegrees;
            }
            set
            {
                _BarrelAngleInDegrees = MathHelper.Clamp(value, 0, 180);
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

        private int _Power;
        public int Power
        {
            get
            {
                return _Power;
            }
            set
            {
                _Power = MathHelper.Clamp(value, 0, 100);
            }
        }

        public Tank(
            string name,
            GraphicsDevice graphicsDevice,
            Texture2D tankTexture,
            Texture2D barrelTexture,
            Color color)
            : base(
                "player_" + name,
                tankTexture,
                Vector2.Zero
            )
        {
            GraphicsDevice = graphicsDevice;
            Depth = Constants.Graphics.DrawOrder.TankMiddle;
            TankTexture = tankTexture;
            BarrelTexture = barrelTexture;
            var barrelPosition = new Vector2(16f, 4.5f);

            var barrel = new FieldObject(
                "barrel",
                barrelTexture,
                barrelPosition);

            barrel.Origin = new Vector2(1f, 3.5f);
            barrel.Depth = Constants.Graphics.DrawOrder.TankBack;
            barrel.RotationInRadians = BarrelAngleInRadians;
            ChildObjects.Add(barrel.Id, barrel);

            //SetColor(color);

            Power = 100;

            PhysicsType |= Physics.PhysicsType.AffectedByGravity;
            PhysicsType |= Physics.PhysicsType.CollidesWithTerrain;
            PhysicsType |= Physics.PhysicsType.OnCollisionStop;
        }

        public void SetColor(Color color)
        {
            Texture = GraphicsUtility.ColorizeTexture(GraphicsDevice, TankTexture, color);
            ChildObjects["barrel"].Texture = GraphicsUtility.ColorizeTexture(GraphicsDevice, BarrelTexture, color);
        }

        public void SetAngleAndPowerByTouchGesture(Vector2 aim, float minLength, float maxLength)
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
    }
}
