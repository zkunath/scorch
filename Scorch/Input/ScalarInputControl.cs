using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Scorch;
using Scorch.DataModels;
using Scorch.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Scorch.Input
{
    public class ScalarInputControl : InputControl
    {
        private float UnscaledLeftValue;
        private float UnscaledRightValue;
        private float _UnscaledValue;
        public float UnscaledValue
        {
            get
            {
                return _UnscaledValue;
            }
            set
            {
                _UnscaledValue = MathHelper.Clamp(
                    value,
                    Math.Min(UnscaledLeftValue, UnscaledRightValue),
                    Math.Max(UnscaledLeftValue, UnscaledRightValue));
            }
        }

        public float Value
        {
            get
            {
                return ScaleDown(UnscaledValue);
            }
            set
            {
                UnscaledValue = ScaleUp(value);
            }
        }

        public ScalarInputControl(
            GraphicsDevice graphicsDevice,
            SpriteFont font,
            string text,
            Color color,
            Rectangle footprint,
            float leftValue,
            float rightValue) 
            : base(
                graphicsDevice,
                font,
                text,
                color,
                footprint)
        {
            UnscaledLeftValue = ScaleUp(leftValue);
            UnscaledRightValue = ScaleUp(rightValue);
        }

        public override void Update(
            GameTime gameTime,
            Dictionary<int, TouchInput> touchInputs,
            List<TouchGesture> touchGestures)
        {
            base.Update(gameTime, touchInputs, touchGestures);
        }

        protected override void HandleTouchInput(TouchInput touchInput)
        {
            base.HandleTouchInput(touchInput);

            if (!Constants.HUD.ConsumeDragGesture)
            {
                AddToUnscaledValue(touchInput.Latest.Position.X - touchInput.Previous.Position.X);
            }
        }

        protected override void HandleTouchGesture(TouchGesture touchGesture)
        {
            base.HandleTouchGesture(touchGesture);
            if (Constants.HUD.ConsumeDragGesture)
            {
                AddToUnscaledValue(touchGesture.Gesture.Delta.X);
            }
        }

        private void AddToUnscaledValue(float delta)
        {
            UnscaledValue += delta * Math.Sign(UnscaledRightValue - UnscaledLeftValue);
            Active = true;
        }

        private static float ScaleUp(float value)
        {
            return value * Constants.HUD.ScalarButtonScaleFactor;
        }

        private static float ScaleDown(float value)
        {
            return value / Constants.HUD.ScalarButtonScaleFactor;
        }
    }
}
