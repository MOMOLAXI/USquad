namespace UniverseEngine
{
    /// <summary>
    /// 系统组件, 声明周期被托管在GameSystem
    /// </summary>
    public abstract class SystemComponent : ISystem
    {
        internal virtual int Priority { get; } = 0;

        int ISystem.Priority => Priority;

        void ISystem.Init()
        {
            OnInit();
        }

        void ISystem.Update(float dt)
        {
            OnUpdate(dt);
        }

        void ISystem.FixedUpdate(float dt)
        {
            OnFixedUpdate(dt);
        }

        void ISystem.LateUpdate(float dt)
        {
            OnLateUpdate(dt);
        }

        void ISystem.Reset()
        {
            OnReset();
        }

        void ISystem.Destroy()
        {
            OnDestroy();
        }

        void ISystem.ApplicationFocus(bool hasFocus)
        {
            OnApplicationFocus(hasFocus);
        }

        void ISystem.ApplicationPause(bool pauseStatus)
        {
            OnApplicationPause(pauseStatus);
        }

        void ISystem.ApplicationQuit()
        {
            OnApplicationQuit();
        }

        protected virtual void OnInit()
        {

        }

        protected virtual void OnUpdate(float dt)
        {

        }

        protected virtual void OnFixedUpdate(float dt)
        {

        }

        protected virtual void OnLateUpdate(float dt)
        {

        }

        protected virtual void OnReset()
        {

        }

        protected virtual void OnDestroy()
        {

        }

        protected virtual void OnApplicationFocus(bool hasFocus)
        {

        }

        protected virtual void OnApplicationPause(bool pauseStatus)
        {

        }

        protected virtual void OnApplicationQuit()
        {

        }
    }
}
