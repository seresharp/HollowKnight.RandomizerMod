using System;
using System.Collections;
using System.Reflection;
using Modding;
using RandomizerMod.Extensions;
using UnityEngine;

namespace RandomizerMod.Components
{
    internal class StickyWall : MonoBehaviour
    {
        private static MethodInfo hcWallJump = typeof(HeroController).GetMethod("DoWallJump", BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo hcCanDoubleJump = typeof(HeroController).GetMethod("CanDoubleJump", BindingFlags.Instance | BindingFlags.NonPublic);

        private BoxCollider2D box;
        private bool wallRunning;

        public static void Create(float x, float y, float w, float h)
        {
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
            box = GetComponent<BoxCollider2D>();

            // Create line renderer for drawing the rectangle
            LineRenderer lineRend = gameObject.AddComponent<LineRenderer>();
            lineRend.positionCount = 2;
            lineRend.SetPositions(new Vector3[] { box.bounds.center + new Vector3(0, box.bounds.extents.y, -1), box.bounds.center - new Vector3(0, box.bounds.extents.y, 1) });
            lineRend.startWidth = box.bounds.size.x;
            lineRend.endWidth = box.bounds.size.x;
            lineRend.sharedMaterial = new Material(Shader.Find("Particles/Additive"));
            lineRend.startColor = new Color(0x88, 0xFF, 0x88, 0xFF);
            lineRend.endColor = new Color(0x88, 0xFF, 0x88, 0xFF);
        }

        public void OnDisable()
        {
            // Unhook everything and fix the hero on unload
            On.HeroController.DoDoubleJump -= No;
            ModHooks.Instance.TakeDamageHook -= DamageTaken;

            if (wallRunning)
            {
                FixHero(HeroController.instance);
            }
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag != "Player")
            {
                return;
            }

            HeroController hc = collision.gameObject.GetComponent<HeroController>();

            if (hc == null || wallRunning)
            {
                return;
            }

            // Start wall running if the player touches the wall
            StartWallRunning(hc);
        }

        public void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.tag != "Player")
            {
                return;
            }

            HeroController hc = collision.gameObject.GetComponent<HeroController>();

            if (hc == null || wallRunning)
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
            if (wallRunning && GameManager.instance.inputHandler.inputActions.jump.WasPressed)
            {
                StopWallRunning(HeroController.instance);
                return;
            }

            HeroController hc = HeroController.instance;

            if (hc == null || !wallRunning)
            {
                return;
            }

            // Handle movement of the hero
            // Bounds checking is only in the direction the player is currently moving, but this should be fine
            if (hc.transform.position.y < (transform.position.y + (box.size.y / 2)) && GameManager.instance.inputHandler.inputActions.left.IsPressed)
            {
                hc.FaceLeft();
                hc.gameObject.transform.SetPositionX(transform.position.x + box.size.x + .15f);

                hc.gameObject.transform.SetPositionY(hc.gameObject.transform.position.y + (hc.GetRunSpeed() * Time.deltaTime));
                hc.GetComponent<tk2dSpriteAnimator>().Play(hc.GetRunAnimName());
            }
            else if (hc.transform.position.y > (transform.position.y - (box.size.y / 2)) && GameManager.instance.inputHandler.inputActions.right.IsPressed)
            {
                hc.FaceRight();
                hc.gameObject.transform.SetPositionX(transform.position.x + box.size.x + .15f);

                hc.gameObject.transform.SetPositionY(hc.gameObject.transform.position.y - (hc.GetRunSpeed() * Time.deltaTime));
                hc.GetComponent<tk2dSpriteAnimator>().Play(hc.GetRunAnimName());
            }
            else if (GameManager.instance.inputHandler.inputActions.left.WasReleased || GameManager.instance.inputHandler.inputActions.right.WasReleased)
            {
                hc.GetComponent<tk2dSpriteAnimator>().Play("Run To Idle");
                hc.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
            else
            {
                string animName = hc.GetComponent<tk2dSpriteAnimator>().CurrentClip.name;
                if (animName != "Run To Idle")
                {
                    hc.GetComponent<tk2dSpriteAnimator>().Play("Idle");
                }

                hc.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
        }

        private void StartWallRunning(HeroController hc)
        {
            // Check for incompatible hero state
            if (wallRunning || hc.controlReqlinquished || (hc.hero_state != GlobalEnums.ActorStates.idle && hc.hero_state != GlobalEnums.ActorStates.running && hc.hero_state != GlobalEnums.ActorStates.airborne))
            {
                return;
            }

            // Completely remove control of the character from the game
            hc.RelinquishControl();
            hc.GetComponent<HeroAnimationController>().StopControl();
            hc.AffectedByGravity(false);

            // Rotate towards the wall and move a little bit away to prevent clipping
            hc.transform.SetPositionX(transform.position.x + box.size.x + .15f);
            hc.transform.rotation = Quaternion.Euler(0, 0, -90);

            // Make sure the hero is inside the vertical bounds of the collider
            // They can otherwise be quite a ways above it after the rotation
            if (hc.transform.position.y > (transform.position.y + (box.size.y / 2)))
            {
                hc.transform.SetPositionY(transform.position.y + (box.size.y / 2));
            }
            else if (hc.transform.position.y < (transform.position.y - (box.size.y / 2)))
            {
                hc.transform.SetPositionY(transform.position.y - (box.size.y / 2));
            }

            wallRunning = true;
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

            wallRunning = false;
        }

        private void StopWallRunning(HeroController hc)
        {
            if (hc == null)
            {
                return;
            }

            FixHero(hc);

            // Move the hero a bit up left after the rotation in FixHero to prevent clipping
            hc.transform.SetPositionX(hc.transform.position.x - 0.75f);
            hc.transform.SetPositionY(hc.transform.position.y + 0.5f);

            // Force a wall jump
            hc.FaceLeft();
            hc.cState.wallSliding = true;
            hc.touchingWallL = true;
            hcWallJump.Invoke(hc, null);

            // If the player has wings, prevent a double jump
            if (PlayerData.instance.hasDoubleJump)
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
            if (wallRunning)
            {
                FixHero(HeroController.instance);
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
    }
}
