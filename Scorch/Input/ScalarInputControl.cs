using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scorch;
using Scorch.DataModels;
using Scorch.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scorch.Input
{
    public class ScalarInputControl : InputControl
    {
        private float LeftValue;
        private float RightValue;

        private float _Value;
        public float Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = MathHelper.Clamp(
                    value,
                    Math.Min(LeftValue, RightValue),
                    Math.Max(LeftValue, RightValue));
            }
        }

        public ScalarInputControl(
            GraphicsDevice graphicsDevice,
            SpriteFont font,
            string text,
            Color color,
            Rectangle footprint,
            float value,
            float leftValue,
            float rightValue) 
            : base(
                graphicsDevice,
                font,
                text,
                color,
                footprint)
        {
            _Value = value;
            LeftValue = leftValue;
            RightValue = rightValue;
        }

        public override void HandleTouchInput(TouchInput touchInput)
        {
            base.HandleTouchInput(touchInput);
            _Value += (touchInput.Latest.Position.X - touchInput.Previous.Position.X) * (LeftValue > RightValue ? -1 : 1);
        }
    }
}
