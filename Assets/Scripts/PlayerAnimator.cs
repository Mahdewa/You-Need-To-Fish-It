using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Animation parameters - gunakan int untuk state yang lebih reliable
    private const string ANIM_STATE = "AnimState"; // Int parameter
    private const string ANIM_SPEED = "Speed";
    private const string ANIM_IS_ON_BOAT = "IsOnBoat";

    // State values
    private const int STATE_IDLE = 0;
    private const int STATE_WALK = 1;
    private const int STATE_BAITING = 2;
    private const int STATE_FISHING_START = 3;
    private const int STATE_FISHING_WAITING = 4;
    private const int STATE_FISHING_END = 5;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
        {
            Debug.LogWarning("Animator component tidak ditemukan pada Player!");
        }
    }

    // --- GERAKAN ANIMASI ---
    public void PlayIdle()
    {
        if (animator != null)
        {
            animator.SetInteger(ANIM_STATE, STATE_IDLE);
            animator.SetFloat(ANIM_SPEED, 0f);
            Debug.Log("PlayIdle");
        }
    }

    public void PlayWalk(float moveInput)
    {
        if (animator != null)
        {
            animator.SetFloat(ANIM_SPEED, Mathf.Abs(moveInput));
            if (Mathf.Abs(moveInput) > 0.01f)
            {
                animator.SetInteger(ANIM_STATE, STATE_WALK);
            }
        }
    }

    public void SetBoatState(bool isOnBoat)
    {
        if (animator != null)
        {
            animator.SetBool(ANIM_IS_ON_BOAT, isOnBoat);
        }
    }

    // --- BAITING ANIMASI ---
    public void PlayBaiting()
    {
        if (animator != null)
        {
            animator.SetInteger(ANIM_STATE, STATE_BAITING);
            Debug.Log("PlayBaiting");
        }
    }

    // --- FISHING ANIMASI ---
    public void PlayFishingStart()
    {
        if (animator != null)
        {
            animator.SetInteger(ANIM_STATE, STATE_FISHING_START);
            Debug.Log("PlayFishingStart");
        }
    }

    public void PlayFishingWaiting()
    {
        if (animator != null)
        {
            animator.SetInteger(ANIM_STATE, STATE_FISHING_WAITING);
            Debug.Log("PlayFishingWaiting");
        }
    }

    public void PlayFishingEnd()
    {
        if (animator != null)
        {
            animator.SetInteger(ANIM_STATE, STATE_FISHING_END);
            Debug.Log("PlayFishingEnd");
        }
    }

    // --- FLIP SPRITE ---
    public void FlipSprite(float moveInput)
    {
        if (spriteRenderer != null)
        {
            if (moveInput > 0)
                spriteRenderer.flipX = false;
            else if (moveInput < 0)
                spriteRenderer.flipX = true;
        }
    }
}

