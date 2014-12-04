using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Scorch.Graphics;
using Scorch.Physics;
using System;
using System.Collections.Generic;

namespace Scorch.DataModels
{
    public class Tank : FieldObject
    {
        private GraphicsDevice GraphicsDevice;
        private Texture2D TankTexture;
        private Texture2D BarrelTexture;
        private Color BaseColor;
        private Color _Color;
        public Color Color 
        {
            get
            {
                return _Color;
            }
            set
            {
                BaseColor = _Color = value;
                Colorize(value);
            }
        }

        private int _BarrelAngleInDegrees;
        public int BarrelAngleInDegrees
        {
            get
            {
                return _BarrelAngleInDegrees;
            }
            set
            {
                _BarrelAngleInDegrees = MathHelper.Clamp(
                    value,
                    Constants.HUD.MinAngleInDegrees,
                    Constants.HUD.MaxAngleInDegrees);
                ChildObjects["barrel"].RotationInRadians = BarrelAngleInRadians;
            }
        }
        public float BarrelAngleInRadians
        {
            get
            {
                return MathHelper.ToRadians(-BarrelAngleInDegrees);
            }
        }
        public Vector2 BarrelOriginPosition
        {
            get
            {
                return Position + ChildObjects["barrel"].Position;
            }
        }

        public Vector2 BarrelEndPosition
        {
            get
            {
                var barrel = ChildObjects["barrel"];
                return BarrelOriginPosition +
                    barrel.Size.X * barrel.Scale.X * new Vector2(
                        (float)Math.Cos(BarrelAngleInRadians),
                        (float)Math.Sin(BarrelAngleInRadians));
            }
        }

        private int _Power;
        public int Power
        {
            get
            {
                return _Power;
            }
            set
            {
                _Power = MathHelper.Clamp(
                    value,
                    Constants.HUD.MinPower,
                    Constants.HUD.MaxPower);
            }
        }

        private int _Health;
        public int Health
        { 
            get
            {
                return _Health;
            }
            set
            {
                _Health = MathHelper.Clamp(value, 0, 100);
            }
        }

        public bool Dead
        {
            get
            {
                return Health == 0f;
            }
        }

        public Tank(
            string name,
            GraphicsDevice graphicsDevice,
            Dictionary<string, Texture2D> textureAssets,
            Color color)
            : base(
                PhysicsType.Tank,
                "player_" + name,
                textureAssets["Tank"],
                Vector2.Zero
            )
        {
            GraphicsDevice = graphicsDevice;
            Depth = Constants.Graphics.DrawOrder.TankMiddle;
            TankTexture = textureAssets["Tank"];
            BarrelTexture = textureAssets["TankBarrel"];
            var barrelPosition = new Vector2(16f, 4.5f);

            var barrel = new FieldObject(
                PhysicsType.Tank,
                "barrel",
                BarrelTexture,
                barrelPosition);

            barrel.Origin = new Vector2(1f, 3.5f);
            barrel.Depth = Constants.Graphics.DrawOrder.TankBack;
            barrel.RotationInRadians = BarrelAngleInRadians;
            ChildObjects.Add(barrel.Id, barrel);

            PhysicalProperties |= Physics.PhysicalProperties.AffectedByGravity;
        }

        public void SetAngleAndPowerByAimVector(Vector2 aim, float minLength, float maxLength)
        {
            if (aim.Y > 0)
            {
                var normalizedAim = new Vector2(aim.X, aim.Y);
                normalizedAim.Normalize();
                BarrelAngleInDegrees = (int)MathHelper.ToDegrees((float)Math.Acos(-normalizedAim.X));
            }
            else
            {
                BarrelAngleInDegrees = (aim.X > 0 ? 180 : 0);
            }

            float aimLength = Math.Min(aim.Length(), maxLength);
            aimLength = (aimLength <= minLength ? 0 : aimLength);
            Power = (int)(aimLength / maxLength * 100);
        }

        public void Reset()
        {
            // initial angle is dependent on Tank position and set during Terrain generation
            Power = Constants.HUD.InitialPower;
            Visible = true;
            Health = Constants.Physics.PlayerMaxHealth;
        }

        public void Die(ScorchGame game)
        {
            Health = 0;
            Visible = false;
            game.HUD.InfoText += string.Format("\n{0} died", Id);

            var explosion = new Explosion(
                Id + "_explosion",
                game.TextureAssets,
                Position,
                Constants.Physics.ExplosionBaseRadius * Constants.Physics.PlayerDeathExplosionFactor,
                Constants.Physics.ExplosionCenterDamage);

            game.PhysicsEngine.AddPhysicsObject(explosion);
            game.GraphicsEngine.AddDrawableObject(explosion);
        }

        public override void HandleCollision(ScorchGame game, Collision collision)
        {
            int damage = 0;
            string damageText = string.Empty;

            if (collision.CollisionObjectPhysicsType == PhysicsType.Terrain)
            {
                float fallingSpeed = Velocity.Length();
                damage = (int)Math.Round(fallingSpeed / Constants.Physics.TerrainCollisionSpeedToDamageRatio);
                game.PhysicsEngine.StopFallingObjectOnTerrain(this, (Terrain)collision.CollisionObject);
                damageText = string.Format(
                    "{0} hit the terrain moving {1} px/s: {2} damage",
                    Id,
                    Math.Round(fallingSpeed),
                    damage);
            }
            else if (collision.CollisionObjectPhysicsType == PhysicsType.Projectile)
            {
                float projectileSpeed = collision.CollisionObject.Velocity.Length();
                damage = (int)Math.Round(projectileSpeed / Constants.Physics.ProjectileCollisionSpeedToDamageRatio);
                damageText = string.Format(
                    "{0} was hit by a projectile moving {1} px/s: {2} damage",
                    Id,
                    Math.Round(projectileSpeed),
                    damage);
            }
            else if (collision.CollisionObjectPhysicsType == Physics.PhysicsType.Explosion)
            {
                var explosion = (Explosion)collision.CollisionObject;
                float distanceFromExplosionCenter = collision.Distance;
                float damageFactor = 1f - distanceFromExplosionCenter / explosion.Radius;
                damage = (int)Math.Round(damageFactor * explosion.Damage);
                if (damage > 0)
                {
                    damageText = string.Format(
                        "{0} was hit by an explosion {1} px away: {2} damage",
                        Id,
                        Math.Round(distanceFromExplosionCenter),
                        damage);
                }
            }

            ApplyDamage(damage);

            if (damageText != string.Empty)
            {
                game.HUD.InfoText += "\n" + damageText;
            }
        }

        private void ApplyDamage(int damage)
        {
            Health -= damage;
            Colorize(GraphicsUtility.Blacken(Color, 1f - 1f * Health / Constants.Physics.PlayerMaxHealth));
        }

        private void Colorize(Color color)
        {
            _Color = color;
            Texture = GraphicsUtility.ColorizeTexture(GraphicsDevice, TankTexture, color, Constants.Graphics.TankColorizeAmount);
            ChildObjects["barrel"].Texture = GraphicsUtility.ColorizeTexture(GraphicsDevice, BarrelTexture, color, Constants.Graphics.TankColorizeAmount);
        }
    }
}
