using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combat : MonoBehaviour
{

    public Animator animator;

    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    public int attackDamage = 1;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            // Play an attack animation yass kween you go gurl
            animator.SetTrigger("Sexy Attack 69");
            // the animation will call attack()
        }
    }

    public void Attack()
    {
        // Detect enemies that are gonna die
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Damage those baddies
        foreach(Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
        }
    }   

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        return;
    
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
    
