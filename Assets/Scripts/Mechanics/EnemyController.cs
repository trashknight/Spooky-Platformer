using System.Collections;
using System.Collections.Generic;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
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
            HandleCollisionLogic(collision);
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            HandleCollisionLogic(collision);
        }

        void HandleCollisionLogic(Collision2D collision)
        {
            GameObject playerObj = collision.gameObject;
            var combat = playerObj.GetComponentInParent<Combat>();
            if (combat == null) return;

            bool playerHitFromRight = collision.collider.CompareTag("HitboxR") && combat.facingRight;
            bool playerHitFromLeft  = collision.collider.CompareTag("HitboxL") && !combat.facingRight;

            if (playerHitFromRight || playerHitFromLeft)
            {
                var player = playerObj.GetComponentInParent<PlayerController>();
                var playerHealth = playerObj.GetComponentInParent<Health>();

                if (player != null)
                {
                    var ev = Schedule<PlayerEnemyCollision>();
                    ev.player = player;
                    ev.enemy = this;
                }

                if (playerHealth != null && playerHealth.IsAlive && !playerHealth.isInvincible)
                {
                    playerHealth.TakeDamage(1, false);
                }
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