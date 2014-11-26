namespace Scorch.Physics
{
    public class Collision
    {
        public IPhysicsObject PhysicsObject { get; private set; }
        public IPhysicsObject CollisionObject { get; private set; }
        public PhysicsType CollisionObjectPhysicsType { get; private set; }

        public Collision(IPhysicsObject physicsObject, IPhysicsObject collisionObject)
        {
            PhysicsObject = physicsObject;
            CollisionObject = collisionObject;
            CollisionObjectPhysicsType = collisionObject.PhysicsType;
        }

        public Collision(IPhysicsObject physicsObject, PhysicsType collisionObjectPhysicsType)
        {
            PhysicsObject = physicsObject;
            CollisionObjectPhysicsType = collisionObjectPhysicsType;
        }
    }
}
