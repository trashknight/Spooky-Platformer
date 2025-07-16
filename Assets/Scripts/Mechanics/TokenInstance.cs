using UnityEngine;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This class contains the data required for implementing token collection mechanics.
    /// It does not perform animation of the token, this is handled in a batch by the 
    /// TokenController in the scene.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TokenInstance : MonoBehaviour
    {
        public AudioClip tokenCollectAudio;

        [Tooltip("If true, animation will start at a random position in the sequence.")]
        public bool randomAnimationStartTime = false;

        [Tooltip("List of frames that make up the animation.")]
        public Sprite[] idleAnimation, collectedAnimation;

        internal Sprite[] sprites = new Sprite[0];
        internal SpriteRenderer _renderer;

        // Unique index which is assigned by the TokenController in a scene.
        internal int tokenIndex = -1;
        internal TokenController controller;

        // Active frame in animation, updated by the controller.
        internal int frame = 0;
        internal bool collected = false;

        public GameManager gameManager;

        void Awake()
        {
            gameManager = FindObjectOfType<GameManager>();
            _renderer = GetComponent<SpriteRenderer>();
            sprites = idleAnimation;

            if (randomAnimationStartTime)
                frame = Random.Range(0, sprites.Length);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                OnPlayerEnter(player);
            }
        }

        void OnPlayerEnter(PlayerController player)
        {
            if (collected) return;

            frame = 0;
            sprites = collectedAnimation;
            collected = true;

            if (gameManager != null)
                gameManager.unsavedScore += 1;

            if (tokenCollectAudio != null)
                AudioSource.PlayClipAtPoint(tokenCollectAudio, transform.position);

            Destroy(gameObject);
        }
    }
}