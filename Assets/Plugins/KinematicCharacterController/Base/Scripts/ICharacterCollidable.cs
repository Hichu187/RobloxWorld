namespace Kcc.Base
{
    public interface ICharacterCollidable 
    {
        void OnCollisionEnter(Character character);
        void OnCollisionExit(Character character);
        void OnTriggerEnter(Character character);
        void OnTriggerExit(Character character);
    }
}
