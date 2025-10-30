using Hichu;
using Sirenix.OdinInspector;

namespace Game
{
    public class Player : MonoSingleton<Player>
    {
        public PlayerControl control;
        public PlayerGUI gui;
        public Character character;

        [Button]
        public void SpeedUp()
        {
            character.cControl.JumpSpeedMultiple = 1f;
            character.cControl.MoveSpeedMultiple = 1.5f;
        }
        [Button]
        public void JumpUp()
        {
            character.cControl.JumpSpeedMultiple = 1.5f;
            character.cControl.MoveSpeedMultiple = 1f;
        }
    }
}