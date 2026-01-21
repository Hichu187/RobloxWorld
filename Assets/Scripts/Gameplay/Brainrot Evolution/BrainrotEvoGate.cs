using Cysharp.Threading.Tasks;
using Easypapa;
using Hichu;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Game
{
    public class BrainrotEvoGate : MonoBehaviour, ICharacterCollidable
    {
        [SerializeField] string mapName;
        [SerializeField] TextMeshPro _text;
        [SerializeField] int levelOpen = 0;
        

        private void Start()
        {
            _text.text = $"{mapName} \n Level {levelOpen}";
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
            
            if(DataBrainrotEvo.level >= levelOpen - 1)
            {
                DataBrainrotEvo.MoveNextMap();
                character.Motor.enabled = false;

                await UniTask.WaitForEndOfFrame();

                StaticBus<Event_BrainrotEvo_Change_Space>.Post(null);

                Easypapa.EasypapaAdSdk.LogEvent($"unlock_map_{levelOpen}", "map", levelOpen);

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
