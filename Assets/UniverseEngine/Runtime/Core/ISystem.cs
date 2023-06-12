namespace UniverseEngine
{
    public interface ISystem
    {
        internal int Priority => 0;
        internal void Init();
        internal void Update(float dt);
        internal void FixedUpdate(float dt);
        internal void LateUpdate(float dt);
        internal void Reset();
        internal void Destroy();
        internal void ApplicationFocus(bool hasFocus);
        internal void ApplicationPause(bool pauseStatus);
        internal void ApplicationQuit();
    }
}
