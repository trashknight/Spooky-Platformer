using System.Collections;

using UnityEngine;

using Platformer.Mechanics;



public class BossIntro : MonoBehaviour

{

    public Animator bossAnimator;               // Animator on Spider Idle

    public SpriteRenderer bossSpriteRenderer;   // Sprite Renderer on Spider Idle

    public Sprite idleSprite;                   // The static idle sprite to show after roaring

    public Sprite spiderRiseSprite;             // Assign the "Spider Rise" sprite here

    public Transform spiderParent;              // The top-level SpiderBoss GameObject

    public PlayerController player;             // The Player (with PlayerController script)



    public AudioSource roarAudio;     // Drag in the Audio Source from Spider Idle

    public AudioClip roarClip;        // Drag in the SpiderRoar.wav



    public float riseHeight = 5f;               // How far the spider rises

    public float riseDuration = 1.5f;           // How long the rising takes

    public float roarDuration = 2f;             // How long to wait during roar



    private bool hasPlayed = false;



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

        player.controlEnabled = false;



        // Play the roar sound

        roarAudio.PlayOneShot(roarClip);



        // 2. Play the roar animation

        bossAnimator.Play("SpiderRoar");



        // 3. Wait for the roar animation to finish

        yield return new WaitForSeconds(roarDuration);



        // 4. Stop the animator and set the rising sprite

        bossAnimator.enabled = false;

        bossSpriteRenderer.sprite = spiderRiseSprite;



        // 5. Move the spider upward over time

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



        // 6. Re-enable the player

        player.controlEnabled = true;



        Debug.Log("Starting attack pattern now");

        transform.parent.GetComponent<BossAttackBehavior>().StartAttack();

    }

}