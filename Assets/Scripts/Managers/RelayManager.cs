using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class RelayManager : MonoBehaviour
{
    [SerializeField] private Image playImage;

    public static Vector2 lastPosition;
    public static int characterId;
    public static string RelayCode { get; private set; }
    public static bool RelayConnected { get; private set; }

    public static RelayManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private async void Start()
    {
        await UnityServices.InitializeAsync();  // Initializes the unity services.
    }

    /// <summary>
    /// Signs in to the game anonymously.
    /// This is expected to be called by the UI button.
    /// </summary>
    public async void SignIn()
    {
        playImage.raycastTarget = false;

        try
        {
            // Logs in anonymously.
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        // Logs in case there's an error.
        catch (Exception e)
        {
            Debug.LogWarning(e);

            playImage.raycastTarget = true;
            return;
        }

        Debug.Log($"Signed in: {AuthenticationService.Instance.PlayerId}");

        // Transfers to the main lobby.
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Creates the relay.
    /// </summary>
    public async Task CreateRelay()
    {
        try
        {
            // Creates relay allocation. Takes 'max players host excluded' as an argument.
            Allocation alloc = await RelayService.Instance.CreateAllocationAsync(3);

            // Gets the join code for the lobby.
            RelayCode = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
            Debug.Log($"Join code: {RelayCode}");

            // Starts NetworkManager server.
            RelayServerData relayServerData = new RelayServerData(alloc, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            RelayConnected = true;

            NetworkManager.Singleton.StartHost();
        }
        // Logs incase there's an error.
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    /// <summary>
    /// Joins an existing relay with the corresponding code.
    /// </summary>
    /// <param name="code">The relay code.</param>
    public async Task<bool> JoinRelay(string code)
    {
        try
        {
            // Attempts to join a relay with the given code.
            JoinAllocation joinAlloc = await RelayService.Instance.JoinAllocationAsync(code);

            // Starts NetworkManager server.
            RelayServerData relayServerData = new RelayServerData(joinAlloc, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            RelayCode = code;
            RelayConnected = true;

            NetworkManager.Singleton.StartClient();

            return true;
        }
        // Logs incase there's an error.
        catch (Exception e)
        {
            Debug.LogWarning(e);

            return false;
        }
    }
}