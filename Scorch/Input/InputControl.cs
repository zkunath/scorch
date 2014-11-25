using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scorch;
using Scorch.DataModels;
using Scorch.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Scorch.Input
{
    public class InputControl
    {
        private string Text;
        private Rectangle Footprint;
        private Vector2 TextPosition;
        private SpriteFont Font;
        private Texture2D BackgroundTexture;
        private Texture2D ActiveOverlayTexture;
        private Texture2D PressedOverlayTexture;
        private bool Active;
        private int ActiveTouchInputId;
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
            var activeColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            BackgroundTexture = GraphicsUtility.CreateTexture(graphicsDevice, 1, 1, color);
            ActiveOverlayTexture = GraphicsUtility.CreateTexture(graphicsDevice, 1, 1, activeColor);
            PressedOverlayTexture = GraphicsUtility.CreateTexture(graphicsDevice, 1, 1, Color.Lerp(activeColor, Color.White, 0.5f));
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

        public void Update(GameTime gameTime, Dictionary<int, TouchInput> touchInputs)
        {
            if (NeedsToProcessButtonPressed)
            {
                ProcessButtonPressed();
            }

            if (ActiveTouchInputId == 0)
            {
                var newTouchOnMe = touchInputs.Values.FirstOrDefault(t => Footprint.Contains(t.Origin.Position) && !t.LatestIsHandled);
                if (newTouchOnMe != null)
                {
                    ActiveTouchInputId = newTouchOnMe.Id;
                    newTouchOnMe.LatestIsHandled = true;
                }
            }

            if (ActiveTouchInputId != 0)
            {
                if (touchInputs.ContainsKey(ActiveTouchInputId))
                {
                    touchInputs[ActiveTouchInputId].LatestIsHandled = true;
                    Active = Footprint.Contains(touchInputs[ActiveTouchInputId].Latest.Position);
                }
                else
                {
                    NeedsToProcessButtonPressed = Active;
                    Active = false;
                    ActiveTouchInputId = 0;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                BackgroundTexture,
                drawRectangle: Footprint,
                depth: Constants.Graphics.DrawOrder.HudMiddle);

            if (NeedsToProcessButtonPressed)
            {
                spriteBatch.Draw(
                    PressedOverlayTexture,
                    drawRectangle: Footprint,
                    depth: Constants.Graphics.DrawOrder.HudTop);
            }
            else if (Active)
            {
                spriteBatch.Draw(
                    ActiveOverlayTexture,
                    drawRectangle: Footprint,
                    depth: Constants.Graphics.DrawOrder.HudTop);
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
                Constants.Graphics.DrawOrder.HudFront);
        }

        private void ProcessButtonPressed()
        {
            if (ButtonPressed != null && Game != null)
            {
                ButtonPressed(Game);
            }

            NeedsToProcessButtonPressed = false;
        }
    }
}
