using System;
using UnityEngine;
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
    public void SwitchTelephoneInteraction(bool networkConnected)
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
        if (!Network_MainLobbyManager.NetworkConnected.Value)
        {
            LastPosition = offlinePlayer.transform.position;
            offlinePlayer.BlockMovement = true;
        }

        canvas.enabled = true;
        CurrentCanvas = canvas;
    }
    /// <summary>
    /// Sets the given canvas to be deactive.
    /// </summary>
    /// <param name="canvas">The canvas to be deacivated.</param>
    public void DeactivateCanvas(Canvas canvas)
    {
        if (!Network_MainLobbyManager.NetworkConnected.Value)
        {
            offlinePlayer.BlockMovement = false;
        }

        canvas.enabled = false;
        CurrentCanvas = null;
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

        // Manages the UI;
        DeactivateCanvas(CurrentCanvas);
        mainCanvas.enabled = true;
        codeText.text += RelayManager.RelayCode;

        // Disables the offline player.
        offlinePlayer.enabled = false;
        offlinePlayer.gameObject.SetActive(false);
    }
    /// <summary>
    /// Joins an existing relay room and manages the player.
    /// </summary>
    public async void JoinRoom()
    {
        // Creates a relay room.
        if (!await RelayManager.Instance.JoinRelay(codeInput.text)) return;

        // Manages the UI;
        DeactivateCanvas(CurrentCanvas);
        mainCanvas.enabled = true;
        codeText.text += codeInput.text;

        // Disables the offline player.
        offlinePlayer.enabled = false;
        offlinePlayer.gameObject.SetActive(false);
    }
}