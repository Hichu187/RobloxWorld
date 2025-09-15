using Hichu;
using UnityEngine;

namespace Game
{
    public class Event_Player_Add_Exp : IEvent
    {
        public float exp;

        public Event_Player_Add_Exp(float exp)
        {
            this.exp = exp;
        }
    }
}
