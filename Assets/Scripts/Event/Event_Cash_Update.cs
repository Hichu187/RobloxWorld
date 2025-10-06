using Hichu;
using UnityEngine;

namespace Game
{
    public class Event_Cash_Update : IEvent
    {
        public int total;

        public Event_Cash_Update(int total)
        {
            this.total = total;
        }
    }
}
