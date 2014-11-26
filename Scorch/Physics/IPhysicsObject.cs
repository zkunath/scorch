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
        PhysicalProperties PhysicalProperties { get; }
        PhysicsType PhysicsType { get; }
        Vector2 Position { get; set; }
        Vector2 Velocity { get; set; }
        Rectangle Footprint { get; }
        TimeSpan? TimeToLive { get; set; }
        void HandleCollision(ScorchGame game, Collision collision);
    }
}
