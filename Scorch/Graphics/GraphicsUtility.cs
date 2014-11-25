using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Scorch.Graphics
{
    public static class GraphicsUtility
    {
        public static Rectangle AlignRectangle(Rectangle bounds, Vector2 size, Align align)
        {
            float top = 0;
            float left = 0;

            if (align.HasFlag(Align.Left))
            {
                left = bounds.Left;
            }
            else if (align.HasFlag(Align.Right))
            {
                left = bounds.Width - size.X;
            }
            else if (align.HasFlag(Align.CenterX))
            {
                left = bounds.Left + (bounds.Width - size.X) / 2f;
            }

            if (align.HasFlag(Align.Top))
            {
                top = bounds.Top;
            }
            else if (align.HasFlag(Align.Bottom))
            {
                top = bounds.Height - size.Y;
            }
            else if (align.HasFlag(Align.CenterY))
            {
                top = bounds.Top + (bounds.Height - size.Y) / 2f;
            }

            return new Rectangle((int)left, (int)top, (int)size.X, (int)size.Y);
        }

        public static Vector2 AlignText(Rectangle bounds, SpriteFont font, string text, Align align)
        {
            var textSize = font.MeasureString(text);
            var alignedTextRectangle = AlignRectangle(bounds, textSize, align);
            return new Vector2(alignedTextRectangle.X, alignedTextRectangle.Y);
        }

        public static Texture2D CreateTexture(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            var texture = new Texture2D(graphicsDevice, width, height);
            return FillTexture(texture, color);
        }

        public static Texture2D FillTexture(Texture2D texture, Color color)
        {
            Color[] textureData = new Color[texture.Width * texture.Height];
            for (int i = 0; i < textureData.Length; i++)
            {
                textureData[i] = color;
            }

            texture.SetData(textureData);
            return texture;
        }

        public static Color[] GetTextureSample(Texture2D texture, Rectangle sampleRectangle)
        {
            var textureData = new Color[sampleRectangle.Width * sampleRectangle.Height];

            texture.GetData(
                0,
                sampleRectangle,
                textureData,
                0,
                textureData.Length);

            return textureData;
        }

        public static void SetTextureSample(Texture2D texture, Rectangle sampleRectangle, Color[] textureData)
        {
            texture.SetData(
                0,
                sampleRectangle,
                textureData,
                0,
                textureData.Length);
        }

        public static Texture2D ColorizeTexture(GraphicsDevice graphicsDevice, Texture2D texture, Color color)
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

        public static Color RandomizedFade(Random randomizer, Color from, Color to, float ratio, float randomization)
        {
            var gradientColorData = new Color(from.ToVector3() + (to.ToVector3() - from.ToVector3()) * ratio);
            return RandomizeColor(randomizer, gradientColorData, randomization);
        }

        public static Color RandomizeColor(Random randomizer, Color color, float randomization)
        {
            var randomizedColorDataModifier = new Vector3(
                (float)randomizer.NextDouble() * randomization - randomization / 2,
                (float)randomizer.NextDouble() * randomization - randomization / 2,
                (float)randomizer.NextDouble() * randomization - randomization / 2);

            return new Color(color.ToVector3() + randomizedColorDataModifier);
        }

        public static Color ChooseRandomColor(Random randomizer, float randomization)
        {
            float primaryMin = 1f - randomization;
            float primaryMax = 1f;
            float secondaryMin = (1f - randomization) / 2f;
            float secondaryMax = 1f - secondaryMin;
            float tertiaryMin = 0f;
            float tertiaryMax = randomization;

            float primary = primaryMin + (float)(randomizer.NextDouble() * (primaryMax - primaryMin));
            float secondary = secondaryMin + (float)(randomizer.NextDouble() * (secondaryMax - secondaryMin));
            float tertiary = tertiaryMin + (float)(randomizer.NextDouble() * (tertiaryMax - tertiaryMin));

            int combo = randomizer.Next(6);
            return new Color(
                combo == 0 || combo == 1 ? primary : (combo == 2 || combo == 3 ? secondary : tertiary),
                combo == 2 || combo == 4 ? primary : (combo == 0 || combo == 5 ? secondary : tertiary),
                combo == 3 || combo == 5 ? primary : (combo == 1 || combo == 4 ? secondary : tertiary));
        }

        public static Color Whiten(Color color, float whiteness)
        {
            return Color.Lerp(color, Color.White, whiteness);
        }

        public static Color Blacken(Color color, float blackness)
        {
            return Color.Lerp(color, Color.Black, blackness);
        }
    }

    [Flags]
    public enum Align
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,
        CenterX = 16,
        CenterY = 32,
        Center = 48
    }
}
