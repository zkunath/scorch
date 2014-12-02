using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System.Diagnostics;

namespace Scorch.Input
{
    public class TouchInput
    {
        public int Id { get; private set; }
        public bool IsHandled { get; set; }
        public TouchLocation Latest { get; private set; }
        public TouchLocation Previous { get; private set; }
        public TouchLocation Origin { get; private set; }
             
        public TouchInput(TouchLocation origin)
        {
            Id = origin.Id;
            Debug.Assert(Id != 0, "TouchInput.Id == 0");
            Latest = CopyTouchLocation(origin);
            Previous = CopyTouchLocation(origin);
            Origin = CopyTouchLocation(origin);
        }

        public void UpdateState(TouchLocation latest)
        {
            Previous = Latest;
            Latest = CopyTouchLocation(latest);
            IsHandled = false;
        }

        public float GetDistanceFrom(GestureSample gesture)
        {
            return Vector2.Distance(gesture.Position, Latest.Position);
        }

        private static TouchLocation CopyTouchLocation(TouchLocation touchLocation)
        {
            return new TouchLocation(touchLocation.Id, touchLocation.State, touchLocation.Position);
        }
    }
}
