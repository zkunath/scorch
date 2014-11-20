using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Scorch.Graphics
{
    public interface IDrawable
    {
        string Id { get; }
        Texture2D Texture { get; }
        Vector2 Position { get; }
        Vector2 Size { get; }
        Vector2 Scale { get; }
        Vector2 Origin { get; }
        float RotationInRadians { get; }
        bool Visible { get; }
        float Depth { get; }
        IEnumerable<Scorch.Graphics.IDrawable> Children { get; }
    }
}
