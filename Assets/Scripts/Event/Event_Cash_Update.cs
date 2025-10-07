using Hichu;
using UnityEngine;

namespace Game
{
    public class Event_Cash_Update : IEvent
    {
        public int total;

        public bool encreaseCash = false;

        public Event_Cash_Update(int total, bool encreaseCash)
        {
            this.total = total;
            this.encreaseCash = encreaseCash;
        }
    }
}
