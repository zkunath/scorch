using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Scorch.DataModels;
using System;

namespace Scorch.Graphics
{
    public class InputControl
    {
        private string Text;
        private Rectangle Footprint;
        private Vector2 TextPosition;
        private SpriteFont Font;
        private Texture2D BackgroundTexture;
        private Texture2D ActiveOverlayTexture;
        private bool Active;
        private bool NeedsToProcessButtonPressed;
        private event ScorchGame.GameEventHandler ButtonPressed;
        private ScorchGame Game;

        public InputControl(
            GraphicsDevice graphicsDevice,
            SpriteFont font,
            string text,
            Color color,
            Rectangle footprint)
        {
            BackgroundTexture = GraphicsUtility.CreateTexture(graphicsDevice, 1, 1, color);
            ActiveOverlayTexture = GraphicsUtility.CreateTexture(graphicsDevice, 1, 1, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            Footprint = footprint;
            TextPosition = GraphicsUtility.AlignText(footprint, font, text, Align.Center);
            Text = text;
            Font = font;
        }

        public void AddOnButtonPressedEventHandler(ScorchGame.GameEventHandler eventHandler, ScorchGame game)
        {
            ButtonPressed += eventHandler;
            Game = game;
        }

        public void Update(GameTime gameTime, TouchCollection touchPanelState)
        {
            if (NeedsToProcessButtonPressed)
            {
                if (ButtonPressed != null && Game != null)
                {
                    ButtonPressed(Game);
                }

                NeedsToProcessButtonPressed = false;
            }

            bool anyTouchOnFootprint = false;

            foreach (TouchLocation touchLocation in touchPanelState)
            {
                if (Footprint.Contains(touchLocation.Position))
                {
                    anyTouchOnFootprint = true;

                    if (touchLocation.State == TouchLocationState.Pressed)
                    {
                        NeedsToProcessButtonPressed = true;
                    }
                }
            }

            Active = anyTouchOnFootprint;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                BackgroundTexture,
                drawRectangle: Footprint,
                depth: DrawOrder.HudMiddle);

            if (Active)
            {
                spriteBatch.Draw(
                    ActiveOverlayTexture,
                    drawRectangle: Footprint,
                    depth: DrawOrder.HudTop);
            }

            spriteBatch.DrawString(
                Font,
                Text,
                TextPosition,
                Color.Black,
                0f,
                Vector2.Zero,
                1f,
                SpriteEffects.None,
                DrawOrder.HudFront);
        }
    }
}
