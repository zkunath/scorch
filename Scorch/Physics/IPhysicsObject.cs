using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scorch.Physics
{
    public interface IPhysicsObject
    {
        string Id { get; }
        Vector2 Position { get; set; }
        Vector2 Velocity { get; set; }
        Rectangle Footprint { get; }
        TimeSpan? TimeToLive { get; set; }
        PhysicsType PhysicsType { get; set; }
    }

    [Flags]
    public enum PhysicsType
    {
        None = 0,
        AffectedByGravity = 1,
        CollidesWithTerrain = 2,
        OnCollisionExplode = 4,
        OnCollisionStop = 8
    }
}
