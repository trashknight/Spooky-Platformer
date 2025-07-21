using Platformer.Core;
using Platformer.Mechanics;
using UnityEngine;

namespace Platformer.Gameplay
{
    public class PlayerLanded : Simulation.Event<PlayerLanded>
    {
        public PlayerController player;

        public override void Execute()
        {
            if (player == null)
                player = GameObject.FindObjectOfType<PlayerController>();

            if (player != null)
            {
                Debug.Log("PlayerLanded event triggered.");

                player.LandedVFX();

                if (player.audioSource && player.landedAudio)
                {
                    player.audioSource.PlayOneShot(player.landedAudio, 0.5f);
                    Debug.Log("Played proper landedAudio!");
                }
                else
                {
                    Debug.LogWarning("Missing audioSource or landedAudio in PlayerController.");
                }
            }
            else
            {
                Debug.LogWarning("PlayerLanded: player is null.");
            }
        }
    }
}