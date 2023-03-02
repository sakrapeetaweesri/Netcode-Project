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
}

public interface IPlayerMovement
{

}