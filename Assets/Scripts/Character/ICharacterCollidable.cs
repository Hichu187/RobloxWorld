using Kcc.Base;
namespace Game
{
    public interface ICharacterCollidable
    {
        void OnCollisionEnter(Character character);
        void OnTriggerEnter(Character character);
        void OnTriggerExit(Character character);
    }
}
