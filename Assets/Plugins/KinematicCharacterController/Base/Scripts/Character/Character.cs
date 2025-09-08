using UnityEngine;

namespace Kcc.Base
{
    public class Character : MonoBehaviour
    {
        public CharacterController cControl;
        public CharacterCamera cCamera;
        public CharacterCombat cCombat;
        public CharacterInteract cInteract;
        public FieldOfView fov;
    }
}
