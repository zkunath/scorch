using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scorch;
using Scorch.DataModels;
using Scorch.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Scorch.Input
{
    public class HeadsUpDisplay
    {
        private GraphicsDevice GraphicsDevice;
        private SpriteFont Font;
        private Texture2D AimOverlayTexture;
        private Texture2D PowerIndicatorTexture;
        private Texture2D BackgroundTexture;
        private Rectangle BackgroundFootprint;
        private Vector2 Position;
        private int Width;
        private int Height;
        
        public string Mode { get; set; }
        public Vector2 AimOverlayPosition { get; set; }
        public int AimTouchId { get; set; }
        public Dictionary<string, InputControl> InputControls { get; private set; }

        public HeadsUpDisplay(
            GraphicsDevice graphicsDevice,
            SpriteFont font,
            Texture2D aimOverlayTexture,
            Texture2D powerIndicatorTexture)
        {
            Width = graphicsDevice.Viewport.TitleSafeArea.Width;
            Height = graphicsDevice.Viewport.TitleSafeArea.Height;

            GraphicsDevice = graphicsDevice;
            Font = font;
            AimOverlayTexture = aimOverlayTexture;
            PowerIndicatorTexture = powerIndicatorTexture;
            Position = new Vector2(32f, 32f);
            Mode = HudMode.Aim;
            AimOverlayPosition = -Vector2.One;
            
            BackgroundTexture = GraphicsUtility.CreateTexture(graphicsDevice, 1, 1, new Color(0f, 0f, 0f, 0.25f));
            int backgroundHeight = (int)(Height * Constants.HUD.BackgroundHeightFactor);
            BackgroundFootprint = GraphicsUtility.AlignRectangle(
                GraphicsDevice.Viewport.TitleSafeArea,
                new Vector2(Width, backgroundHeight),
                Align.Left | Align.Bottom);

            InputControls = new Dictionary<string, InputControl>();
            var buttonSize = new Vector2((int)(Width * Constants.HUD.ButtonWidthFactor), backgroundHeight);

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

        public void Update(ScorchGame game, GameTime gameTime, Dictionary<int, TouchInput> touchInputs)
        {
            foreach (var inputControl in InputControls.Values)
            {
                inputControl.Update(gameTime, touchInputs);
            }

            if (Mode == HudMode.Aim)
            {
                if (AimTouchId == 0)
                {
                    var newAimTouch = touchInputs.Values.FirstOrDefault(t => !t.LatestIsHandled);
                    if (newAimTouch != null)
                    {
                        newAimTouch.LatestIsHandled = true;
                        AimTouchId = newAimTouch.Id;
                    }
                }

                if (AimTouchId != 0)
                {
                    if (touchInputs.ContainsKey(AimTouchId))
                    {
                        touchInputs[AimTouchId].LatestIsHandled = true;

                        AimOverlayPosition = game.CurrentPlayerTank.BarrelOriginPosition;

                        var aim = touchInputs[AimTouchId].Latest.Position - AimOverlayPosition;
                        game.CurrentPlayerTank.SetAngleAndPowerByTouchGesture(
                            aim,
                            Height / 32,
                            Height / 4);
                    }
                    else
                    {
                        AimTouchId = 0;
                    }
                }
            }
        }

        public void Draw(ScorchGame game)
        {
            game.SpriteBatch.DrawString(
                Font,
                string.Format(
                    "HUD mode: {0}\n{1}\nangle: {2}\npower: {3}",
                    Mode,
                    game.CurrentPlayerTank.Id,
                    game.CurrentPlayerTank.BarrelAngleInDegrees.ToString(),
                    game.CurrentPlayerTank.Power.ToString()),
                Position,
                Color.Black);

            game.SpriteBatch.Draw(
                BackgroundTexture,
                drawRectangle: BackgroundFootprint,
                depth: Constants.Graphics.DrawOrder.HudBack);

            foreach (var inputControl in InputControls.Values)
            {
                inputControl.Draw(game.SpriteBatch);
            }

            if (AimTouchId != 0)
            {
                game.SpriteBatch.Draw(
                    AimOverlayTexture,
                    position: AimOverlayPosition,
                    origin: new Vector2(96, 0),
                    scale: Vector2.One * 2f,
                    depth: Constants.Graphics.DrawOrder.TankMiddle);

                // 190px = radius of aim indicator asset
                // 128px = radius of power indicator asset
                float scaleFactor = game.CurrentPlayerTank.Power / 100f * 190f / 128f;

                game.SpriteBatch.Draw(
                    PowerIndicatorTexture,
                    position: game.CurrentPlayerTank.BarrelOriginPosition,
                    scale: Vector2.One * scaleFactor,
                    origin: new Vector2(0f, 17f),
                    rotation: game.CurrentPlayerTank.BarrelAngleInRadians,
                    depth: Constants.Graphics.DrawOrder.Back);
            }
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
        private static class HudMode
        {
            public const string Explore = "Explore";
            public const string Aim = "Aim";
            public const string Locked = "Locked";
        }
    }
}
