using Hichu;
using UnityEngine;

namespace Game
{
    public class Event_AI_Dead : IEvent
    {
        public AI ai;

        public Event_AI_Dead(AI ai)
        {
            this.ai = ai;
        }
    }
}
