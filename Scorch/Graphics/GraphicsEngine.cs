using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Scorch.Graphics
{
    public class GraphicsEngine
    {
        private GraphicsDevice GraphicsDevice;
        private Dictionary<string, IDrawable> DrawableObjects;

        public Vector2 CameraSize { get; private set; }
        public Vector2 FieldSize { get; private set; }

        public GraphicsEngine(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            CameraSize = new Vector2(graphicsDevice.Viewport.TitleSafeArea.Width, graphicsDevice.Viewport.TitleSafeArea.Height);
            FieldSize = CameraSize * 1f;
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
            // TODO: remove this debug option
            const bool showPlayerFootprint = true;

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

                if (showPlayerFootprint && drawable.Id.StartsWith("player"))
                {
                    game.SpriteBatch.Draw(
                        game.TextureAssets["Green"],
                        drawRectangle: ((Scorch.Physics.IPhysicsObject)drawable).Footprint,
                        depth: Scorch.DataModels.DrawOrder.HudTop);
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
