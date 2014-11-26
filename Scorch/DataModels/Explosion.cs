using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scorch.Physics;
using System;
using System.Collections.Generic;

namespace Scorch.DataModels
{
    public class Explosion : FieldObject
    {
        public float Damage { get; set; }

        public Explosion(
            string id,
            Dictionary<string, Texture2D> TextureAssets,
            Vector2 position,
            float radius,
            float damage)
            : base(
                PhysicsType.Projectile,
                id,
                TextureAssets["SpikyCircle"],
                position)
        {
            Origin = Size / 2f;
            Scale = Vector2.One * (radius * 2f / Size.X);
            Depth = Constants.Graphics.DrawOrder.TankFront;
            TimeToLive = TimeSpan.FromMilliseconds(Constants.Physics.ExplosionDurationInMilliseconds);

            Damage = damage;
        }
    }
}
