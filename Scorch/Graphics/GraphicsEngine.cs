using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Scorch.Graphics
{
    public class GraphicsEngine
    {
        private GraphicsDevice GraphicsDevice;
        private SpriteBatch SpriteBatch;
        private Dictionary<string, IDrawable> FieldObjects;

        public Vector2 CameraSize { get; private set; }
        public Vector2 FieldSize { get; private set; }

        public GraphicsEngine(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
            SpriteBatch = new SpriteBatch(graphicsDevice);
            CameraSize = new Vector2(graphicsDevice.Viewport.TitleSafeArea.Width, graphicsDevice.Viewport.TitleSafeArea.Height);
            FieldSize = CameraSize * 1f;
            FieldObjects = new Dictionary<string, IDrawable>();
        }

        public void AddDrawableToField(IDrawable drawable)
        {
            this.FieldObjects.Add(drawable.Id, drawable);
        }

        public void DrawField()
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            SpriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            foreach (var drawable in FieldObjects.Values.Where(d => d.Visible))
            {
                var drawablePosition = drawable.Position;

                SpriteBatch.Draw(
                    drawable.Texture,
                    position: drawablePosition,
                    scale: drawable.Scale,
                    origin: drawable.Origin,
                    rotation: drawable.RotationInRadians,
                    depth: drawable.Depth);

                foreach (var child in drawable.Children.Where(c => c.Visible))
                {
                    SpriteBatch.Draw(
                        child.Texture,
                        position: drawablePosition + child.Position,
                        scale: child.Scale,
                        origin: child.Origin,
                        rotation: child.RotationInRadians,
                        depth: child.Depth);
                }
            }

            SpriteBatch.End();
        }
    }
}
