using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        public AudioClip landedAudio;
        public AudioClip spawnpointAudio;

        public GameObject walkingParticles;


        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;
        Combat combat;

        public Transform spawnPoint;
        GameManager gameManager;
        public GameObject landedVFX;
        public float landedVFXDuration = 2f;
        public Transform landedVFXTransform;

        public GameObject victory1;
        public GameObject victory2;
        public GameObject victory3;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            combat = GetComponent<Combat>();
            gameManager = FindObjectOfType<GameManager>();
        }
        new private void Start() {
            // i want to call player spawn at the beginning of the scene, but there's some problem accessing the simulation...
            spawnPoint = health.spawnpoints[gameManager.spawnPointId];
            Simulation.Schedule<PlayerSpawn>(0);
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                move.x = Input.GetAxis("Horizontal");
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
                    if ((jumpState == JumpState.Grounded) || (jumpState == JumpState.Landed))
                        Schedule<PlayerJumped>().player = this;
                    jumpState = JumpState.PrepareToJump;
                }
                else if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }

        void UpdateFacingDirection()
        {
            if (move.x > 0.01f) {
                spriteRenderer.flipX = false;
                combat.facingRight = true;
            }

            else if (move.x < -0.01f) {
                spriteRenderer.flipX = true;
                combat.facingRight = false;
            }

        }

        public void victory() {
            int score = gameManager.unsavedScore + gameManager.savedScore;
            if (score > 43) {
                victory3.SetActive(true);
            } else if (score > 32) {
                victory2.SetActive(true);
            } else {
                victory1.SetActive(true);
            }
                
        }

        public void LandedVFX(){
            GameObject landed = Instantiate(landedVFX, landedVFXTransform.position, landedVFXTransform.rotation);
            Destroy(landed, landedVFXDuration);
        }

        public void loseUnsavedPoints(){
            gameManager.unsavedScore = 0;
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            UpdateFacingDirection();

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);
            if ((IsGrounded) && ((Mathf.Abs(velocity.x) / maxSpeed) != 0)) {
                walkingParticles.SetActive(true);
            } else {
                walkingParticles.SetActive(false);
            }

            targetVelocity = move * maxSpeed;
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}