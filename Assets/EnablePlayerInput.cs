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
        public override void Execute()
        {
            var model = Simulation.GetModel<PlatformerModel>();
            var player = model.player;

            if (player == null)
            {
                Debug.LogWarning("EnablePlayerInput: model.player is null.");
                return;
            }

            player.controlEnabled = true;
            player.animator.SetBool("Spawning", false);

            Combat combat = player.GetComponent<Combat>();
            if (combat != null)
            {
                combat.attackEnabled = true;
            }
        }
    }
}