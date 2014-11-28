using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scorch;
using Scorch.DataModels;
using System.Collections.Generic;
using System.Linq;

namespace Scorch.Graphics
{
    public class GraphicsEngine
    {
        private GraphicsDevice GraphicsDevice;
        private Dictionary<string, IDrawable> DrawableObjects;

        public Vector2 ViewportSize { get; private set; }
        public Vector2 FieldSize { get; private set; }

        public GraphicsEngine(GraphicsDevice graphicsDevice, Vector2 viewportSize, Vector2 fieldSize)
        {
            GraphicsDevice = graphicsDevice;
            ViewportSize = viewportSize;
            FieldSize = fieldSize;
            DrawableObjects = new Dictionary<string, IDrawable>();
        }

        public void AddDrawableObject(IDrawable drawable)
        {
            this.DrawableObjects.Add(drawable.Id, drawable);
        }

        public void RemoveDrawableObject(string id)
        {
            this.DrawableObjects.Remove(id);
        }

        public void DrawField(ScorchGame game)
        {
            foreach (var drawable in DrawableObjects.Values.Where(d => d.Visible))
            {
                var drawablePosition = drawable.Position;

                game.SpriteBatch.Draw(
                    drawable.Texture,
                    position: drawablePosition,
                    scale: drawable.Scale,
                    origin: drawable.Origin,
                    rotation: drawable.RotationInRadians,
                    depth: drawable.Depth);

                if (Constants.Debug.DrawFootprints && (drawable is Tank || drawable is Projectile))
                {
                    game.SpriteBatch.Draw(
                        game.TextureAssets["Green"],
                        drawRectangle: ((Scorch.Physics.IPhysicsObject)drawable).Footprint,
                        depth: Constants.Graphics.DrawOrder.HudTop);
                }

                if (Constants.Debug.DrawTerrainHeightMap && (drawable is Terrain))
                {
                    for (int x = 0; x < game.Terrain.HeightMap.Length; x++)
                    {
                        game.SpriteBatch.Draw(
                            game.TextureAssets["Green"],
                            position: new Vector2(x, game.Terrain.Size.Y - game.Terrain.HeightMap[x]),
                            depth: Constants.Graphics.DrawOrder.HudTop);
                    }
                }

                foreach (var child in drawable.Children.Where(c => c.Visible))
                {
                    game.SpriteBatch.Draw(
                        child.Texture,
                        position: drawablePosition + child.Position,
                        scale: child.Scale,
                        origin: child.Origin,
                        rotation: child.RotationInRadians,
                        depth: child.Depth);
                }
            }
        }
    }
}
