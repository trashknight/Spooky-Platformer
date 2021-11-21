using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    float nextattackTime =0f;
    public bool attackEnabled = true;
    public bool facingRight = true;

    GameObject blackoutSquare;

    private void Start() {
        blackoutSquare = GameObject.FindGameObjectWithTag("Blackout Square");
        blackoutSquare.SetActive(true);
        StartCoroutine(ExecuteAfterTime(0.5f));
    }

    

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextattackTime)
        {
            if ((Input.GetKeyDown(KeyCode.Space)) && attackEnabled)
            {
                // Play an attack animation yass kween you go gurl
                animator.SetTrigger("Sexy Attack 69");
                // the animation will call attack()
                nextattackTime = Time.time + 1f / attackRate;
            }
        }
    }

    public void Attack()
    {
        if (attackEnabled) {
            Debug.Log("Calling attack");
            audioSource.PlayOneShot(biteAudio);
            // Detect enemies that are gonna die
            Collider2D[] hitEnemies;
            if (facingRight) {
                hitEnemies = Physics2D.OverlapCircleAll(attackPointR.position, attackRange, enemyLayers);
            }
            else {
                hitEnemies = Physics2D.OverlapCircleAll(attackPointL.position, attackRange, enemyLayers);
            }

            // Damage those baddies
            foreach(Collider2D enemy in hitEnemies)
            {
                // if you've destroyed a pumpkin, make the corresponding pumpkineer grow a new one
                PumpkinScript pumpkin = enemy.GetComponent<PumpkinScript>();
                if (pumpkin) {
                    int spawnId = pumpkin.spawnId;
                    foreach (var pumpkineer in FindObjectsOfType(typeof(ShootProjectile)) as ShootProjectile[]) {
                        if (pumpkineer.SpawnID == spawnId) {
                            pumpkineer.RespawnPumpkin();
                        }
                    }
                }
                enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
                Debug.Log("sending damage");
            }
        }
    }   

    public void EnableAttack() {
        attackEnabled = true;
        //Debug.Log("Enabling Attack");
    }

    public void DisableAttack() {
        attackEnabled = false;
        //Debug.Log("Disabling Attack");
    }

    private void OnDrawGizmosSelected()
    {
        if ((attackPointR == null) || (attackPointL == null))
        return;
    
        Gizmos.DrawWireSphere(attackPointR.position, attackRange);
        Gizmos.DrawWireSphere(attackPointL.position, attackRange);
    }

    public void ReloadScene(float time) {
        // add in fadeout
        StartCoroutine(FadeBlackoutSquare());
    }

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        StartCoroutine(FadeBlackoutSquare(-4));
    }

    IEnumerator FadeBlackoutSquare(float fadeSpeed = 4) {
        Color objectColor = blackoutSquare.GetComponent<Image>().color;
        float fadeAmount;
        bool go = true;
        while (go) {
            fadeAmount = objectColor.a + (fadeSpeed/3 * Time.deltaTime);

            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            blackoutSquare.GetComponent<Image>().color = objectColor;
            if ((blackoutSquare.GetComponent<Image>().color.a >= 1) && (fadeSpeed > 0)) {
                go = false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            if ((blackoutSquare.GetComponent<Image>().color.a <= 0) && (fadeSpeed < 0)) {
                go = false;
            }
            yield return null;
        }       
    }
}
    
