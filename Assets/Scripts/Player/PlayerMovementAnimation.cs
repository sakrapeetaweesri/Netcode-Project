using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementAnimation : MonoBehaviour
{
    [SerializeField] private AnimationClip[] animationClips;
    [SerializeField] private Animator _anim;

    private IPlayerMovement player;

    [SerializeField] private PlayerDirection lastDirection = PlayerDirection.Down;
    [SerializeField] private PlayerDirection currentDirection = PlayerDirection.Down;

    public enum PlayerDirection
    {
        Up,
        Down,
        Left,
        Right,
    }

    private void Start()
    {
        player = GetComponent<IPlayerMovement>();
    }

    public void UpdateAnimation()
    {
        if (player == null) return;

        var velocity = player.GetVelocity();

        if (Mathf.Abs(velocity.x) <= 0.01f && Mathf.Abs(velocity.y) <= 0.01f)
        {
            lastDirection = currentDirection;
            SetAnimation(lastDirection, false);
            return;
        }

        if (velocity.x != 0f)
        {
            currentDirection = velocity.x > 0f ? PlayerDirection.Right : PlayerDirection.Left;
        }
        if (velocity.y != 0f)
        {
            currentDirection = velocity.y > 0f ? PlayerDirection.Up : PlayerDirection.Down;
        }
        SetAnimation(currentDirection, true);

        void SetAnimation(PlayerDirection playerDirection, bool isMoving)
        {
            _anim.Play(animationClips[((int)playerDirection * 2) + (isMoving ? 1 : 0)].name);
        }
    }

    public PlayerDirection GetDirection() => currentDirection;
}