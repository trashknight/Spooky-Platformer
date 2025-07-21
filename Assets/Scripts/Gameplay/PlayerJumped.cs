using Platformer.Core;
using Platformer.Mechanics;

namespace Platformer.Gameplay
{
    /// <summary>
    /// Fired when the player performs a Jump.
    /// </summary>
    public class PlayerJumped : Simulation.Event<PlayerJumped>
    {
        public PlayerController player;

        public override void Execute()
        {
            // Jump sound is now handled in PlayerController to avoid duplicate playback.
            // if (player.IsGrounded && player.audioSource && player.jumpAudio)
            // {
            //     player.audioSource.PlayOneShot(player.jumpAudio);
            // }
        }
    }
}