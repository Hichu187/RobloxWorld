namespace Hichu
{
    public interface IStateMachine
    {
        void Init();
        void OnStart();
        void OnUpdate();
        void OnFixUpdate();
        void OnStop();
    }
}