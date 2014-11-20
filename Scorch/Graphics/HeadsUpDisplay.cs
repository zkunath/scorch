using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scorch.DataModels;

namespace Scorch.Graphics
{
    public class HeadsUpDisplay
    {
        private GraphicsDevice GraphicsDevice;
        private SpriteBatch SpriteBatch;
        private SpriteFont Font;
        private Texture2D AimOverlayTexture;
        private Vector2 Position;
        private const string HudFormat = "{0}\nangle: {1}\npower: {2}";

        public string Mode { get; set; }
        public Vector2 AimOverlayPosition { get; set; }

        public HeadsUpDisplay(GraphicsDevice graphicsDevice, SpriteFont font, Texture2D aimOverlayTexture)
        {
            GraphicsDevice = graphicsDevice;
            Font = font;
            AimOverlayTexture = aimOverlayTexture;
            SpriteBatch = new SpriteBatch(graphicsDevice);
            Position = new Vector2(32f, 32f);
            Mode = HudMode.Aim;
            AimOverlayPosition = -Vector2.One;
        }

        public void Draw(params string[] args)
        {
            SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            SpriteBatch.DrawString(
                Font,
                "HUD mode: " + Mode + "\n" + string.Format(HudFormat, args),
                Position,
                Color.Black);

            if (AimOverlayPosition != -Vector2.One)
            {
                SpriteBatch.Draw(
                    AimOverlayTexture,
                    position: AimOverlayPosition,
                    origin: new Vector2(96, 0),
                    scale: Vector2.One * 2f,
                    depth: DrawOrder.Front);
            }


            SpriteBatch.End();
        }
    }

    public struct HudMode
    {
        public const string Explore = "Explore";
        public const string Aim = "Aim";
        public const string Locked = "Locked";
    }
}
