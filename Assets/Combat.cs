using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Combat : MonoBehaviour
{

    public Animator animator;

    public Transform attackPointR;
    public Transform attackPointL;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    public int attackDamage = 1;

    public float attackRate = 2f;
    float nextattackTime =0f;
    public bool attackEnabled = true;
    public bool facingRight = true;

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

    public void ReloadScene() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
    
