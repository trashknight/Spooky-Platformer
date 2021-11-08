using System.Collections;
using System.Collections.Generic;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    /// <summary>
    /// A simple controller for enemies. Provides movement control over a patrol path.
    /// </summary>
    [RequireComponent(typeof(AnimationController), typeof(Collider2D))]
    public class EnemyController : MonoBehaviour
    {
        public PatrolPath path;
        public AudioClip ouch;

        internal PatrolPath.Mover mover;
        internal AnimationController control;
        internal Collider2D _collider;
        internal AudioSource _audio;
        SpriteRenderer spriteRenderer;

        public Bounds Bounds => _collider.bounds;

        void Awake()
        {
            control = GetComponent<AnimationController>();
            _collider = GetComponent<Collider2D>();
            _audio = GetComponent<AudioSource>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            GameObject playerObj = collision.gameObject;
            var combat = playerObj.GetComponentInParent<Combat>();
            if(collision.collider.tag == "HitboxR") {
               // check facing right
               if (combat.facingRight) {
                   HandlePlayerCollision(playerObj);
               }
            }
            if(collision.collider.tag == "HitboxL") {
               // check facing left
               if (!combat.facingRight) {
                   HandlePlayerCollision(playerObj);
               }
            }
        }

        void HandlePlayerCollision(GameObject playerObj) {
            var player = playerObj.GetComponentInParent<PlayerController>();
            var playerHealth = playerObj.GetComponentInParent<Health>();
            var combat = playerObj.GetComponentInParent<Combat>();
            if (player != null)
            {
                combat.DisableAttack();
                var ev = Schedule<PlayerEnemyCollision>();
                ev.player = player;
                ev.enemy = this;
            }
            if (playerHealth != null) {
                playerHealth.Decrement();
            }
        }

        void Update()
        {
            if (path != null)
            {
                if (mover == null) mover = path.CreateMover(control.maxSpeed * 0.5f);
                control.move.x = Mathf.Clamp(mover.Position.x - transform.position.x, -1, 1);
            }
        }

    }
}