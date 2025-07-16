using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Platformer.Mechanics;

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

    private void Start()
    {
        blackoutSquare = GameObject.FindGameObjectWithTag("Blackout Square");
        player = GetComponent<PlayerController>();

        if (blackoutSquare != null)
        {
            blackoutSquare.SetActive(true);
            StartCoroutine(ExecuteAfterTime(0.5f));
        }
        else
        {
            Debug.LogError("Combat: Blackout Square GameObject not found! Fading will not work.");
        }
    }

    void Update()
    {
        // Prevent attack input if player can't control
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
            hitEnemies = Physics2D.OverlapCircleAll(attackPointL.position, attackRange, enemyLayers);
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

            Debug.Log("sending damage");
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

    public void ReloadScene(float time)
    {
        if (blackoutSquare != null)
        {
            StartCoroutine(FadeBlackoutSquare());
        }
        else
        {
            Debug.LogError("Combat: Cannot start fade. BlackoutSquare is null.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        if (blackoutSquare != null)
        {
            StartCoroutine(FadeBlackoutSquare(-4));
        }
        else
        {
            Debug.LogError("Combat: Cannot start fade. BlackoutSquare is null.");
        }
    }

    IEnumerator FadeBlackoutSquare(float fadeSpeed = 4)
    {
        if (blackoutSquare == null)
        {
            Debug.LogError("Combat: BlackoutSquare is null. Cannot fade.");
            yield break;
        }

        Image blackoutImage = blackoutSquare.GetComponent<Image>();
        if (blackoutImage == null)
        {
            Debug.LogError("Combat: No Image component found on blackoutSquare.");
            yield break;
        }

        Color objectColor = blackoutImage.color;
        float fadeAmount;
        bool go = true;

        while (go)
        {
            fadeAmount = objectColor.a + (fadeSpeed / 3 * Time.deltaTime);
            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            blackoutImage.color = objectColor;

            if (fadeSpeed > 0 && objectColor.a >= 1)
            {
                go = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else if (fadeSpeed < 0 && objectColor.a <= 0)
            {
                go = false;
            }

            yield return null;
        }
    }
}