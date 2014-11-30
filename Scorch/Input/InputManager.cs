using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
using System.Linq;

namespace Scorch.Input
{
    public class InputManager
    {
        public Dictionary<int, TouchInput> TouchInputs { get; private set; }
        public Dictionary<int, TouchInput> EmptyTouchInputs { get; private set; }
        public Queue<GestureSample> GestureSamples { get; private set; }
        
        public InputManager()
        {
            TouchInputs = new Dictionary<int, TouchInput>();
            EmptyTouchInputs = new Dictionary<int, TouchInput>();
            GestureSamples = new Queue<GestureSample>();
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

            if (TouchPanel.IsGestureAvailable)
            {
                GestureSamples.Enqueue(TouchPanel.ReadGesture());
            }
        }
    }
}
