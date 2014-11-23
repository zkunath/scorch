using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Scorch.DataModels;
using System.Collections.Generic;

namespace Scorch.Graphics
{
    public class HeadsUpDisplay
    {
        private GraphicsDevice GraphicsDevice;
        private SpriteBatch SpriteBatch;
        private SpriteFont Font;
        private Texture2D AimOverlayTexture;
        private Texture2D BackgroundTexture;
        private Rectangle BackgroundFootprint;
        private Vector2 Position;
        private int Width;
        private int Height;
        private const string HudFormat = "{0}\nangle: {1}\npower: {2}";
        
        public string Mode { get; set; }
        public Vector2 AimOverlayPosition { get; set; }
        public Dictionary<string, InputControl> InputControls { get; private set; }

        public HeadsUpDisplay(GraphicsDevice graphicsDevice, SpriteFont font, Texture2D aimOverlayTexture)
        {
            const float backgroundHeightFactor = 0.125f;
            const float buttonWidthFactor = 0.25f;

            Width = graphicsDevice.Viewport.TitleSafeArea.Width;
            Height = graphicsDevice.Viewport.TitleSafeArea.Height;

            GraphicsDevice = graphicsDevice;
            Font = font;
            AimOverlayTexture = aimOverlayTexture;
            SpriteBatch = new SpriteBatch(graphicsDevice);
            Position = new Vector2(32f, 32f);
            Mode = HudMode.Aim;
            AimOverlayPosition = -Vector2.One;
            
            BackgroundTexture = GraphicsUtility.CreateTexture(graphicsDevice, 1, 1, new Color(0f, 0f, 0f, 0.25f));
            int backgroundHeight = (int)(Height * backgroundHeightFactor);
            BackgroundFootprint = GraphicsUtility.AlignRectangle(
                GraphicsDevice.Viewport.TitleSafeArea,
                new Vector2(Width, backgroundHeight),
                Align.Left | Align.Bottom);

            InputControls = new Dictionary<string, InputControl>();
            var buttonSize = new Vector2((int)(Width * buttonWidthFactor), backgroundHeight);

            AddInputControl(
                "playerButton",
                "PLAYER",
                Color.Blue,
                buttonSize,
                Align.Left | Align.CenterY);

            AddInputControl(
                "fireButton",
                "FIRE",
                Color.Red,
                buttonSize,
                Align.Center);

            AddInputControl(
                "terrainButton",
                "TERRAIN",
                Color.Green,
                buttonSize,
                Align.Right | Align.CenterY);
        }

        public void Update(GameTime gameTime, TouchCollection touchPanelState)
        {
            foreach (var inputControl in InputControls.Values)
            {
                inputControl.Update(gameTime, touchPanelState);
            }
        }

        public void Draw(params string[] args)
        {
            
            SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            SpriteBatch.DrawString(
                Font,
                "HUD mode: " + Mode + "\n" + string.Format(HudFormat, args),
                Position,
                Color.Black);

            SpriteBatch.Draw(
                BackgroundTexture,
                drawRectangle: BackgroundFootprint,
                depth: DrawOrder.HudBack);

            foreach (var inputControl in InputControls.Values)
            {
                inputControl.Draw(SpriteBatch);
            }

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

        private void AddInputControl(string id, string text, Color color, Vector2 size, Align align)
        {
            var inputControl = new InputControl(
                GraphicsDevice,
                Font,
                text,
                color,
                GraphicsUtility.AlignRectangle(
                    BackgroundFootprint,
                    size,
                    align));

            InputControls.Add(id, inputControl);
        }
    }

    public struct HudMode
    {
        public const string Explore = "Explore";
        public const string Aim = "Aim";
        public const string Locked = "Locked";
    }
}
