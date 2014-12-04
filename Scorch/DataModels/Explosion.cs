using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scorch.Physics;
using System;
using System.Collections.Generic;

namespace Scorch.DataModels
{
    public class Explosion : FieldObject
    {
        public int Damage { get; set; }
        public float Radius { get; set; }
        public bool IsCollisionChecked { get; set; }

        public Explosion(
            string id,
            Dictionary<string, Texture2D> TextureAssets,
            Vector2 position,
            float radius,
            int damage)
            : base(
                PhysicsType.Explosion,
                id,
                TextureAssets["Circle"],
                position)
        {
            Origin = Size / 2f;
            Scale = Vector2.One * (radius * 2f / Size.X);
            Depth = Constants.Graphics.DrawOrder.TankFront;
            TimeToLive = TimeSpan.FromMilliseconds(Constants.Physics.ExplosionDurationInMilliseconds);

            Radius = radius;
            Damage = damage;
            IsCollisionChecked = false;
        }
    }
}
