using Hichu;
using UnityEngine;

namespace Game
{
    public class Event_Player_Add_Exp : IEvent
    {
        public int exp;

        public Event_Player_Add_Exp(int exp)
        {
            this.exp = exp;
        }
    }
}
