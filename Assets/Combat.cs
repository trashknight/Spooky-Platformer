using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Needed for Image component
using Platformer.Mechanics; // Required for PlayerController

public class Combat : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip biteAudio;

    public Transform attackPointR;
    public Transform attackPointL;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    public int attackDamage = 1;

    public float attackRate = 2f;
    float nextattackTime = 0f;
    public bool attackEnabled = true;
    public bool facingRight = true;

    GameObject blackoutSquare;
    PlayerController player;

    // Flag to ensure initial fade only plays once per new game start
    private bool hasPlayedInitialFade = false;

    void Awake()
    {
        blackoutSquare = GameObject.FindGameObjectWithTag("Blackout Square");
        player = GetComponent<PlayerController>();

        // Ensure blackoutSquare is present and initially transparent/inactive.
        if (blackoutSquare != null)
        {
            Image blackoutImage = blackoutSquare.GetComponent<Image>();
            if (blackoutImage != null)
            {
                blackoutImage.color = new Color(blackoutImage.color.r, blackoutImage.color.g, blackoutImage.color.b, 0); // Start fully transparent
                blackoutSquare.SetActive(false); // Initially inactive, only activate when fading
            }
            else
            {
                Debug.LogError("Combat.Awake(): Blackout Square does not have an Image component. Fading will not work.");
            }
        }
        else
        {
            Debug.LogError("Combat.Awake(): Blackout Square GameObject not found with tag 'Blackout Square'! Fading will not work.");
        }
    }

    private void Start()
    {
        // Initial fade sequence is now called explicitly AFTER 'Play' button is clicked from MainUIController
        // or after scene reload if GameManager.showMenu is false (e.g., player death, which also reloads scene)
        // This 'Start' method is intentionally left clean as the fade is externally triggered.
    }

    // Public method to trigger the initial fade specifically after 'Play' is clicked from the menu
    public void TriggerInitialGameFade()
    {
        if (!hasPlayedInitialFade && blackoutSquare != null)
        {
            Debug.Log("Combat: TriggerInitialGameFade() called. Starting initial fade sequence.");
            StartCoroutine(InitialFadeSequence());
            hasPlayedInitialFade = true;
        }
        else if (hasPlayedInitialFade)
        {
            Debug.Log("Combat: Initial game fade already played. Skipping.");
        }
        else
        {
            Debug.LogError("Combat: TriggerInitialGameFade: blackoutSquare is null. Cannot perform initial fade.");
        }
    }


    void Update() // No longer 'protected override'
    {
        if (player != null && !player.controlEnabled)
            return;

        if (Time.time >= nextattackTime)
        {
            if (Input.GetKeyDown(KeyCode.Space) && attackEnabled)
            {
                if (animator != null)
                {
                    animator.SetTrigger("Sexy Attack 69");
                }
                else
                {
                    Debug.LogWarning("Combat: Animator not assigned.");
                }

                nextattackTime = Time.time + 1f / attackRate;
            }
        }
    }

    public void Attack()
    {
        if (!attackEnabled || (player != null && !player.controlEnabled)) return;

        Debug.Log("Calling attack");

        if (audioSource != null && biteAudio != null)
        {
            audioSource.PlayOneShot(biteAudio);
        }
        else
        {
            Debug.LogWarning("Combat: Missing AudioSource or Bite Audio.");
        }

        Collider2D[] hitEnemies;

        if (facingRight && attackPointR != null)
        {
            hitEnemies = Physics2D.OverlapCircleAll(attackPointR.position, attackRange, enemyLayers);
        }
        else if (!facingRight && attackPointL != null)
        {
            hitEnemies = Physics2D.OverlapCircleAll(attackPointL.position, attackRange, enemyLayers); // Fixed typo here
        }
        else
        {
            Debug.LogWarning("Combat: Missing attack point.");
            hitEnemies = new Collider2D[0];
        }

        foreach (Collider2D enemy in hitEnemies)
        {
            PumpkinScript pumpkin = enemy.GetComponent<PumpkinScript>();
            if (pumpkin)
            {
                int spawnId = pumpkin.spawnId;
                foreach (var pumpkineer in FindObjectsOfType<ShootProjectile>())
                {
                    if (pumpkineer.SpawnID == spawnId)
                    {
                        pumpkineer.RespawnPumpkin();
                    }
                }
            }

            Enemy e = enemy.GetComponent<Enemy>();
            if (e != null)
            {
                e.TakeDamage(attackDamage);
            }
            else
            {
                SpiderHealth spider = enemy.GetComponent<SpiderHealth>();
                if (spider != null)
                {
                    spider.TakeDamage(attackDamage);
                }
                else
                {
                    DestructibleProjectile projectile = enemy.GetComponent<DestructibleProjectile>();
                    if (projectile != null)
                    {
                        projectile.DestroyProjectile();
                    }
                }
            }
        }
    }

    public void EnableAttack()
    {
        attackEnabled = true;
    }

    public void DisableAttack()
    {
        attackEnabled = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPointR != null)
        {
            Gizmos.DrawWireSphere(attackPointR.position, attackRange);
        }
        if (attackPointL != null)
        {
            Gizmos.DrawWireSphere(attackPointL.position, attackRange);
        }
    }

    // MODIFIED: This method now *only* starts the fade TO black and returns control to caller.
    public IEnumerator ReloadSceneCoroutine(float time) // Changed to IEnumerator to yield
    {
        Debug.Log($"Combat.ReloadSceneCoroutine() called (now just initiates fade). blackoutSquare is {(blackoutSquare != null ? "NOT NULL" : "NULL")}.");
        if (blackoutSquare != null)
        {
            Image blackoutImage = blackoutSquare.GetComponent<Image>();
            if (blackoutImage != null)
            {
                blackoutImage.color = new Color(blackoutImage.color.r, blackoutImage.color.g, blackoutImage.color.b, 0); // Start TRANSPARENT
            }
            blackoutSquare.SetActive(true); // Activate to start fade
            yield return StartCoroutine(FadeBlackoutSquare(4)); // Yield until fade to black is complete
        }
        else
        {
            Debug.LogError("Combat: Cannot start fade. BlackoutSquare is null. Reloading must be handled by caller.");
        }
    }

    // NEW: Added a specific method to initiate fade TO black for external callers (like GameResetter or PlayerDeath).
    // This now calls the Coroutine version of ReloadScene
    public IEnumerator FadeToBlack(float duration) // Added this for clarity, is now an IEnumerator
    {
        Debug.Log($"Combat: FadeToBlack called, initiating blackout. Duration: {duration}");
        yield return StartCoroutine(ReloadSceneCoroutine(duration)); // Yield until the blackout fade is complete.
    }

    // In Combat.cs

    // NEW: Public method to trigger a fade to black and then scene reload (for non-coroutines)
    public void InitiateFadeAndReload(float duration)
    {
        Debug.Log($"Combat: InitiateFadeAndReload called. Starting fade-to-black then reload. Duration: {duration}");
        StartCoroutine(FadeToBlack(duration)); // Start the fade coroutine
        // Note: The SceneManager.LoadScene will happen *after* the fade completes inside FadeToBlack (via GameResetter's logic for consistency).
        // For PlayerDeath/DeathZone, the fade will complete, then the scene reloads.
    }

    // Handles the fade-in-from-black cinematic after "Play" is clicked.
    IEnumerator InitialFadeSequence()
    {
        if (blackoutSquare == null)
        {
            Debug.LogError("InitialFadeSequence: blackoutSquare is null. Cannot perform initial fade.");
            yield break;
        }

        Image blackoutImage = blackoutSquare.GetComponent<Image>();
        if (blackoutImage == null)
        {
            Debug.LogError("InitialFadeSequence: No Image component found on blackoutSquare. Cannot perform initial fade.");
            yield break;
        }

        // Ensure blackoutSquare is active and instantly black
        blackoutImage.color = new Color(blackoutImage.color.r, blackoutImage.color.g, blackoutImage.color.b, 1);
        blackoutSquare.SetActive(true);
        Debug.Log("InitialFadeSequence: BlackoutSquare set to opaque. Starting fade out.");

        // Now, fade out from black to reveal the game (fadeSpeed < 0 for fade-out)
        yield return StartCoroutine(FadeBlackoutSquare(-4));

        Debug.Log("InitialFadeSequence: Fade out complete. Game visible.");
    }


    IEnumerator FadeBlackoutSquare(float fadeSpeed = 4)
    {
        if (blackoutSquare == null)
        {
            Debug.LogError("FadeBlackoutSquare: BlackoutSquare is null. Cannot fade.");
            yield break;
        }

        Image blackoutImage = blackoutSquare.GetComponent<Image>();
        if (blackoutImage == null)
        {
            Debug.LogError("FadeBlackoutSquare: No Image component found on blackoutSquare.");
            yield break;
        }

        Color objectColor = blackoutImage.color;
        float fadeAmount;
        bool go = true;

        // Ensure correct starting alpha based on fade direction
        if (fadeSpeed > 0 && objectColor.a < 1) // Fading IN to black
        {
            objectColor.a = 0; // Start fully transparent
            blackoutImage.color = objectColor;
        }
        else if (fadeSpeed < 0 && objectColor.a > 0) // Fading OUT from black
        {
            objectColor.a = 1; // Start fully opaque
            blackoutImage.color = objectColor;
        }

        while (go)
        {
            // Use Time.unscaledDeltaTime for fade to ensure it works even if Time.timeScale is 0
            fadeAmount = objectColor.a + (fadeSpeed / 3 * Time.unscaledDeltaTime);
            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            blackoutImage.color = objectColor;

            if (fadeSpeed > 0 && objectColor.a >= 1) // Faded TO black (opaque)
            {
                go = false;
                // REMOVED SceneManager.LoadScene() from here. GameResetter now handles it.
            }
            else if (fadeSpeed < 0 && objectColor.a <= 0) // Faded OUT (transparent)
            {
                go = false;
                blackoutSquare.SetActive(false); // Deactivate after fading out completely
            }

            yield return null;
        }
    }
}