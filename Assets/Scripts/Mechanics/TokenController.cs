using UnityEngine;

namespace Platformer.Mechanics
{
    public class TokenController : MonoBehaviour
    {
        public float frameRate = 12;
        public TokenInstance[] tokens;

        float nextFrameTime = 0;

        [ContextMenu("Find All Tokens")]
        void FindAllTokensInScene()
        {
            tokens = UnityEngine.Object.FindObjectsOfType<TokenInstance>();
        }

        void Awake()
        {
            if (tokens.Length == 0)
                FindAllTokensInScene();

            GameManager gameManager = FindObjectOfType<GameManager>();

            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];

                // ✅ DO NOT auto-assign tokenIndex. Use manually assigned value in Inspector.
                token.controller = this;

                // ✅ Destroy tokens collected before last checkpoint
                if (gameManager != null && gameManager.ShouldDestroyToken(token.tokenIndex))
                {
                    Destroy(token.gameObject);
                    tokens[i] = null; // Prevent animation of destroyed token
                }
            }
        }

        void Update()
        {
            if (Time.time - nextFrameTime > (1f / frameRate))
            {
                for (var i = 0; i < tokens.Length; i++)
                {
                    var token = tokens[i];
                    if (token != null)
                    {
                        token._renderer.sprite = token.sprites[token.frame];
                        if (token.collected && token.frame == token.sprites.Length - 1)
                        {
                            token.gameObject.SetActive(false);
                            tokens[i] = null;
                        }
                        else
                        {
                            token.frame = (token.frame + 1) % token.sprites.Length;
                        }
                    }
                }

                nextFrameTime += 1f / frameRate;
            }
        }
    }
}