using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IPlayerMovement
{
    private float movementSpeed = 7f;
    [SerializeField] private Rigidbody2D _rigidbody2D;
    private bool blockMovement;

    private void Start()
    {
        if (MainLobbyManager.Instance == null) return;
        MainLobbyManager.Instance.onCanvasEnabled += SetBlockMovement;
    }

    private void OnEnable()
    {
        if (MainLobbyManager.Instance == null) return;
        MainLobbyManager.Instance.onCanvasEnabled += SetBlockMovement;
    }
    private void OnDisable()
    {
        if (MainLobbyManager.Instance == null) return;
        MainLobbyManager.Instance.onCanvasEnabled -= SetBlockMovement;
    }
    private void SetBlockMovement(bool state)
    {
        blockMovement = state;
    }

    /// <summary>
    /// Controls the player movement.
    /// </summary>
    private void FixedUpdate()
    {
        if (blockMovement)
        {
            _rigidbody2D.velocity = Vector2.zero;
            return;
        }

        // Movement
        Movement();
    }
    /// <summary>
    /// Handles the player movement.
    /// </summary>
    private void Movement()
    {
        float x_Movement = Input.GetAxisRaw("Horizontal");
        float y_Movement = Input.GetAxisRaw("Vertical");

        Vector2 moveDir = new Vector2(x_Movement, y_Movement).normalized * movementSpeed;
        _rigidbody2D.velocity = moveDir;
    }

    public Vector2 GetVelocity()
    {
        return _rigidbody2D.velocity;
    }

    [SerializeField] private AnimationClip[] animationClips;
    [SerializeField] private Animator _anim;

    [SerializeField] private PlayerDirection lastDirection = PlayerDirection.Down;
    [SerializeField] private PlayerDirection currentDirection = PlayerDirection.Down;

    private void Update()
    {
        UpdateAnimation();
    }
    private void UpdateAnimation()
    {
        var velocity = GetVelocity();

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

public enum PlayerDirection
{
    Up,
    Down,
    Left,
    Right,
}

public interface IPlayerMovement
{
    Vector2 GetVelocity();
}