using System;
using System.Collections;
using System.Reflection;
using Modding;
using UnityEngine;
using UnityEngine.UI;

namespace RandomizerMod.Components
{
    internal class StickyWall : MonoBehaviour
    {
        private static MethodInfo hcWallJump = typeof(HeroController).GetMethod("DoWallJump", BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo hcCanDoubleJump = typeof(HeroController).GetMethod("CanDoubleJump", BindingFlags.Instance | BindingFlags.NonPublic);

        private RectTransform canvasRect;
        private BoxCollider2D box;
        private bool wallRunning;

        public void Awake()
        {
            // Stuff breaks really hard if we try to remain wall running after getting hit
            ModHooks.Instance.TakeDamageHook += DamageTaken;

            // Store box collider because it has the size of the wall
            box = GetComponent<BoxCollider2D>();

            // Create canvas for drawing the rectangle
#warning TODO: Figure out how the hell mesh renderers work and/or switch this to a sprite renderer
            GameObject parent = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(Screen.width, Screen.height));
            GameObject canvas = CanvasUtil.CreateImagePanel(parent, CanvasUtil.NullSprite(new byte[] { 0x88, 0xFF, 0x88, 0xFF }), GetRectData());
            canvas.GetComponent<Image>().preserveAspect = false;

            canvasRect = canvas.GetComponent<RectTransform>();
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
            // Move the graphic to the correct location every frame
            CanvasUtil.RectData rect = GetRectData();
            canvasRect.anchorMax = rect.AnchorMax;
            canvasRect.anchorMin = rect.AnchorMin;
            canvasRect.pivot = rect.AnchorPivot;
            canvasRect.sizeDelta = rect.RectSizeDelta;
            canvasRect.anchoredPosition = rect.AnchorPosition;

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

                hc.gameObject.transform.SetPositionY(hc.gameObject.transform.position.y + (hc.RUN_SPEED * Time.deltaTime));
                hc.GetComponent<tk2dSpriteAnimator>().Play("Run");
            }
            else if (hc.transform.position.y > (transform.position.y - (box.size.y / 2)) && GameManager.instance.inputHandler.inputActions.right.IsPressed)
            {
                hc.FaceRight();
                hc.gameObject.transform.SetPositionX(transform.position.x + box.size.x + .15f);

                hc.gameObject.transform.SetPositionY(hc.gameObject.transform.position.y - (hc.RUN_SPEED * Time.deltaTime));
                hc.GetComponent<tk2dSpriteAnimator>().Play("Run");
            }
            else
            {
                hc.GetComponent<tk2dSpriteAnimator>().Play("Idle");
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
            hc.GetComponent<HeroAnimationController>().PlayClip("Run");
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

        // Returns the world location of the collider as screen coordinates
        private CanvasUtil.RectData GetRectData()
        {
            Vector2 camMin = GameCameras.instance.tk2dCam.ScreenCamera.WorldToScreenPoint(box.bounds.min);
            Vector2 camMax = GameCameras.instance.tk2dCam.ScreenCamera.WorldToScreenPoint(box.bounds.max);

            return new CanvasUtil.RectData(
                Vector2.zero,
                Vector2.zero,
                new Vector2(camMin.x / Screen.width, camMin.y / Screen.height),
                new Vector2(camMax.x / Screen.width, camMax.y / Screen.height));
        }

        // Undo all the wall run stuff on getting hit
        private int DamageTaken(ref int hazardType, int damage)
        {
            FixHero(HeroController.instance);
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
