using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator; // Reference to Animator
    [SerializeField] private SpriteRenderer spriteRenderer; // Reference to SpriteRenderer
    [SerializeField] private Transform castPoint; // Reference to CastPoint

    private Vector3 castPointOriginalPosition;

    private void Awake()
    {
        // Store the original local position of castPoint
        if (castPoint != null)
        {
            castPointOriginalPosition = castPoint.localPosition;
        }
    }

    public void AnimateMove(Vector2 direction)
    {
        // Flip the sprite based on movement direction
        if (direction.x > 0)
        {
            spriteRenderer.flipX = true;
            FlipCastPoint(false);
        }
        else if (direction.x < 0)
        {
            spriteRenderer.flipX = false;
            FlipCastPoint(true);
        }

        // Trigger movement animation
        animator.SetTrigger("Move");
    }

    public void AnimateIdle()
    {
        animator.SetTrigger("Idle");
    }

    public void SetInvincibility(bool isInvincible)
    {
        animator.SetBool("Invincible", isInvincible);
    }

    private void FlipCastPoint(bool flipX)
    {
        if (castPoint != null)
        {
            // Mirror the local position of castPoint along the X-axis
            float flippedX = flipX ? -Mathf.Abs(castPointOriginalPosition.x) : Mathf.Abs(castPointOriginalPosition.x);
            castPoint.localPosition = new Vector3(flippedX, castPointOriginalPosition.y, castPointOriginalPosition.z);
        }
    }
}
