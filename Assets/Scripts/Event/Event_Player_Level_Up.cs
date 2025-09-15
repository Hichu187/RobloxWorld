using Hichu;
using UnityEngine;

namespace Game
{
    public class Event_Player_Level_Up : IEvent
    {
        public int level;

        public Event_Player_Level_Up(int level)
        {
            this.level = level;
        }
    }
}
