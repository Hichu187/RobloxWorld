using Hichu;
using Kcc.Base;
using UnityEngine;

namespace Game
{
    public class Event_Checkpoint : IEvent
    {
        public PlatformCheckpoint checkpoint { get; private set; }
        public Character character { get; private set; }

        public Event_Checkpoint(PlatformCheckpoint checkpoint, Character character)
        {
            this.checkpoint = checkpoint;
            this.character = character;
        }
    }
}
