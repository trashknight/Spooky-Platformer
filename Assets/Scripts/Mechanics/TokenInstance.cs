using UnityEngine;

namespace Platformer.Mechanics
{
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

        public int tokenIndex = -1;
        internal TokenController controller;

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

            Debug.Log($"[TokenInstance] Awake(): tokenIndex = {tokenIndex}, GameManager present: {gameManager != null}");
        }

        void Start()
        {
            Debug.Log($"[TokenInstance] Start(): tokenIndex = {tokenIndex}");

            if (gameManager != null)
            {
                bool shouldDestroy = gameManager.ShouldDestroyToken(tokenIndex);
                Debug.Log($"[TokenInstance] Checking if should destroy token {tokenIndex}: {shouldDestroy}");

                if (shouldDestroy)
                {
                    Debug.Log($"[TokenInstance] Destroying token {tokenIndex} on load.");
                    Destroy(gameObject);
                }
            }
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
            {
                gameManager.unsavedScore += 1;
                gameManager.collectedThisRun.Add(tokenIndex);
                Debug.Log($"[TokenInstance] Token {tokenIndex} collected and added to collectedThisRun.");
            }

            if (tokenCollectAudio != null)
                AudioSource.PlayClipAtPoint(tokenCollectAudio, transform.position);

            Destroy(gameObject);
        }
    }
}