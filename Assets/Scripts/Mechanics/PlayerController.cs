using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using TMPro;
using UnityEngine.SceneManagement;

namespace Platformer.Mechanics
{
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        public AudioClip landedAudio;
        public AudioClip spawnpointAudio;

        [Header("Spawn Audio")]
        public AudioClip firstSpawnAudio;

        [HideInInspector] public bool hasSpawnedOnce = false;

        public GameObject walkingParticles;
        public float maxSpeed = 7;
        public float jumpTakeOffSpeed = 7;

        public Collider2D collider2d;
        public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        public Transform spawnPoint;
        public GameObject landedVFX;
        public float landedVFXDuration = 2f;
        public Transform landedVFXTransform;
        public GameObject respawnVFX;

        public GameObject victory1;
        public GameObject victory2;
        public GameObject victory3;

        public TextMeshProUGUI commonVialCountText;
        public TextMeshProUGUI commonRestartMessageText;
        public GameObject commonTokenIcon;

        [Header("Ground & Ceiling Detection")]
        public LayerMask whatIsGround;

        private SpriteRenderer spriteRenderer;
        internal Animator animator;
        private Combat combat;
        private GameManager gameManager;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        private bool jump;
        private Vector2 move;
        private bool wasGroundedLastFrame = true;
        private bool hasPlayedJumpAudio = false;
        private float groundedTime = 0f;
        private const float minGroundedTimeBeforeJump = 0.05f;

        public Bounds Bounds => collider2d.bounds;

        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            combat = GetComponent<Combat>();
            gameManager = FindObjectOfType<GameManager>();

            if (victory1 != null) victory1.SetActive(false);
            if (victory2 != null) victory2.SetActive(false);
            if (victory3 != null) victory3.SetActive(false);

            if (commonVialCountText != null) commonVialCountText.gameObject.SetActive(false);
            if (commonRestartMessageText != null) commonRestartMessageText.gameObject.SetActive(false);
            if (commonTokenIcon != null) commonTokenIcon.SetActive(false);
        }

        new private void Start()
        {
            Simulation.Schedule<PlayerSpawn>(0);
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                move.x = Input.GetAxis("Horizontal");

                bool jumpPressed = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
                bool jumpReleased = Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow);

                if (jumpPressed && (jumpState == JumpState.Grounded || jumpState == JumpState.Landed) && groundedTime > minGroundedTimeBeforeJump)
                {
                    Debug.Log($"Jump initiated after {groundedTime:F3}s grounded. State: {jumpState}, time: {Time.time}");
                    jumpState = JumpState.PrepareToJump;
                }

                if (jumpReleased)
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

            if (controlEnabled && !wasGroundedLastFrame && IsGrounded)
            {
                Schedule<PlayerLanded>().player = this;
            }

            if (IsGrounded)
                groundedTime += Time.deltaTime;
            else
                groundedTime = 0f;

            wasGroundedLastFrame = IsGrounded;
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                Schedule<PlayerJumped>().player = this;
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;

                if (!hasPlayedJumpAudio && jumpAudio != null && controlEnabled)
                {
                    audioSource.PlayOneShot(jumpAudio);
                    hasPlayedJumpAudio = true;
                }

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

            if (velocity.y > 0 && IsTouchingCeiling())
            {
                velocity.y = 0;
            }

            UpdateFacingDirection();

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);
            walkingParticles.SetActive(IsGrounded && Mathf.Abs(velocity.x / maxSpeed) != 0);

            targetVelocity = move * maxSpeed;
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
                    hasPlayedJumpAudio = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                        jumpState = JumpState.InFlight;
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                        jumpState = JumpState.Landed;
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        void UpdateFacingDirection()
        {
            if (move.x > 0.01f)
            {
                spriteRenderer.flipX = false;
                combat.facingRight = true;
                FlipColliderOffset(true);
            }
            else if (move.x < -0.01f)
            {
                spriteRenderer.flipX = true;
                combat.facingRight = false;
                FlipColliderOffset(false);
            }
        }

        void FlipColliderOffset(bool facingRight)
        {
            BoxCollider2D box = collider2d as BoxCollider2D;
            if (box != null)
            {
                Vector2 offset = box.offset;
                offset.x = facingRight ? Mathf.Abs(offset.x) : -Mathf.Abs(offset.x);
                box.offset = offset;
            }
        }

        bool IsTouchingCeiling()
        {
            Bounds bounds = collider2d.bounds;
            Vector2 origin = new Vector2(bounds.center.x, bounds.max.y + 0.01f);
            Vector2 size = new Vector2(bounds.size.x * 0.95f, 0.05f);
            RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.up, 0.02f, whatIsGround);
            return hit.collider != null;
        }

        public void LandedVFX()
        {
            GameObject landed = Instantiate(landedVFX, landedVFXTransform.position, landedVFXTransform.rotation);
            Destroy(landed, landedVFXDuration);
        }

        public void loseUnsavedPoints()
        {
            gameManager.unsavedScore = 0;
        }

        public void victory()
        {
            int score = gameManager.unsavedScore + gameManager.savedScore;
            int totalVialsInLevel = 60;

            if (victory1 != null) victory1.SetActive(false);
            if (victory2 != null) victory2.SetActive(false);
            if (victory3 != null) victory3.SetActive(false);

            if (commonVialCountText != null) commonVialCountText.gameObject.SetActive(true);
            if (commonRestartMessageText != null) commonRestartMessageText.gameObject.SetActive(true);
            if (commonTokenIcon != null) commonTokenIcon.SetActive(true);

            if (commonVialCountText != null)
            {
                commonVialCountText.text = $"COLLECTED: {score}/{totalVialsInLevel}";
                commonVialCountText.color = score == totalVialsInLevel ? Color.yellow : Color.white;
            }

            if (commonRestartMessageText != null)
                commonRestartMessageText.text = "CLICK ANYWHERE TO RESTART";

            if (score > 46 && victory3 != null) victory3.SetActive(true);
            else if (score > 36 && victory2 != null) victory2.SetActive(true);
            else if (victory1 != null) victory1.SetActive(true);
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
