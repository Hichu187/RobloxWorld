namespace Game
{
    public interface ICharacterCollidable
    {
        void OnCollisionEnter(CharacterControl character);
        void OnCollisionExit(CharacterControl character);
        void OnTriggerEnter(CharacterControl character);
        void OnTriggerExit(CharacterControl character);
    }
}

