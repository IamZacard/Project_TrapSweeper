using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator; // Reference to Animator
    [SerializeField] private SpriteRenderer spriteRenderer; // Reference to SpriteRenderer

    public void AnimateMove(Vector2 direction)
    {
        // Flip the sprite based on movement direction
        if (direction.x > 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (direction.x < 0)
        {
            spriteRenderer.flipX = false;
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
}
