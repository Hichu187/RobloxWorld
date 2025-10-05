using Hichu;

namespace Game
{
    public class Player : MonoSingleton<Player>
    {
        public PlayerControl control;
        public PlayerGUI gui;
        public Character character;

    }
}