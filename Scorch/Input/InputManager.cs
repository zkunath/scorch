using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Scorch.Input
{
    public class InputManager
    {
        private float minGestureDeltaLength = float.MaxValue;
        private float maxGestureDeltaLength = 0;

        public Dictionary<int, TouchInput> TouchInputs { get; private set; }
        public Dictionary<int, TouchInput> EmptyTouchInputs { get; private set; }
        public List<TouchGesture> TouchGestures { get; private set; }
        
        public InputManager()
        {
            TouchInputs = new Dictionary<int, TouchInput>();
            EmptyTouchInputs = new Dictionary<int, TouchInput>();
            TouchGestures = new List<TouchGesture>();
        }

        public void Update()
        {
            var touchCollection = TouchPanel.GetState();

            var newTouchInputs = new Dictionary<int, TouchLocation>();
            foreach (var touchLocation in touchCollection)
            {
                newTouchInputs.Add(touchLocation.Id, touchLocation);
            }

            var touchIds = new HashSet<int>();
            touchIds.UnionWith(TouchInputs.Keys);
            touchIds.UnionWith(newTouchInputs.Keys);
            foreach (var id in touchIds)
            {
                if (TouchInputs.ContainsKey(id) && newTouchInputs.ContainsKey(id))
                {
                    TouchInputs[id].UpdateState(newTouchInputs[id]);
                }
                else if (!TouchInputs.ContainsKey(id))
                {
                    TouchInputs.Add(id, new TouchInput(newTouchInputs[id]));
                }
                else
                {
                    TouchInputs.Remove(id);
                }
            }

            while (TouchPanel.IsGestureAvailable)
            {
                var touchGesture = new TouchGesture(TouchPanel.ReadGesture(), TouchInputs);
                TouchGestures.Add(touchGesture);

                var deltaLength = touchGesture.Gesture.Delta.Length();
                minGestureDeltaLength = Math.Min(minGestureDeltaLength, deltaLength);
                maxGestureDeltaLength = Math.Max(maxGestureDeltaLength, deltaLength);
            }
        }
    }
}
