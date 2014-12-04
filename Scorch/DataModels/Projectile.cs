using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scorch.Physics;
using System;
using System.Collections.Generic;

namespace Scorch.DataModels
{
    public class Projectile : FieldObject
    {
        public Projectile(
            string id,
            Dictionary<string, Texture2D> TextureAssets,
            Vector2 position,
            Vector2 velocity)
            : base(
                PhysicsType.Projectile,
                id,
                TextureAssets["Circle"],
                position)
        {
            Origin = Size / 2f;
            Scale = Vector2.One * Constants.Graphics.ProjectileScale;
            Velocity = velocity;
            PhysicalProperties |= PhysicalProperties.AffectedByGravity;
        }

        public override void HandleCollision(ScorchGame game, Collision collision)
        {
            if (collision.CollisionObjectPhysicsType == PhysicsType.FieldBounds)
            {
                TimeToLive = new TimeSpan(0);
            }
            else if (collision.CollisionObjectPhysicsType == PhysicsType.Terrain ||
                     collision.CollisionObjectPhysicsType == PhysicsType.Tank)
            {
                TimeToLive = new TimeSpan(0);

                var explosion = new Explosion(
                    Id + "_explosion",
                    game.TextureAssets,
                    Position,
                    Constants.Physics.ExplosionBaseRadius,
                    Constants.Physics.ExplosionCenterDamage);

                game.PhysicsEngine.AddPhysicsObject(explosion);
                game.GraphicsEngine.AddDrawableObject(explosion);
            }
        }
    }
}
