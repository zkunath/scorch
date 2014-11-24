using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
using System.Diagnostics;

namespace Scorch.Input
{
    public class TouchInput
    {
        public int Id { get; private set; }
        public bool LatestIsHandled { get; set; }
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
            Previous = latest;
            Latest = CopyTouchLocation(latest);
            LatestIsHandled = false;
        }

        private static TouchLocation CopyTouchLocation(TouchLocation touchLocation)
        {
            return new TouchLocation(touchLocation.Id, touchLocation.State, touchLocation.Position);
        }
    }
}
