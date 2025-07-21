using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Platformer.Mechanics;
using Platformer.Core; // Needed for GameResetter

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

    private bool hasPlayedInitialFade = false;
    public bool HasAlreadyFaded { get; private set; } = false;

    void Awake()
    {
        blackoutSquare = GameObject.FindGameObjectWithTag("Blackout Square");
        player = GetComponent<PlayerController>();

        if (blackoutSquare != null)
        {
            Image blackoutImage = blackoutSquare.GetComponent<Image>();
            if (blackoutImage != null)
            {
                blackoutImage.color = new Color(blackoutImage.color.r, blackoutImage.color.g, blackoutImage.color.b, 1);
                blackoutSquare.SetActive(true);
            }
        }
    }

    public void TriggerInitialGameFade()
    {
        if (!hasPlayedInitialFade && blackoutSquare != null)
        {
            StartCoroutine(InitialFadeSequence());
            hasPlayedInitialFade = true;
        }
    }

    void Update()
    {
        if (player != null && !player.controlEnabled)
            return;

        if (Time.time >= nextattackTime)
        {
            if (Input.GetKeyDown(KeyCode.Space) && attackEnabled)
            {
                animator?.SetTrigger("Sexy Attack 69");
                nextattackTime = Time.time + 1f / attackRate;
            }
        }
    }

    public void Attack()
    {
        if (!attackEnabled || (player != null && !player.controlEnabled)) return;

        audioSource?.PlayOneShot(biteAudio);

        Collider2D[] hitEnemies;

        if (facingRight && attackPointR != null)
        {
            hitEnemies = Physics2D.OverlapCircleAll(attackPointR.position, attackRange, enemyLayers);
        }
        else if (!facingRight && attackPointL != null)
        {
            hitEnemies = Physics2D.OverlapCircleAll(attackPointL.position, attackRange, enemyLayers);
        }
        else
        {
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
            if (e != null) e.TakeDamage(attackDamage);
            else
            {
                SpiderHealth spider = enemy.GetComponent<SpiderHealth>();
                if (spider != null) spider.TakeDamage(attackDamage);
                else
                {
                    DestructibleProjectile projectile = enemy.GetComponent<DestructibleProjectile>();
                    if (projectile != null) projectile.DestroyProjectile();
                }
            }
        }
    }

    public void EnableAttack() => attackEnabled = true;
    public void DisableAttack() => attackEnabled = false;

    private void OnDrawGizmosSelected()
    {
        if (attackPointR != null)
            Gizmos.DrawWireSphere(attackPointR.position, attackRange);
        if (attackPointL != null)
            Gizmos.DrawWireSphere(attackPointL.position, attackRange);
    }

    public void InitiateFadeAndReload(float duration)
    {
        StartCoroutine(FadeToBlack(duration));
    }

    public IEnumerator FadeToBlack(float duration)
    {
        HasAlreadyFaded = true;
        yield return StartCoroutine(ReloadSceneCoroutine(duration));
    }

    public IEnumerator ReloadSceneCoroutine(float time)
    {
        if (blackoutSquare != null)
        {
            Image blackoutImage = blackoutSquare.GetComponent<Image>();
            if (blackoutImage != null)
            {
                blackoutImage.color = new Color(blackoutImage.color.r, blackoutImage.color.g, blackoutImage.color.b, 0);
            }

            blackoutSquare.SetActive(true);
            yield return StartCoroutine(FadeBlackoutSquare(4));
        }
    }

    IEnumerator InitialFadeSequence()
    {
        if (blackoutSquare == null) yield break;
        Image blackoutImage = blackoutSquare.GetComponent<Image>();
        if (blackoutImage == null) yield break;

        blackoutImage.color = new Color(blackoutImage.color.r, blackoutImage.color.g, blackoutImage.color.b, 1);
        blackoutSquare.SetActive(true);
        yield return StartCoroutine(FadeBlackoutSquare(-4));
    }

    IEnumerator FadeBlackoutSquare(float fadeSpeed = 4)
    {
        if (blackoutSquare == null) yield break;

        Image blackoutImage = blackoutSquare.GetComponent<Image>();
        if (blackoutImage == null) yield break;

        Color objectColor = blackoutImage.color;
        float fadeAmount;
        bool go = true;

        if (fadeSpeed > 0) objectColor.a = 0;
        else objectColor.a = 1;

        blackoutImage.color = objectColor;

        while (go)
        {
            fadeAmount = objectColor.a + (fadeSpeed / 3 * Time.unscaledDeltaTime);
            objectColor.a = Mathf.Clamp01(fadeAmount);
            blackoutImage.color = objectColor;

            if ((fadeSpeed > 0 && objectColor.a >= 1) || (fadeSpeed < 0 && objectColor.a <= 0))
                go = false;

            yield return null;
        }

        if (fadeSpeed < 0)
            blackoutSquare.SetActive(false);
    }

    public void FadeInAfterRespawn(float delaySeconds = 0f)
    {
        StartCoroutine(FadeInAfterDelay(delaySeconds));
    }

    private IEnumerator FadeInAfterDelay(float delay)
    {
        if (blackoutSquare == null) yield break;

        Canvas blackCanvas = blackoutSquare.GetComponentInParent<Canvas>();
        if (blackCanvas != null)
            blackCanvas.sortingOrder = 999;

        yield return new WaitForSecondsRealtime(delay);
        yield return new WaitForEndOfFrame();

        Image blackoutImage = blackoutSquare.GetComponent<Image>();
        if (blackoutImage == null) yield break;

        blackoutSquare.SetActive(true);
        Color objectColor = blackoutImage.color;
        objectColor.a = 1;
        blackoutImage.color = objectColor;

        float fadeSpeed = -4f;
        while (objectColor.a > 0f)
        {
            float fadeAmount = objectColor.a + (fadeSpeed / 3 * Time.unscaledDeltaTime);
            objectColor.a = Mathf.Clamp01(fadeAmount);
            blackoutImage.color = objectColor;
            yield return null;
        }

        blackoutSquare.SetActive(false);

        if (blackCanvas != null)
            blackCanvas.sortingOrder = 900;
    }

    public void SetBlackoutInstant()
    {
        var image = blackoutSquare.GetComponent<Image>();
        if (image != null)
        {
            var color = image.color;
            color.a = 1f;
            image.color = color;
        }
    }
}
