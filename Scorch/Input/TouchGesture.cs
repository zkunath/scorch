using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
using System.Linq;

namespace Scorch.Input
{
    public class TouchGesture
    {
        public int Id { get; private set; }
        public bool IsHandled { get; set; }
        public GestureSample Gesture { get; private set; }

        public TouchGesture(GestureSample gesture, Dictionary<int, TouchInput> touchInputs)
        {
            if (touchInputs.Count > 0)
            {
                // attempt to match gesture to the touchInput which caused it
                if (gesture.GestureType == GestureType.Flick)
                {
                    Id = touchInputs.Values.FirstOrDefault(t => t.Latest.State == TouchLocationState.Released).Id;
                }
                else if (gesture.Position != Vector2.Zero)
                {
                    // Id = touchInputs.Values.OrderByDescending(
                    //     t => Vector2.Distance(gesture.Position, t.Latest.Position)).First().Id;

                    Id = touchInputs.Values.Aggregate(
                        (minT, t) =>
                            (minT == null || t.GetDistanceFrom(gesture) < minT.GetDistanceFrom(gesture) ? t : minT)
                    ).Id;
                }
            }

            Gesture = gesture;
        }
    }
}
