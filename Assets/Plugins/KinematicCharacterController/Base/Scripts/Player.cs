using UnityEngine;
using UnityEngine.TextCore.Text;
using Hichu;

namespace Kcc.Base
{
    public class Player : MonoSingleton<Player>
    {
        public PlayerControl control;
        public CharacterController cControl;
        public CharacterCamera cCamera;
        public PlayerGUI gui;
        public FieldOfView fov;

    }
}