using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scorch;
using Scorch.Physics;
using System;
using System.Collections.Generic;

namespace Scorch.DataModels
{
    public class FieldObject : Scorch.Graphics.IDrawable, Scorch.Physics.IPhysicsObject
    {
        public string Id { get; set; }
        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public Vector2 Scale { get; set; }
        public Vector2 Origin { get; set; }
        public float RotationInRadians { get; set; }
        public bool Visible  { get; set; }
        public float Depth { get; set; }
        public Vector2 Velocity { get; set; }
        public TimeSpan? TimeToLive { get; set; }
        public PhysicsType PhysicsType { get; set; }
        public Rectangle Footprint
        {
            get 
            {
                var scaledSize = Size * Scale;
                return new Rectangle(
                    (int)(Position.X - Origin.X),
                    (int)(Position.Y - Origin.Y),
                    (int)scaledSize.X,
                    (int)scaledSize.Y);
            }
        }

        public Dictionary<string, FieldObject> ChildObjects { get; private set; }
        public IEnumerable<Scorch.Graphics.IDrawable> Children
        {
            get
            {
                return ChildObjects.Values;
            }
        }

        public FieldObject(
            string id,
            Texture2D texture,
            Vector2 position)
        {
            Id = id;
            Texture = texture;
            Position = position;
            Velocity = Vector2.Zero;
            Size = new Vector2(texture.Width, texture.Height);
            Scale = Vector2.One;
            Origin = Vector2.Zero;
            RotationInRadians = 0f;
            Visible = true;
            Depth = Constants.Graphics.DrawOrder.TankMiddle;
            TimeToLive = null;
            ChildObjects = new Dictionary<string, FieldObject>();
        }
    }
}
