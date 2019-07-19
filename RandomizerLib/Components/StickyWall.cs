using System.Collections;
using System.Reflection;
using GlobalEnums;
using Modding;
using MonoMod.Utils;
using SeanprCore;
using UnityEngine;

// ReSharper disable file UnusedMember.Global

namespace RandomizerLib.Components
{
    internal class StickyWall : MonoBehaviour
    {
        private static readonly FastReflectionDelegate HcWallJump =
            typeof(HeroController).GetMethod("DoWallJump", BindingFlags.Instance | BindingFlags.NonPublic).CreateFastDelegate();

        private static readonly Material Mat = new Material(Shader.Find("Particles/Additive")) { renderQueue = 4000 };

        private BoxCollider2D _box;
        private bool _wallRunning;

        public static void Create(float x, float y, float w, float h)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            GameObject wallClimb = new GameObject();
            wallClimb.layer = 8;
            wallClimb.transform.position = new Vector3(x, y, 0.5f);
            wallClimb.AddComponent<BoxCollider2D>().size = new Vector2(w, h);
            wallClimb.AddComponent<StickyWall>();
        }

        public void Awake()
        {
            // Stuff breaks really hard if we try to remain wall running after getting hit
            ModHooks.Instance.TakeDamageHook += DamageTaken;

            // Store box collider because it has the size of the wall
            _box = GetComponent<BoxCollider2D>();

            // Create line renderer for drawing the rectangle
            LineRenderer lineRend = gameObject.AddComponent<LineRenderer>();
            lineRend.positionCount = 2;

            Bounds bounds = _box.bounds;
            lineRend.SetPositions(new[]
            {
                bounds.center + new Vector3(0, bounds.extents.y),
                bounds.center - new Vector3(0, bounds.extents.y)
            });

            lineRend.startWidth = _box.bounds.size.x;
            lineRend.endWidth = bounds.size.x;
            lineRend.sharedMaterial = Mat;
            lineRend.startColor = new Color(0x88, 0xFF, 0x88, 0xFF);
            lineRend.endColor = new Color(0x88, 0xFF, 0x88, 0xFF);
        }

        public void OnDisable()
        {
            // Unhook everything and fix the hero on unload
            On.HeroController.DoDoubleJump -= No;
            ModHooks.Instance.TakeDamageHook -= DamageTaken;

            if (_wallRunning)
            {
                FixHero(Ref.Hero);
            }
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player"))
            {
                return;
            }

            HeroController hc = collision.gameObject.GetComponent<HeroController>();

            if (hc == null || _wallRunning)
            {
                return;
            }

            // Start wall running if the player touches the wall
            StartWallRunning(hc);
        }

        public void OnCollisionStay2D(Collision2D collision)
        {
            if (!collision.gameObject.CompareTag("Player"))
            {
                return;
            }

            HeroController hc = collision.gameObject.GetComponent<HeroController>();

            if (hc == null || _wallRunning)
            {
                return;
            }

            // Also try to start wall running on collider stay because the one on enter might fail
            // due to incompatible hero state
            StartWallRunning(hc);
        }

        public void Update()
        {
            // Jump off the wall if the player presses jump
            if (_wallRunning && Ref.Input.inputActions.jump.WasPressed)
            {
                StopWallRunning(Ref.Hero);
                return;
            }

            HeroController hc = Ref.Hero;

            if (hc == null || !_wallRunning)
            {
                return;
            }

            GameObject hero = hc.gameObject;

            // Handle movement of the hero
            // Bounds checking is only in the direction the player is currently moving, but this should be fine
            if (hc.transform.position.y < transform.position.y + _box.size.y / 2 &&
                Ref.Input.inputActions.left.IsPressed)
            {
                hc.FaceLeft();
                hero.transform.SetPositionX(transform.position.x + _box.size.x + .15f);

                hero.transform.SetPositionY(hero.transform.position.y +
                                                     GetHCRunSpeed() * Time.deltaTime);
                hc.GetComponent<tk2dSpriteAnimator>().Play(GetHCRunAnimName());
            }
            else if (hc.transform.position.y > transform.position.y - _box.size.y / 2 &&
                     Ref.Input.inputActions.right.IsPressed)
            {
                hc.FaceRight();
                hero.transform.SetPositionX(transform.position.x + _box.size.x + .15f);

                hero.transform.SetPositionY(hero.transform.position.y -
                                                     GetHCRunSpeed() * Time.deltaTime);
                hc.GetComponent<tk2dSpriteAnimator>().Play(GetHCRunAnimName());
            }
            else if (Ref.Input.inputActions.left.WasReleased ||
                     Ref.Input.inputActions.right.WasReleased)
            {
                hc.GetComponent<tk2dSpriteAnimator>().Play("Run To Idle");
                hero.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
            else
            {
                string animName = hc.GetComponent<tk2dSpriteAnimator>().CurrentClip.name;
                if (animName != "Run To Idle")
                {
                    hc.GetComponent<tk2dSpriteAnimator>().Play("Idle");
                }

                hero.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
        }

        private void StartWallRunning(HeroController hc)
        {
            // Check for incompatible hero state
            if (_wallRunning || hc.controlReqlinquished || hc.hero_state != ActorStates.idle &&
                hc.hero_state != ActorStates.running && hc.hero_state != ActorStates.airborne)
            {
                return;
            }

            // Completely remove control of the character from the game
            hc.RelinquishControl();
            hc.GetComponent<HeroAnimationController>().StopControl();
            hc.AffectedByGravity(false);

            // Rotate towards the wall and move a little bit away to prevent clipping
            hc.transform.SetPositionX(transform.position.x + _box.size.x + .15f);
            hc.transform.rotation = Quaternion.Euler(0, 0, -90);

            // Make sure the hero is inside the vertical bounds of the collider
            // They can otherwise be quite a ways above it after the rotation
            if (hc.transform.position.y > transform.position.y + _box.size.y / 2)
            {
                hc.transform.SetPositionY(transform.position.y + _box.size.y / 2);
            }
            else if (hc.transform.position.y < transform.position.y - _box.size.y / 2)
            {
                hc.transform.SetPositionY(transform.position.y - _box.size.y / 2);
            }

            _wallRunning = true;
        }

        private void FixHero(HeroController hc)
        {
            if (hc == null)
            {
                return;
            }

            // Return control to the game, reset rotation
            hc.AffectedByGravity(true);
            hc.RegainControl();
            hc.GetComponent<HeroAnimationController>().StartControl();
            hc.gameObject.transform.rotation = Quaternion.identity;

            _wallRunning = false;
        }

        private void StopWallRunning(HeroController hc)
        {
            if (hc == null)
            {
                return;
            }

            FixHero(hc);

            // Move the hero a bit up left after the rotation in FixHero to prevent clipping
            Transform t = hc.transform;
            Vector3 pos = t.position;
            t.SetPositionX(pos.x - 0.75f);
            hc.transform.SetPositionY(pos.y + 0.5f);

            // Force a wall jump
            hc.FaceLeft();
            hc.cState.wallSliding = true;
            hc.touchingWallL = true;
            HcWallJump(hc, null);

            // If the player has wings, prevent a double jump
            if (Ref.PD.hasDoubleJump)
            {
                On.HeroController.DoDoubleJump -= No;
                On.HeroController.DoDoubleJump += No;

                // Unhook after one frame just to be 100% certain this doesn't cause dropped inputs
                StartCoroutine(UnhookNo());
            }
        }

        // Undo all the wall run stuff on getting hit
        private int DamageTaken(ref int hazardType, int damage)
        {
            if (_wallRunning)
            {
                FixHero(Ref.Hero);
            }

            return damage;
        }

        // Used to prevent double jump
        // Redundant unhook here because I really really don't want to cause dropped inputs
        private void No(On.HeroController.orig_DoDoubleJump orig, HeroController self)
        {
            On.HeroController.DoDoubleJump -= No;
        }

        // Unhook the no double jump function after a frame
        private IEnumerator UnhookNo()
        {
            yield return new WaitForEndOfFrame();
            On.HeroController.DoDoubleJump -= No;
        }

        private static float GetHCRunSpeed()
        {
            // Sprintmaster and dashmaster
            if (Ref.PD.GetBool(nameof(PlayerData.equippedCharm_37)))
            {
                return Ref.PD.GetBool(nameof(PlayerData.equippedCharm_31))
                    ? Ref.Hero.RUN_SPEED_CH_COMBO
                    : Ref.Hero.RUN_SPEED_CH;
            }

            return Ref.Hero.RUN_SPEED;
        }

        private static string GetHCRunAnimName()
        {
            return Ref.PD.GetBool(nameof(PlayerData.equippedCharm_37)) ? "Sprint" : "Run";
        }
    }
}