using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scorch.Graphics;
using System;

namespace Scorch.DataModels
{
    public class Tank : FieldObject
    {
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
                _BarrelAngleInDegrees = MathHelper.Clamp(value, 0, 180);
                ChildObjects["barrel"].RotationInRadians = BarrelAngleInRadians;
                ChildObjects["powerIndicator"].RotationInRadians = BarrelAngleInRadians;
            }
        }
        public float BarrelAngleInRadians
        {
            get
            {
                return MathHelper.ToRadians(-BarrelAngleInDegrees);
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
                // 190px = radius of aim indicator asset
                // 128px = radius of power indicator asset
                float scaleFactor = _Power / 100f * 190f / 128f;
                ChildObjects["powerIndicator"].Scale = Vector2.One * scaleFactor;
            }
        }

        public Tank(
            string name,
            GraphicsDevice graphicsDevice,
            Texture2D tankTexture,
            Texture2D barrelTexture,
            Texture2D powerIndicatorTexture,
            Color color)
            : base(
                "player_" + name,
                tankTexture,
                Vector2.Zero
            )
        {
            Color = color;
            Depth = DrawOrder.Front;
            Texture = GraphicsUtility.ColorizeTexture(graphicsDevice, Texture, Color);
            var barrelPosition = new Vector2(16f, 4.5f);

            var barrel = new FieldObject(
                "barrel",
                GraphicsUtility.ColorizeTexture(graphicsDevice, barrelTexture, Color),
                barrelPosition);

            barrel.Origin = new Vector2(1f, 3.5f);
            barrel.Depth = DrawOrder.Middle;
            barrel.RotationInRadians = BarrelAngleInRadians;
            ChildObjects.Add(barrel.Id, barrel);

            var powerIndicator = new FieldObject(
                "powerIndicator",
                powerIndicatorTexture,
                barrelPosition);

            powerIndicator.Origin = new Vector2(0f, 17f);
            powerIndicator.Depth = DrawOrder.Back;
            powerIndicator.RotationInRadians = BarrelAngleInRadians;
            powerIndicator.Visible = false;
            ChildObjects.Add(powerIndicator.Id, powerIndicator);

            Power = 100;
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
