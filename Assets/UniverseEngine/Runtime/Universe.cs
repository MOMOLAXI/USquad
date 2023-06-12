using UnityEngine;

namespace UniverseEngine
{
    public class Universe : MonoBehaviour
    {
        void Update()
        {
            UniverseEngine.Update();
        }

        void FixedUpdate()
        {
            UniverseEngine.FixedUpdate();
        }

        void LateUpdate()
        {
            UniverseEngine.LateUpdate();
        }

        void Reset()
        {
            UniverseEngine.Reset();
        }

        void OnDestroy()
        {
            UniverseEngine.Destroy();
        }

        void OnApplicationFocus(bool hasFocus)
        {
            UniverseEngine.ApplicationFocus(hasFocus);
        }

        void OnApplicationPause(bool pauseStatus)
        {
            UniverseEngine.ApplicationPause(pauseStatus);
        }

        void OnApplicationQuit()
        {
            UniverseEngine.ApplicationQuit();
        }
    }
}
