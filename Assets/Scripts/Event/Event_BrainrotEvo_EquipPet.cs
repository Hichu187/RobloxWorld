using Hichu;
using UnityEngine;

namespace Game
{
    public class Event_BrainrotEvo_EquipPet : IEvent
    {
        public BrainrotEvoPetConfig petData;

        public Event_BrainrotEvo_EquipPet(BrainrotEvoPetConfig petData)
        {
            this.petData = petData;
        }
    }
}
