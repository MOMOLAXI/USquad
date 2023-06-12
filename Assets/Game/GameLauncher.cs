using System.Collections.Generic;
using UnityEngine;
using UniverseEngine;

namespace USquad
{
    public class GameLauncher : GlobalBehavior
    {
        // [SerializeField]
        // GameInitializeArguments m_Arguments = new();

        protected override string Name => nameof(GameLauncher);

        protected override void OnAwake()
        {
            // Engine.Initialize(m_Arguments);
            // Engine.GetSystem<LaunchSystem>().Launch(m_Arguments).Forget();
        }
    }
}
