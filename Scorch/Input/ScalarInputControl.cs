using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private float InputScale;
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
                return _UnscaledValue / InputScale;
            }
            set
            {
                _UnscaledValue = MathHelper.Clamp(
                    value * InputScale,
                    Math.Min(UnscaledLeftValue, UnscaledRightValue) * InputScale,
                    Math.Max(UnscaledLeftValue, UnscaledRightValue) * InputScale);
            }
        }

        public ScalarInputControl(
            GraphicsDevice graphicsDevice,
            SpriteFont font,
            string text,
            Color color,
            Rectangle footprint,
            float leftValue,
            float rightValue,
            float inputScale) 
            : base(
                graphicsDevice,
                font,
                text,
                color,
                footprint)
        {
            InputScale = inputScale;
            UnscaledLeftValue = leftValue * inputScale;
            UnscaledRightValue = rightValue * inputScale;
        }

        public override void HandleTouchInput(TouchInput touchInput)
        {
            base.HandleTouchInput(touchInput);
            _UnscaledValue += (touchInput.Latest.Position.X - touchInput.Previous.Position.X) * (UnscaledLeftValue > UnscaledRightValue ? -1 : 1);
        }
    }
}
