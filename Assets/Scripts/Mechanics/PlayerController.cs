using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using TMPro; // Needed for TextMeshProUGUI
using UnityEngine.SceneManagement; // Needed for SceneManager.LoadScene (keep this one!)

namespace Platformer.Mechanics
{
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        public AudioClip landedAudio;
        public AudioClip spawnpointAudio;

        public GameObject walkingParticles;

        public float maxSpeed = 7;
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
        public Combat combat; // FIXED: Changed to public

        public Transform spawnPoint;
        GameManager gameManager; // Reference to your GameManager
        public GameObject landedVFX;
        public float landedVFXDuration = 2f;
        public Transform landedVFXTransform;

        public GameObject respawnVFX; // ✅ NEW: Reference to dirt particle prefab

        // These are your references to the unique victory screen UI panels/GameObjects
        public GameObject victory1;
        public GameObject victory2;
        public GameObject victory3;

        // References for common UI elements (Token Icon, Vial Count Text, Restart Message Text)
        public TextMeshProUGUI commonVialCountText;
        public TextMeshProUGUI commonRestartMessageText;
        public GameObject commonTokenIcon; // Reference for the Image GameObject itself

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            combat = GetComponent<Combat>();
            gameManager = FindObjectOfType<GameManager>();

            // Ensure ALL victory-related UI elements are hidden at the start of the scene.
            if (victory1 != null) victory1.SetActive(false);
            if (victory2 != null) victory2.SetActive(false);
            if (victory3 != null) victory3.SetActive(false);

            // Hide the common UI elements initially as well.
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
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                {
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
            if (move.x > 0.01f)
            {
                spriteRenderer.flipX = false;
                combat.facingRight = true;
            }
            else if (move.x < -0.01f)
            {
                spriteRenderer.flipX = true;
                combat.facingRight = false;
            }
        }

        public void victory()
        {
            int score = gameManager.unsavedScore + gameManager.savedScore;
            int totalVialsInLevel = 60;

            Debug.Log($"Player collected a total of {score} vials. Determining victory screen...");

            if (victory1 != null) victory1.SetActive(false);
            if (victory2 != null) victory2.SetActive(false);
            if (victory3 != null) victory3.SetActive(false);

            if (commonVialCountText != null) commonVialCountText.gameObject.SetActive(true);
            if (commonRestartMessageText != null) commonRestartMessageText.gameObject.SetActive(true);
            if (commonTokenIcon != null) commonTokenIcon.SetActive(true);

            if (commonVialCountText != null)
            {
                commonVialCountText.text = $"COLLECTED: {score}/{totalVialsInLevel}";
                if (score == totalVialsInLevel)
                {
                    commonVialCountText.color = Color.yellow;
                    Debug.Log("Full collection! Vial count text set to yellow.");
                }
                else
                {
                    commonVialCountText.color = Color.white;
                    Debug.Log("Partial collection. Vial count text set to default color.");
                }
            }
            else
            {
                Debug.LogWarning("Common Vial Count Text not assigned in Inspector!");
            }

            if (commonRestartMessageText != null)
            {
                commonRestartMessageText.text = "CLICK ANYWHERE TO RESTART";
            }
            else
            {
                Debug.LogWarning("Common Restart Message Text not assigned in Inspector!");
            }

            if (score > 46)
            {
                if (victory3 != null)
                {
                    victory3.SetActive(true);
                    Debug.Log("Activated Victory Screen 3 (Score > 43)");
                }
            }
            else if (score > 36)
            {
                if (victory2 != null)
                {
                    victory2.SetActive(true);
                    Debug.Log("Activated Victory Screen 2 (Score 33-43)");
                }
            }
            else
            {
                if (victory1 != null)
                {
                    victory1.SetActive(true);
                    Debug.Log("Activated Victory Screen 1 (Score 0-32)");
                }
            }
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
            if ((IsGrounded) && ((Mathf.Abs(velocity.x) / maxSpeed) != 0))
            {
                walkingParticles.SetActive(true);
            }
            else
            {
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