using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using TMPro;

public class MainLobbyManager : MonoBehaviour
{
    public Canvas CurrentCanvas { get; private set; }
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private GameObject joinOrCreatePanel;
    [SerializeField] private GameObject relayRoomPanel;

    [SerializeField] private TextMeshProUGUI codeText;
    [SerializeField] private TMP_InputField codeInput;

    [SerializeField] private PlayerController offlinePlayer;

    public Action<bool> onCanvasEnabled;

    public Vector2 LastPosition { get; private set; }

    public static MainLobbyManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Switches the telephone interactions.
    /// </summary>
    private void SwitchObjectInteraction(bool networkConnected)
    {
        relayRoomPanel.SetActive(networkConnected);
        joinOrCreatePanel.SetActive(!networkConnected);
    }

    /// <summary>
    /// Sets the given canvas to be active.
    /// </summary>
    /// <param name="canvas">The canvas to be acivated.</param>
    public void ActivateCanvas(Canvas canvas)
    {
        RelayManager.lastPosition = offlinePlayer.transform.position;

        canvas.enabled = true;
        CurrentCanvas = canvas;

        onCanvasEnabled?.Invoke(true);
    }
    /// <summary>
    /// Sets the given canvas to be deactive.
    /// </summary>
    /// <param name="canvas">The canvas to be deacivated.</param>
    public void DeactivateCanvas(Canvas canvas)
    {
        canvas.enabled = false;
        CurrentCanvas = null;

        onCanvasEnabled?.Invoke(false);
    }

    private void Update()
    {
        if (CurrentCanvas == null) return;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            DeactivateCanvas(CurrentCanvas);
        }
    }

    /// <summary>
    /// Creates a relay room and manages the player.
    /// </summary>
    public async void CreateRoom()
    {
        // Creates a relay room.
        await RelayManager.Instance.CreateRelay();

        ManageRelayContent();
    }
    /// <summary>
    /// Joins an existing relay room and manages the player.
    /// </summary>
    public async void JoinRoom()
    {
        // Creates a relay room.
        if (!await RelayManager.Instance.JoinRelay(codeInput.text)) return;

        ManageRelayContent();
    }
    private void ManageRelayContent()
    {
        SwitchObjectInteraction(true);

        // Manages the UI;
        mainCanvas.enabled = true;
        codeText.text += RelayManager.RelayCode;

        // Disables the offline player.
        offlinePlayer.enabled = false;
        offlinePlayer.gameObject.SetActive(false);
    }
}