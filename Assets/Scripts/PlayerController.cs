using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour, IPlayerMovement
{
    private float movementSpeed = 7f;
    [SerializeField] private Rigidbody2D _rigidbody2D;
    public bool BlockMovement { get; set; }

    /// <summary>
    /// Controls the player movement.
    /// </summary>
    private void FixedUpdate()
    {
        if (BlockMovement) return;

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
}

public interface IPlayerMovement
{

}