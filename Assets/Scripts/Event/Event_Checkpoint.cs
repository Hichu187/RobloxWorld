using Hichu;
using UnityEngine;

namespace Game
{
    public class Event_Checkpoint : IEvent
    {
        public PlatformCheckpoint checkpoint { get; private set; }
        public CharacterControl character { get; private set; }

        public Event_Checkpoint(PlatformCheckpoint checkpoint, CharacterControl character)
        {
            this.checkpoint = checkpoint;
            this.character = character;
        }
    }
}
