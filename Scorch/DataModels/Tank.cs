using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
                ChildObjects["powerIndicator"].Scale = new Vector2(_Power / 100f * 190f / 128f, 1f);
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

            Texture = ColorizeTexture(graphicsDevice, Texture, Color);

            var barrel = new FieldObject(
                "barrel",
                ColorizeTexture(graphicsDevice, barrelTexture, Color),
                new Vector2(16f, 3.5f));

            barrel.Origin = new Vector2(1f, 3.5f);
            barrel.Depth = DrawOrder.Middle;
            barrel.RotationInRadians = BarrelAngleInRadians;
            ChildObjects.Add(barrel.Id, barrel);

            var powerIndicator = new FieldObject(
                "powerIndicator",
                powerIndicatorTexture,
                new Vector2(16f, 3.5f));

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

        private Texture2D ColorizeTexture(GraphicsDevice graphicsDevice, Texture2D texture, Color color)
        {
            Color[] textureData = new Color[texture.Width * texture.Height];
            texture.GetData(textureData);
            for (int x = 0; x < texture.Width; x++)
            {
                for (int y = 0; y < texture.Height; y++)
                {
                    var currentColor = textureData[x + y * texture.Width];
                    if (currentColor != Color.Transparent)
                    {
                        textureData[x + y * texture.Width] = Color.Lerp(currentColor, color, 0.5f);
                    }
                }
            }

            var colorizedTexture = new Texture2D(graphicsDevice, texture.Width, texture.Height);
            colorizedTexture.SetData(textureData);
            return colorizedTexture;
        }
    }
}
