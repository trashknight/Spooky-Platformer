using Platformer.Core;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    /// <summary>
    /// This event is fired when user input should be enabled.
    /// </summary>
    public class EnablePlayerInput : Simulation.Event<EnablePlayerInput>
    {
        PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        Combat combat = GameObject.FindGameObjectWithTag("Player").GetComponent<Combat>();

        public override void Execute() 
        {
            var player = model.player;
            player.controlEnabled = true;
            combat.attackEnabled = true;
        }
    }
}