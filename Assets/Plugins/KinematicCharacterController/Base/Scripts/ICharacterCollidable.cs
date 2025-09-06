namespace Kcc.Base
{
    public interface ICharacterCollidable 
    {
        void OnCollisionEnter(CharacterController character);
        void OnCollisionExit(CharacterController character);
        void OnTriggerEnter(CharacterController character);
        void OnTriggerExit(CharacterController character);
    }
}
