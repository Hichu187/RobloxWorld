using UnityEngine;
using Hichu;

namespace Game
{
    public class Event_BrainrotEvo_UnequipPet : IEvent
    {
        public BrainrotEvoPetConfig petData;

        public Event_BrainrotEvo_UnequipPet(BrainrotEvoPetConfig petData)
        {
            this.petData = petData;
        }
    }
}
