using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player health reaches 0. This usually results in a PlayerDeath event.
    /// </summary>
    public class HealthIsZero : Simulation.Event<HealthIsZero>
    {
        public Health health;

        public override void Execute()
        {
            Debug.Log("HealthIsZero.Execute() called. Scheduling PlayerDeath event...");
            Schedule<PlayerDeath>();
        }
    }
}