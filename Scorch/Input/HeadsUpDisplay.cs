using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Scorch;
using Scorch.DataModels;
using Scorch.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scorch.Input
{
    public class HeadsUpDisplay
    {
        private GraphicsDevice GraphicsDevice;
        private SpriteFont Font;
        private Texture2D CurrentPlayerIndicatorTexture;
        private Texture2D AimOverlayTexture;
        private Texture2D PowerIndicatorTexture;
        private int PowerIndicatorDelayInMilliseconds;
        private Texture2D BackgroundTexture;
        private Rectangle BackgroundFootprint;
        private Vector2 ViewportSize;
        private Rectangle ViewportFootprint;

        public int BackgroundHeight { get; private set; }
        public string Mode { get; set; }
        public Vector2 AimOverlayPosition { get; set; }
        public int AimTouchId { get; set; }
        public Dictionary<string, InputControl> InputControls { get; private set; }

        public HeadsUpDisplay(
            GraphicsDevice graphicsDevice,
            Vector2 viewportSize,
            SpriteFont font,
            Dictionary<string, Texture2D> textureAssets)
        {
            GraphicsDevice = graphicsDevice;
            ViewportSize = viewportSize;
            ViewportFootprint = new Rectangle(0, 0, (int)ViewportSize.X, (int)ViewportSize.Y);
            Font = font;
            CurrentPlayerIndicatorTexture = textureAssets["Circle"];
            AimOverlayTexture = textureAssets["AimOverlay"];
            PowerIndicatorTexture = textureAssets["PowerIndicator"];
            Mode = HudMode.Aim;
            AimOverlayPosition = -Vector2.One;
            
            BackgroundTexture = GraphicsUtility.CreateTexture(graphicsDevice, 1, 1, Color.Black);
            BackgroundHeight = (int)(ViewportSize.Y * Constants.HUD.BackgroundHeightFactor);
            BackgroundFootprint = GraphicsUtility.AlignRectangle(
                GraphicsDevice.Viewport.TitleSafeArea,
                new Vector2(ViewportSize.X, BackgroundHeight),
                Align.Left | Align.Bottom);

            InputControls = new Dictionary<string, InputControl>();
            var buttonSize = new Vector2((int)(ViewportSize.X * Constants.HUD.ButtonWidthFactor), BackgroundHeight);
            var scalarButtonSize = new Vector2((int)(ViewportSize.X * Constants.HUD.ScalarButtonWidthFactor), BackgroundHeight);

            AddInputControl(
                "fireButton",
                "FIRE",
                Color.Red,
                buttonSize,
                Align.CenterX | Align.Bottom);

            AddInputControl(
                "resetButton",
                "RESET",
                Color.Green,
                buttonSize,
                Align.Right | Align.Top);

            var angleControl = new ScalarInputControl(
                GraphicsDevice,
                Font,
                "ANGLE",
                Color.Yellow,
                GraphicsUtility.AlignRectangle(
                    ViewportFootprint,
                    scalarButtonSize,
                    Align.Left | Align.Bottom),
                180f,
                0f);

            InputControls.Add("angleButton", angleControl);

            var powerControl = new ScalarInputControl(
                GraphicsDevice,
                Font,
                "POWER",
                Color.Cyan,
                GraphicsUtility.AlignRectangle(
                    ViewportFootprint,
                    scalarButtonSize,
                    Align.Right | Align.Bottom),
                0f,
                100f);

            InputControls.Add("powerButton", powerControl);
        }

        public void SetCurrentPlayer(Tank tank)
        {
            ((ScalarInputControl)InputControls["angleButton"]).Value = tank.BarrelAngleInDegrees;
            ((ScalarInputControl)InputControls["powerButton"]).Value = tank.Power;
        }

        public void Update(
            ScorchGame game,
            GameTime gameTime,
            Dictionary<int, TouchInput> touchInputs,
            List<TouchGesture> touchGestures)
        {
            ApplyGameTime(gameTime);
            UpdateInputControls(game, gameTime, touchInputs, touchGestures);

            if (Mode == HudMode.Aim)
            {
                if (AimTouchId == 0)
                {
                    var newAimTouch = touchInputs.Values.FirstOrDefault(t => !t.IsHandled);
                    if (newAimTouch != null)
                    {
                        newAimTouch.IsHandled = true;
                        AimTouchId = newAimTouch.Id;
                    }
                }

                if (AimTouchId != 0)
                {
                    ShowPowerIndicator();

                    if (touchInputs.ContainsKey(AimTouchId))
                    {
                        touchInputs[AimTouchId].IsHandled = true;

                        AimOverlayPosition = game.CurrentPlayerTank.BarrelOriginPosition;

                        var aim = touchInputs[AimTouchId].Latest.Position - AimOverlayPosition;
                        game.CurrentPlayerTank.SetAngleAndPowerByAimVector(
                            aim,
                            ViewportSize.Y / 32,
                            ViewportSize.Y / 4);

                        ((ScalarInputControl)InputControls["angleButton"]).Value = game.CurrentPlayerTank.BarrelAngleInDegrees;
                        ((ScalarInputControl)InputControls["powerButton"]).Value = game.CurrentPlayerTank.Power;
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
                string.Format("HUD mode: {0}", Mode),
                new Vector2(8f, 0f),
                Color.Black);

            game.SpriteBatch.Draw(
                BackgroundTexture,
                drawRectangle: BackgroundFootprint,
                depth: Constants.Graphics.DrawOrder.HudBack);

            if (Mode == HudMode.Aim)
            {
                if (Constants.HUD.CurrentPlayerIndicatorEnabled)
                {
                    game.SpriteBatch.Draw(
                        CurrentPlayerIndicatorTexture,
                        position: game.CurrentPlayerTank.BarrelOriginPosition,
                        origin: new Vector2(CurrentPlayerIndicatorTexture.Width, CurrentPlayerIndicatorTexture.Height) / 2f,
                        scale: Vector2.One * Constants.HUD.CurrentPlayerIndicatorScaleFactor,
                        depth: Constants.Graphics.DrawOrder.Back,
                        color: Color.White * Constants.HUD.CurrentPlayerIndicatorOpacity);
                }

                foreach (var inputControl in InputControls.Values)
                {
                    inputControl.Draw(game.SpriteBatch);
                }

                game.SpriteBatch.DrawString(
                    Font,
                    string.Format(
                        "\nangle: {0}\npower: {1}",
                        game.CurrentPlayerTank.BarrelAngleInDegrees.ToString(),
                        game.CurrentPlayerTank.Power.ToString()),
                    new Vector2(8f, 0f),
                    Color.Black);

                if (AimTouchId != 0 && Constants.HUD.AimIndicatorEnabled)
                {
                    game.SpriteBatch.Draw(
                        AimOverlayTexture,
                        position: AimOverlayPosition,
                        origin: new Vector2(96, 0),
                        scale: Vector2.One * 2f,
                        depth: Constants.Graphics.DrawOrder.TankMiddle,
                        color: Color.White * Constants.HUD.AimIndicatorOpacity);
                }

                if (Constants.HUD.PowerIndicatorEnabled && PowerIndicatorDelayInMilliseconds > 0)
                {
                    float scaleFactor = game.CurrentPlayerTank.Power / 100f * Constants.Graphics.PowerIndicatorScaleFactor;
                    game.SpriteBatch.Draw(
                        PowerIndicatorTexture,
                        position: game.CurrentPlayerTank.BarrelEndPosition,
                        scale: Vector2.One * scaleFactor,
                        origin: new Vector2(0f, 17f),
                        rotation: game.CurrentPlayerTank.BarrelAngleInRadians,
                        depth: Constants.Graphics.DrawOrder.Back,
                        color: Color.White * Constants.HUD.PowerIndicatorOpacity *
                            (1f * PowerIndicatorDelayInMilliseconds / Constants.HUD.PowerIndicatorDurationInMilliseconds));
                }
            }
            else if (Mode == HudMode.Locked)
            {
                const string lockedText = "scorch";

                game.SpriteBatch.DrawString(
                    Font,
                    lockedText,
                    GraphicsUtility.AlignText(BackgroundFootprint, Font, lockedText, Align.Center),
                    game.CurrentPlayerTank.Color,
                    0f,
                    Vector2.Zero,
                    1f,
                    SpriteEffects.None,
                    Constants.Graphics.DrawOrder.HudFront);
            }
        }

        private void ApplyGameTime(GameTime gameTime)
        {
            PowerIndicatorDelayInMilliseconds = Math.Max(0, PowerIndicatorDelayInMilliseconds - (int)gameTime.ElapsedGameTime.TotalMilliseconds);
        }

        private void UpdateInputControls(
            ScorchGame game,
            GameTime gameTime,
            Dictionary<int, TouchInput> touchInputs,
            List<TouchGesture> touchGestures)
        {
            foreach (var inputControl in InputControls.Values)
            {
                inputControl.Update(gameTime, touchInputs, touchGestures);
            }

            var angleButton = (ScalarInputControl)InputControls["angleButton"];
            if (angleButton.Active)
            {
                game.CurrentPlayerTank.BarrelAngleInDegrees = (int)angleButton.Value;
                ShowPowerIndicator();
            }

            var powerButton = (ScalarInputControl)InputControls["powerButton"];
            if (powerButton.Active)
            {
                game.CurrentPlayerTank.Power = (int)powerButton.Value;
                ShowPowerIndicator();
            }

            touchGestures.Clear();
        }

        private void ShowPowerIndicator()
        {
            PowerIndicatorDelayInMilliseconds = Constants.HUD.PowerIndicatorDurationInMilliseconds;
        }

        private void AddInputControl(string id, string text, Color color, Vector2 size, Align align)
        {
            var inputControl = new InputControl(
                GraphicsDevice,
                Font,
                text,
                color,
                GraphicsUtility.AlignRectangle(
                    ViewportFootprint,
                    size,
                    align));

            InputControls.Add(id, inputControl);
        }
    }
    public static class HudMode
    {
        public const string Aim = "Aim";
        public const string Locked = "Locked";
    }
}
