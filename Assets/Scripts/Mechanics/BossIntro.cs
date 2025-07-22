using System.Collections;
using UnityEngine;
using Platformer.Mechanics;
using Cinemachine;

public class BossIntro : MonoBehaviour
{
    public Animator bossAnimator;               // Animator on Spider Idle
    public SpriteRenderer bossSpriteRenderer;   // Sprite Renderer on Spider Idle
    public Sprite idleSprite;                   // The static idle sprite to show after roaring
    public Sprite spiderRiseSprite;             // Assign the "Spider Rise" sprite here
    public Transform spiderParent;              // The top-level SpiderBoss GameObject
    public PlayerController player;             // The Player (with PlayerController script)

    public AudioSource roarAudio;               // Drag in the Audio Source from Spider Idle
    public AudioClip roarClip;                  // Drag in the SpiderRoar.wav

    public float riseHeight = 5f;               // How far the spider rises
    public float riseDuration = 1.5f;           // How long the rising takes
    public float roarDuration = 2f;             // How long to wait during roar

    private bool hasPlayed = false;

    [Header("Cinemachine Impulse")]
    public CinemachineImpulseSource impulseSource;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasPlayed && other.CompareTag("Player"))
        {
            hasPlayed = true;
            StartCoroutine(StartBossIntro());
        }
    }

    IEnumerator StartBossIntro()
    {
        // 1. Freeze the player
        if (player != null)
            player.controlEnabled = false;

        // 2. Play roar sound
        if (roarAudio != null && roarClip != null)
            roarAudio.PlayOneShot(roarClip);

        // 3. Play roar animation
        if (bossAnimator != null)
            bossAnimator.Play("SpiderRoar");

        // 4. Trigger Cinemachine impulse for screen shake
        if (impulseSource != null)
            impulseSource.GenerateImpulse();
        else
            Debug.LogWarning("BossIntro: CinemachineImpulseSource missing!");

        // 5. Wait for roar to finish
        yield return new WaitForSeconds(roarDuration);

        // 6. Disable animator, switch to rise sprite
        if (bossAnimator != null)
            bossAnimator.enabled = false;

        if (bossSpriteRenderer != null && spiderRiseSprite != null)
            bossSpriteRenderer.sprite = spiderRiseSprite;

        // 7. Move spider up
        Vector3 startPos = spiderParent.position;
        Vector3 endPos = startPos + Vector3.up * riseHeight;

        float elapsed = 0f;
        while (elapsed < riseDuration)
        {
            spiderParent.position = Vector3.Lerp(startPos, endPos, elapsed / riseDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        spiderParent.position = endPos;

        // 8. Unfreeze player
        if (player != null)
            player.controlEnabled = true;

        // 9. Start attack phase
        BossAttackBehavior attack = transform.parent.GetComponent<BossAttackBehavior>();
        if (attack != null)
        {
            Debug.Log("Starting attack pattern now");
            attack.StartAttack();
        }
        else
        {
            Debug.LogWarning("BossIntro: BossAttackBehavior not found on parent.");
        }
    }
}