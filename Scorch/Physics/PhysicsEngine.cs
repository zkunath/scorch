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
        private Dictionary<string, Tank> Tanks;
        private Terrain Terrain;
        private event ScorchGame.GameEventHandler Settled;
        private ScorchGame Game;
        private TimeSpan SettledTime;

        public PhysicsEngine(ScorchGame game)
        {
            Game = game;
            Terrain = game.Terrain;
            PhysicsObjects = new Dictionary<string, IPhysicsObject>();
            Tanks = new Dictionary<string, Tank>();
        }

        public void AddPhysicsObject(IPhysicsObject physicsObject)
        {
            this.PhysicsObjects.Add(physicsObject.Id, physicsObject);

            if (physicsObject.PhysicsType == PhysicsType.Tank)
            {
                Tanks.Add(physicsObject.Id, (Tank)physicsObject);
            }
        }

        public void RemovePhysicsObject(string id)
        {
            var physicsObject = PhysicsObjects[id];
            this.PhysicsObjects.Remove(id);

            if (physicsObject.PhysicsType == PhysicsType.Tank)
            {
                Tanks.Remove(physicsObject.Id);
            }
        }

        public void Update(GameTime gameTime)
        {
            float elapsedSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var removeObjectIds = new HashSet<string>();
            var addFieldObjects = new List<FieldObject>();
            var collisions = new HashSet<Collision>();

            foreach (var physicsObject in PhysicsObjects.Values.Where(o => o.Visible))
            {
                if (physicsObject.TimeToLive.HasValue)
                {
                    if (physicsObject.TimeToLive <= TimeSpan.Zero)
                    {
                        removeObjectIds.Add(physicsObject.Id);
                        continue;
                    }

                    physicsObject.TimeToLive -= gameTime.ElapsedGameTime;
                }

                Vector2 velocityDeltaFromGravity = CanFall(physicsObject, Terrain) ?
                    new Vector2(0, Constants.Physics.GravityAcceleration) * elapsedSeconds :
                    Vector2.Zero;
                physicsObject.Velocity += velocityDeltaFromGravity;

                var previousPosition = physicsObject.Position;
                var nextPosition = physicsObject.Position += physicsObject.Velocity * elapsedSeconds;
                physicsObject.Position = nextPosition;

                if (physicsObject.Position.X >= Terrain.Size.X || 
                    physicsObject.Position.X < 0)
                {
                    collisions.Add(new Collision(physicsObject, PhysicsType.FieldBounds));
                }
                
                if (CollidesWithTerrain(physicsObject, previousPosition, Terrain))
                {
                    collisions.Add(new Collision(physicsObject, Terrain));
                }

                if (physicsObject.PhysicsType == PhysicsType.Projectile)
                {
                    foreach (var tank in Tanks.Values.Where(t => t.Visible))
                    {
                        if (CollidesWithFootprint(physicsObject, previousPosition, tank))
                        {
                            collisions.Add(new Collision(physicsObject, tank));
                        }
                    }
                }
                else if (physicsObject.PhysicsType == PhysicsType.Explosion)
                {
                    var explosion = (Explosion)physicsObject;
                    if (!explosion.IsCollisionChecked)
                    {
                        float tankDistanceFromExplosionCenter = float.MaxValue;
                        foreach (var tank in Tanks.Values.Where(t => t.Visible))
                        {
                            var vulnerablePositions = new List<Vector2>();
                            vulnerablePositions.Add(tank.BarrelOriginPosition);
                            vulnerablePositions.Add(tank.BarrelEndPosition);
                            vulnerablePositions.Add(new Vector2(tank.Footprint.Left, tank.Footprint.Top));
                            vulnerablePositions.Add(new Vector2(tank.Footprint.Left, tank.Footprint.Bottom));
                            vulnerablePositions.Add(new Vector2(tank.Footprint.Right, tank.Footprint.Top));
                            vulnerablePositions.Add(new Vector2(tank.Footprint.Right, tank.Footprint.Bottom));
                            tankDistanceFromExplosionCenter = vulnerablePositions.Min(p => Vector2.Distance(p, explosion.Position));

                            if (tankDistanceFromExplosionCenter < explosion.Radius)
                            {
                                var collision = new Collision(physicsObject, tank) { Distance = tankDistanceFromExplosionCenter };
                                collisions.Add(collision);
                            }
                        }
                        
                        explosion.IsCollisionChecked = true;
                    }
                }
            }

            foreach (var collision in collisions)
            {
                collision.PhysicsObject.HandleCollision(Game, collision);

                if (collision.CollisionObject != null)
                {
                    collision.CollisionObject.HandleCollision(
                        Game,
                        new Collision(collision.CollisionObject, collision.PhysicsObject)
                        {
                            Distance = collision.Distance
                        });
                }
            }

            RemoveObjectsById(removeObjectIds);

            foreach (var fieldObject in addFieldObjects)
            {
                Game.GraphicsEngine.AddDrawableObject(fieldObject);
                AddPhysicsObject(fieldObject);
            }

            if (Settled != null && IsSettled(gameTime))
            {
                foreach (var tank in Tanks.Values.Where(t => t.Visible))
                {
                    if (tank.Dead)
                    {
                        tank.Die(Game);
                        SettledTime = TimeSpan.Zero;
                    }
                }

                if (IsSettled(gameTime))
                {
                    Settled(Game);
                    Settled = null;
                }
            }
        }

        public void StopFallingObjectOnTerrain(IPhysicsObject physicsObject, Terrain terrain)
        {
            var maxTerrainHeightUnderFootprint = MaxTerrainHeightUnderFootprint(physicsObject, terrain);
            float positionAdjustmentY = terrain.Size.Y - maxTerrainHeightUnderFootprint - physicsObject.Footprint.Bottom;
            physicsObject.Velocity = Vector2.Zero;
            physicsObject.Position += new Vector2(0, positionAdjustmentY);
        }

        public void TrackSettledEvent()
        {
            Settled += ScorchGame.PhysicsSettled;
            SettledTime = TimeSpan.Zero;
        }

        private bool IsSettled(GameTime gameTime)
        {
            bool atLeastOneObjectIsNotSettled = PhysicsObjects.Values.Any(o => o.Velocity != Vector2.Zero || o.TimeToLive.HasValue);
            
            if (atLeastOneObjectIsNotSettled)
            {
                SettledTime = TimeSpan.Zero;
                return false;
            }
            else
            {
                SettledTime += gameTime.ElapsedGameTime;
                return SettledTime > TimeSpan.FromMilliseconds(Constants.Physics.SettledThresholdInMilliseconds);
            }
        }

        private void RemoveObjectsById(HashSet<string> removeObjectIds)
        {
            foreach (var objectId in removeObjectIds)
            {
                Game.GraphicsEngine.RemoveDrawableObject(objectId);
                RemovePhysicsObject(objectId);
            }
        }

        private static bool CollidesWithFootprint(IPhysicsObject physicsObject, Vector2 previousPosition, IPhysicsObject collisionObject)
        {
            return InterpolatePositionUntil(
                physicsObject,
                previousPosition,
                physicsObject.Position,
                p => collisionObject.Footprint.Contains(p.Position));
        }

        private static bool CollidesWithTerrain(IPhysicsObject physicsObject, Vector2 previousPosition, Terrain terrain)
        {
            if (physicsObject.PhysicsType != PhysicsType.Explosion &&
                physicsObject.Velocity == Vector2.Zero)
            {
                return false;
            }

            if (physicsObject.PhysicsType == PhysicsType.Tank)
            {
                return FootprintCollidesWithTerrain(physicsObject, terrain);
            }
            else if (physicsObject.PhysicsType == PhysicsType.Projectile)
            {
                var nextPosition = physicsObject.Position;
                return InterpolatePositionUntil(
                    physicsObject,
                    previousPosition,
                    physicsObject.Position,
                    p => terrain.IsTerrainLocatedAtPosition(p.Position) || p.Position.Y > terrain.Size.Y);
            }
            else if (physicsObject.PhysicsType == PhysicsType.Explosion)
            {
                var explosion = (Explosion)physicsObject;
                if (!explosion.IsCollisionChecked)
                {
                    // it's probably true, so skip the check
                    // return FootprintCollidesWithTerrain(explosion, terrain);
                    return true;
                }
            }

            return false;
        }

        private static bool InterpolatePositionUntil(
            IPhysicsObject physicsObject,
            Vector2 previousPosition,
            Vector2 nextPosition,
            Func<IPhysicsObject, bool> predicate)
        {
            var distance = Vector2.Distance(previousPosition, nextPosition);
            var totalPositionDelta = nextPosition - previousPosition;
            float numSteps = (float)Math.Ceiling(distance / Constants.Physics.CollisionDetectionMinDistancePerStep);
            for (float i = 1; i <= numSteps; i++)
            {
                physicsObject.Position = previousPosition + totalPositionDelta * i / numSteps;

                if (predicate(physicsObject))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool FootprintCollidesWithTerrain(IPhysicsObject physicsObject, Terrain terrain)
        {
            int maxTerrainHeightUnderFootprint = MaxTerrainHeightUnderFootprint(physicsObject, terrain);
            return terrain.Size.Y - maxTerrainHeightUnderFootprint <= physicsObject.Footprint.Bottom;
        }

        private static bool CanFall(IPhysicsObject physicsObject, Terrain terrain)
        {
            bool canFall = false;
            if (physicsObject.PhysicalProperties.HasFlag(PhysicalProperties.AffectedByGravity))
            {
                canFall = true;

                if (physicsObject.PhysicsType == PhysicsType.Tank)
                {
                    var tank = (Tank)physicsObject;
                    if (tank.Footprint.Bottom >= terrain.Size.Y)
                    {
                        canFall = false;
                    }
                    else
                    {
                        canFall = !FootprintCollidesWithTerrain(tank, terrain);
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
                if (x >= 0 && x < terrain.Size.X)
                {
                    maxTerrainHeightUnderFootprint = Math.Max(maxTerrainHeightUnderFootprint, terrain.HeightMap[x]);
                }
            }

            return maxTerrainHeightUnderFootprint;
        }
    }

    [Flags]
    public enum PhysicalProperties
    {
        None = 0,
        AffectedByGravity = 1
    }

    public enum PhysicsType
    {
        None,
        FieldBounds,
        Terrain,
        Tank,
        Projectile,
        Explosion
    }
}
