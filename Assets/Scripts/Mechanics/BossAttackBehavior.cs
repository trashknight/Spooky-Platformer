using UnityEngine;

using System.Collections;



public class BossAttackBehavior : MonoBehaviour

{

    private Coroutine attackLoopCoroutine;

    public Transform player;

    public Transform mouthPoint;

    public GameObject poisonBallPrefab;



    public SpriteRenderer spriteRenderer;

    public Sprite descendSprite;

    public Sprite riseSprite;

    public Sprite neutralSprite;

    public Sprite spit1Sprite;

    public Sprite spit2Sprite;



    public Transform arenaLeftBound;

    public Transform arenaRightBound;

    public float hoverHeight = 3f;

    public float riseHeight = 7f;



    public float minOffset = 2f;

    public float maxOffset = 6f;



    public AudioClip spitSound;

    private AudioSource audioSource;



    private bool isAttacking = false;



    [HideInInspector]

    public bool isSpitting = false;



    void Start()

    {

        audioSource = GetComponent<AudioSource>();

    }



    public void StartAttack()

    {

        if (!isAttacking)

        {

            isAttacking = true;

            attackLoopCoroutine = StartCoroutine(BeginAttackLoop());

        }

    }



    IEnumerator BeginAttackLoop()

    {

        yield return new WaitForSeconds(1f); // Short pause after intro



        while (true)

        {

            yield return StartCoroutine(AttackSequence());

        }

    }



    IEnumerator AttackSequence()

    {

        // 1. Pick random x-position near player, clamped inside arena

        float offset = Random.Range(minOffset, maxOffset);

        if (Random.value < 0.5f) offset = -offset; // 50% chance to go left

        float leftLimit = arenaLeftBound.position.x;

        float rightLimit = arenaRightBound.position.x;

        float targetX = Mathf.Clamp(player.position.x + offset, leftLimit, rightLimit);

        Vector3 targetPos = new Vector3(targetX, riseHeight, 0);





        // 2. Teleport to that x-position at top of screen

        transform.position = targetPos;



        // 3. Face the player

        Vector3 currentScale = transform.localScale;

        currentScale.x = (player.position.x < transform.position.x) ? Mathf.Abs(currentScale.x) : -Mathf.Abs(currentScale.x);

        transform.localScale = currentScale;



        // 4. Switch to descend sprite

        spriteRenderer.sprite = descendSprite;



        // 5. Descend

        float duration = 1f;

        Vector3 start = transform.position;

        Vector3 end = new Vector3(targetX, hoverHeight, 0);

        float t = 0;

        while (t < duration)

        {

            transform.position = Vector3.Lerp(start, end, t / duration);

            t += Time.deltaTime;

            yield return null;

        }

        transform.position = end;



        // 6. Switch to neutral and pause

        spriteRenderer.sprite = neutralSprite;

        yield return new WaitForSeconds(0.5f);



        isSpitting = true;



        // 7. Spit animation (Spit1 → Spit2)

        spriteRenderer.sprite = spit1Sprite;

        audioSource.PlayOneShot(spitSound);

        yield return new WaitForSeconds(0.25f);



        spriteRenderer.sprite = spit2Sprite;



        // 8. Spawn poison

        GameObject spit = Instantiate(poisonBallPrefab, mouthPoint.position, Quaternion.identity);

        Vector2 dir = (player.position.x < transform.position.x) ? Vector2.left : Vector2.right;

        spit.GetComponent<PoisonProjectile>().SetDirection(dir);



        yield return new WaitForSeconds(0.4f);



        // 9. Revert to neutral and pause

        spriteRenderer.sprite = neutralSprite;

        isSpitting = false;

        yield return new WaitForSeconds(0.4f);



        // 10. Rise up

        spriteRenderer.sprite = riseSprite;

        Vector3 riseEnd = new Vector3(transform.position.x, riseHeight, 0);

        float riseTime = 0;

        while (riseTime < 0.75f)

        {

            transform.position = Vector3.Lerp(transform.position, riseEnd, riseTime / 0.75f);

            riseTime += Time.deltaTime;

            yield return null;

        }

        transform.position = riseEnd;

    }



    public void StopAttack()

    {

        StopAllCoroutines(); // This kills the current descent/spit/rise

        isAttacking = false;

    }

}