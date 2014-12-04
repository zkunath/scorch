using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scorch;
using Scorch.Graphics;
using Scorch.Physics;
using System;

namespace Scorch.DataModels
{
    public class Terrain : FieldObject
    {
        private GraphicsDevice GraphicsDevice;
        private Random Randomizer;

        public int[] HeightMap { get; private set; }

        public Terrain(Random randomizer, GraphicsDevice graphicsDevice, int width, int height)
            : base(
                PhysicsType.Terrain,
                "terrain",
                new Texture2D(graphicsDevice, width, height),
                Vector2.Zero
            )
        {
            Randomizer = randomizer;
            GraphicsDevice = graphicsDevice;
            HeightMap = new int[width];

            Depth = Constants.Graphics.DrawOrder.FarBack;
        }

        public bool IsTerrainLocatedAtPosition(Vector2 position)
        {
            if (position.X >= 0 &&
                position.X < Texture.Width &&
                position.Y >= Texture.Height - HeightMap[(int)position.X] &&
                position.Y < Texture.Height)
            {
                return true;
            }

            return false;
        }

        public void Regenerate(Tank[] tanks)
        {
            int bufferWidth = (int)Size.X / 8;

            GenerateHeightMap();

            int fieldPartitionWidth = (int)Size.X / tanks.Length;
            for (int i = 0; i < tanks.Length; i++)
            {
                int leftBuffer = (i == 0 ? bufferWidth : bufferWidth / 2);
                int rightBuffer = (i == tanks.Length - 1 ? bufferWidth : bufferWidth / 2);

                int tankPositionX = i * fieldPartitionWidth + leftBuffer + Randomizer.Next(0, fieldPartitionWidth - leftBuffer - rightBuffer);
                int tankPlatformHeight = Math.Min(HeightMap[tankPositionX], HeightMap[tankPositionX + (int)tanks[i].Size.X - 1]);

                tanks[i].Position = new Vector2(
                    tankPositionX,
                    Size.Y - tanks[i].Size.Y - tankPlatformHeight);

                for (int x = 0; x < tanks[i].Size.X; x++)
                {
                    HeightMap[tankPositionX + x] = tankPlatformHeight;
                }

                tanks[i].BarrelAngleInDegrees = (i >= tanks.Length / 2 ? 180 : 0);
                tanks[i].Color = GraphicsUtility.ChooseRandomColor(Randomizer, Constants.Terrain.PlayerColorRandomness);
            }

            GenerateTexture();
        }

        public override void HandleCollision(ScorchGame game, Collision collision)
        {
            if (collision.CollisionObjectPhysicsType == Physics.PhysicsType.Explosion)
            {
                var explosion = (Explosion)collision.CollisionObject;
                explosion.Scale *= Constants.Physics.ExplosionScorchRadiusFactor;
                game.Terrain.AffectTerrainWithDrawable(explosion, TerrainEffect.Scorch);
                explosion.Scale /= Constants.Physics.ExplosionScorchRadiusFactor;
                game.Terrain.AffectTerrainWithDrawable(explosion, TerrainEffect.Destroy);
            }
        }

        private void AffectTerrainWithDrawable(Scorch.Graphics.IDrawable drawable, TerrainEffect effect)
        {
            var drawableScaledSize = drawable.Size * drawable.Scale;
            AffectTerrainInCircle(drawable.Position, drawableScaledSize.X / 2f, effect);
        }

        private void AffectTerrainInCircle(Vector2 centerPosition, float radius, TerrainEffect effect)
        {
            var affectedArea = new Rectangle(
                (int)(centerPosition.X - radius),
                (int)(centerPosition.Y - radius),
                (int)(2f * radius),
                (int)(2f * radius));
            var terrainRectangle = new Rectangle(
                0,
                0,
                Texture.Width,
                Texture.Height);
            var intersectionRectangle = Rectangle.Intersect(affectedArea, terrainRectangle);

            var terrainTextureData = GraphicsUtility.GetTextureSample(Texture, intersectionRectangle);

            for (int i = 0; i < intersectionRectangle.Width; i++)
            {
                int x = intersectionRectangle.Left + i;
                for (int j = 0; j < intersectionRectangle.Height; j++)
                {
                    int y = intersectionRectangle.Top + j;
                    float distanceFromCenter = Vector2.Distance(centerPosition, new Vector2(x, y));
                    if (distanceFromCenter <= radius)
                    {
                        var currentColor = terrainTextureData[i + j * intersectionRectangle.Width];
                        var newColor = currentColor;

                        if (effect == TerrainEffect.Scorch)
                        {
                            newColor = currentColor != Color.Transparent ?
                                GraphicsUtility.Blacken(currentColor, Constants.Terrain.ScorchBlackness)
                                : currentColor;
                        }
                        else if (effect == TerrainEffect.Destroy)
                        {
                            newColor = Color.Transparent;
                        }

                        terrainTextureData[i + j * intersectionRectangle.Width] = newColor;
                    }
                }
            }

            GraphicsUtility.SetTextureSample(Texture, intersectionRectangle, terrainTextureData);

            if (effect == TerrainEffect.Destroy)
            {
                CleanUpTerrain(new Rectangle(
                    intersectionRectangle.Left,
                    0,
                    intersectionRectangle.Width,
                    intersectionRectangle.Bottom));
            }
        }

        private void CleanUpTerrain(Rectangle affectedArea)
        {
            var terrainTextureData = GraphicsUtility.GetTextureSample(Texture, affectedArea);

            for (int x = 0; x < affectedArea.Width; x++)
            {
                int terrainReconstructionY = affectedArea.Height - 1;
                var terrainHeight = HeightMap[x + affectedArea.Left];
                for (int y = affectedArea.Height - 1; y >= Texture.Height - terrainHeight; y--)
                {
                    var currentIndex = x + y * affectedArea.Width;
                    var currentColor = terrainTextureData[currentIndex];
                    if (currentColor != Color.Transparent)
                    {
                        terrainTextureData[currentIndex] = Color.Transparent;
                        terrainTextureData[x + terrainReconstructionY-- * affectedArea.Width] = currentColor;
                    }
                }

                HeightMap[x + affectedArea.Left] = Math.Min(terrainHeight, Texture.Height - terrainReconstructionY);
            }

            GraphicsUtility.SetTextureSample(Texture, affectedArea, terrainTextureData);
        }

        private void GenerateTexture()
        {
            var color = GraphicsUtility.ChooseRandomColor(Randomizer, Constants.Terrain.TerrainColorRandomness);
            var topColor = GraphicsUtility.Whiten(color, Constants.Terrain.TopColorWhiteness);
            var bottomColor = GraphicsUtility.Blacken(color, Constants.Terrain.BottomColorBlackness);
            var speckleColor = GraphicsUtility.Blacken(color, Constants.Terrain.SpeckleColorBlackness);

            Color[] textureData = new Color[(int)Size.X * (int)Size.Y];
            for (int x = 0; x < Size.X; x++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    if (HeightMap[x] > Size.Y - y)
                    {
                        textureData[x + y * (int)Size.X] = Randomizer.NextDouble() < Constants.Terrain.SpeckleChance ?
                            GraphicsUtility.RandomizeColor(
                                Randomizer,
                                speckleColor,
                                Constants.Terrain.SpeckleColorRandomization) // terrain always has dark spots
                            : 
                            GraphicsUtility.RandomizedFade(
                                Randomizer,
                                topColor,
                                bottomColor,
                                y / Size.Y,
                                Constants.Terrain.GradientRandomization);
                    }
                    else
                    {
                        textureData[x + y * (int)Size.X] = Color.Transparent;
                    }
                }
            }

            Texture.SetData(textureData);
        }

        private void GenerateHeightMap()
        {
            int mountainCount = Randomizer.Next(8, 16);
            int[][] mountains = new int[mountainCount][];
            for (int i = 0; i < mountainCount; i++)
            {
                int xQuadrant = i % 4;
                int mountainPeakX = xQuadrant * (int)Size.X / 4 + Randomizer.Next(0, (int)Size.X / 4);
                mountains[i] = GenerateMountainHeightMap(Randomizer, (int)Size.X, (int)Size.Y, mountainPeakX);
            }

            int minHeight = (int)Size.Y / 10;
            int maxHeight = (int)Size.Y;
            int midHeight = (minHeight + maxHeight) / 3;

            int[] heightMap = new int[(int)Size.X];
            for (int x = 0; x < Size.X; x++)
            {
                heightMap[x] = midHeight;

                for (int i = 0; i < mountains.Length; i++)
                {
                    heightMap[x] += mountains[i][x];
                }

                heightMap[x] = MathHelper.Clamp(heightMap[x], minHeight, maxHeight);
            }

            HeightMap = heightMap;
        }

        private static int[] GenerateMountainHeightMap(Random randomizer, int width, int height, int mountainPeakX)
        {
            int mountainPeakHeight = randomizer.Next(height / 24, height / 8);
            int mountainWidth = randomizer.Next(width / 16, width / 2);
            bool valley = randomizer.Next(0, 2) == 0;

            int[] heightMap = new int[width];

            for (int x = 0; x < width; x++ )
            {
                int mountainLeftX = mountainPeakX - mountainWidth / 2;
                double xInRadians = Math.PI * (x - mountainLeftX) / mountainWidth;

                if (x >= mountainPeakX - mountainWidth / 2 && x < mountainPeakX + mountainWidth / 2)
                {
                    heightMap[x] = (int)(Math.Sin(xInRadians) * mountainPeakHeight) * (valley ? -1 : 1);
                }
            }

            return heightMap;
        }
    }

    public enum TerrainEffect
    {
        Destroy,
        Scorch
    }
}
