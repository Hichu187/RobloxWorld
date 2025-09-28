using Cysharp.Threading.Tasks;
using Hichu;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Game
{
    public class BrainrotEvoGate : MonoBehaviour, ICharacterCollidable
    {
        [SerializeField] TextMeshPro _text;
        [SerializeField] int levelOpen = 0;

        private void Start()
        {
            _text.text = $"Escape _name_ \n Level {levelOpen}";
        }
        void ICharacterCollidable.OnCollisionEnter(CharacterControl character)
        {
;
        }

        void ICharacterCollidable.OnCollisionExit(CharacterControl character)
        {

        }

        async void ICharacterCollidable.OnTriggerEnter(CharacterControl character)
        {
            if (!character.GetComponent<Character>().isPlayer) return;
            
            if(DataBrainrotEvo.level >= levelOpen)
            {
                DataBrainrotEvo.MoveNextMap();
                character.Motor.enabled = false;

                await UniTask.WaitForEndOfFrame();

                StaticBus<Event_BrainrotEvo_Change_Space>.Post(null);

            }
            else
            {
                LDebug.Log<BrainrotEvoGate>("Not Enough Level");
            }
        }

        void ICharacterCollidable.OnTriggerExit(CharacterControl character)
        {

        }
    }
}
