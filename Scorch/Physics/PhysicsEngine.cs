using Microsoft.Xna.Framework;
using Scorch;
using Scorch.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scorch.Physics
{
    public class PhysicsEngine
    {
        private Dictionary<string, IPhysicsObject> PhysicsObjects;
        private Terrain Terrain;

        public PhysicsEngine(Terrain terrain)
        {
            Terrain = terrain;
            PhysicsObjects = new Dictionary<string, IPhysicsObject>();
        }

        public void AddPhysicsObject(IPhysicsObject physicsObject)
        {
            this.PhysicsObjects.Add(physicsObject.Id, physicsObject);
        }

        public void RemovePhysicsObject(string id)
        {
            this.PhysicsObjects.Remove(id);
        }

        public void Update(ScorchGame game, GameTime gameTime)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var collisionObjectIds = new HashSet<string>();
            var removeObjectIds = new HashSet<string>();
            var addFieldObjects = new List<FieldObject>();

            foreach (var physicsObject in PhysicsObjects.Values)
            {
                Vector2 velocityDeltaFromGravity = CanFall(physicsObject, Terrain) ?
                    new Vector2(0, Constants.Physics.GravityAcceleration) * elapsedSeconds :
                    Vector2.Zero;
                physicsObject.Velocity += velocityDeltaFromGravity;

                var previousPosition = physicsObject.Position;
                var nextPosition = physicsObject.Position += physicsObject.Velocity * elapsedSeconds;
                physicsObject.Position = nextPosition;

                if (physicsObject.TimeToLive.HasValue)
                {
                    physicsObject.TimeToLive -= gameTime.ElapsedGameTime;
                }

                if ((physicsObject.TimeToLive.HasValue && physicsObject.TimeToLive < TimeSpan.Zero) ||
                    physicsObject.Position.X >= Terrain.Size.X || 
                    physicsObject.Position.X < 0)
                {
                    removeObjectIds.Add(physicsObject.Id);
                }
                
                if (physicsObject.PhysicsType.HasFlag(PhysicsType.CollidesWithTerrain) &&
                    CollidesWithTerrain(physicsObject, previousPosition, Terrain))
                {
                    collisionObjectIds.Add(physicsObject.Id);
                }
            }

            foreach (var objectId in collisionObjectIds)
            {
                var physicsObject = PhysicsObjects[objectId];
                if (physicsObject.PhysicsType.HasFlag(PhysicsType.OnCollisionExplode))
                {
                    removeObjectIds.Add(physicsObject.Id);
                    var explosion = new FieldObject(
                        physicsObject.Id + "_explosion",
                        game.TextureAssets["SpikyCircle"],
                        physicsObject.Position);
                    explosion.Origin = explosion.Size / 2f;
                    explosion.TimeToLive = TimeSpan.FromMilliseconds(Constants.Physics.ExplosionDurationInMilliseconds);
                    explosion.Depth = Constants.Graphics.DrawOrder.TankFront;
                    explosion.Scale = Vector2.One * 1.05f;
                    Terrain.AffectTerrainWithDrawable(explosion, TerrainEffect.Scorch);
                    explosion.Scale = Vector2.One;
                    Terrain.AffectTerrainWithDrawable(explosion, TerrainEffect.Destroy);
                    addFieldObjects.Add(explosion);
                }
                else if (physicsObject.PhysicsType.HasFlag(PhysicsType.OnCollisionStop))
                {
                    var maxTerrainHeightUnderFootprint = MaxTerrainHeightUnderFootprint(physicsObject, Terrain);
                    float positionAdjustmentY = Terrain.Size.Y - maxTerrainHeightUnderFootprint - physicsObject.Footprint.Bottom;
                    // TODO: player damage based on impact velocity
                    physicsObject.Velocity = Vector2.Zero;
                    physicsObject.Position += new Vector2(0, positionAdjustmentY);
                }
            }

            foreach (var objectId in removeObjectIds)
            {
                game.GraphicsEngine.RemoveDrawableObject(objectId);
                RemovePhysicsObject(objectId);
            }

            foreach (var fieldObject in addFieldObjects)
            {
                game.GraphicsEngine.AddDrawableObject(fieldObject);
                AddPhysicsObject(fieldObject);
            }
        }

        private static bool CollidesWithTerrain(IPhysicsObject physicsObject, Vector2 previousPosition, Terrain terrain)
        {
            if (physicsObject.Velocity == Vector2.Zero)
            {
                return false;
            }

            if (physicsObject.PhysicsType.HasFlag(PhysicsType.OnCollisionStop) &&
                FootprintCollidesWithTerrain(physicsObject, terrain))
            {
                return true;
            }
            
            var nextPosition = physicsObject.Position;
            var distance = Vector2.Distance(previousPosition, nextPosition);
            var totalPositionDelta = nextPosition - previousPosition;
            float numSteps = (float)Math.Ceiling(distance / Constants.Physics.CollisionDetectionMinDistancePerStep);
            for (float i = 1; i <= numSteps; i++)
            {
                physicsObject.Position = previousPosition + totalPositionDelta * i / numSteps;
                if (terrain.IsTerrainLocatedAtPosition(physicsObject.Position) ||
                    physicsObject.Position.Y > terrain.Size.Y)
                {
                    // object stops at interpolated collision position
                    return true;
                }
            }

            return false;
        }

        private static bool CanFall(IPhysicsObject physicsObject, Terrain terrain)
        {
            bool canFall = false;
            if (physicsObject.PhysicsType.HasFlag(PhysicsType.AffectedByGravity))
            {
                canFall = true;

                if (physicsObject.PhysicsType.HasFlag(PhysicsType.CollidesWithTerrain) &&
                    physicsObject.PhysicsType.HasFlag(PhysicsType.OnCollisionStop))
                {
                    if (physicsObject.Footprint.Bottom >= terrain.Size.Y)
                    {
                        canFall = false;
                    }
                    else
                    {
                        canFall = !FootprintCollidesWithTerrain(physicsObject, terrain);
                    }
                }
            }

            return canFall;
        }

        private static int MaxTerrainHeightUnderFootprint(IPhysicsObject physicsObject, Terrain terrain)
        {
            int maxTerrainHeightUnderFootprint = 0;
            for (int i = 0; i < physicsObject.Footprint.Width; i++)
            {
                var x = physicsObject.Footprint.Left + i;
                if (x >=0 && x < terrain.Size.X)
                {
                    maxTerrainHeightUnderFootprint = Math.Max(maxTerrainHeightUnderFootprint, terrain.HeightMap[x]);
                }
            }

            return maxTerrainHeightUnderFootprint;
        }

        private static bool FootprintCollidesWithTerrain(IPhysicsObject physicsObject, Terrain terrain)
        {
            int maxTerrainHeightUnderFootprint = MaxTerrainHeightUnderFootprint(physicsObject, terrain);
            return terrain.Size.Y - maxTerrainHeightUnderFootprint <= physicsObject.Footprint.Bottom;
        }
    }
}
