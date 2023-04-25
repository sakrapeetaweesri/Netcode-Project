using System.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class Task_Type : NetworkBehaviour
{
    [SerializeField] private RectTransform computerPanel;
    [SerializeField] private TextMeshProUGUI providedText;
    [SerializeField] private TMP_InputField inputText;
    [SerializeField] private float interactDistance;
    [SerializeField] private SpriteRenderer errorBubble;
    [SerializeField] private SpriteRenderer computerRenderer;
    [SerializeField] private Sprite[] computerSprites;
    private readonly string TextProvider = "WERTYUOPASDFGHJKLZXCVBNM wertyuiopasdfghjkzxcvbnm";
    private bool computerActive;
    private Coroutine computerCoroutine;
    private Coroutine computerFinishCoroutine;
    public NetworkPlayerController playerInteracting;

    private int providerLength = 10;

    public NetworkVariable<bool> isError = new NetworkVariable<bool>();

    public static Task_Type Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            return;
        }
    }

    private void Update()
    {
        if (!computerActive)
        {
            playerInteracting = NetworkManager.SpawnManager?.GetLocalPlayerObject().GetComponent<NetworkPlayerController>();
            if (playerInteracting == null) return;

            if ((transform.position - playerInteracting.transform.position).sqrMagnitude <= interactDistance)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    RequestComputerTask();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                FinishComputerTask(true);
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        isError.OnValueChanged += HandleErrorStateChanged;

        base.OnNetworkSpawn();
    }
    public override void OnNetworkDespawn()
    {
        isError.OnValueChanged -= HandleErrorStateChanged;

        base.OnNetworkDespawn();
    }

    public void RequestComputerTask()
    {
        if (computerActive) return;

        void SetComputer()
        {
            if (computerCoroutine != null) StopCoroutine(computerCoroutine);

            computerActive = true;
            playerInteracting.GetComponent<NetworkPlayerController>().SetBlockMovement(true);

            computerCoroutine = StartCoroutine(Utils.SlideCoroutine(computerPanel, computerPanel.anchoredPosition, Vector2.zero, 20f));
        }

        if (isError.Value)
        {
            Task_TypeError.Instance.ShowErrorTask();

            if (computerCoroutine != null) StopCoroutine(computerCoroutine);

            SetComputer();
            return;
        }

        var taskObject = playerInteracting.GetComponentInChildren<TaskObject>();
        if (taskObject != null)
        {
            if (taskObject.taskState.Value != TaskState.SignedDocument) return;
            taskObject.RequestSetInteractionServerRpc(true);
        }
        else return;

        SetComputer();

        providedText.SetText("");
        inputText.text = "";
        inputText.interactable = true;
        for (int i = 0; i < providerLength; i++)
        {
            providedText.text += TextProvider[Random.Range(0, TextProvider.Length)];
        }
    }
    public void FinishComputerTask(bool isCanceled = false)
    {
        if (!computerActive) return;

        var taskObject = playerInteracting.GetComponentInChildren<TaskObject>();
        if (taskObject != null)
        {
            if (!isCanceled) taskObject.SetTaskStateServerRpc(TaskState.TypedDocument);
            taskObject.RequestSetInteractionServerRpc(false);
        }

        computerActive = false;
        playerInteracting.GetComponent<NetworkPlayerController>().SetBlockMovement(false);
        playerInteracting = null;

        if (computerCoroutine != null) StopCoroutine(computerCoroutine);
        computerCoroutine = StartCoroutine(Utils.SlideCoroutine(computerPanel, computerPanel.anchoredPosition, new Vector2(0f, -1100f), 30f));
    }
    public void InputValidate()
    {
        if (!computerActive) return;
        if (computerFinishCoroutine != null) return;
        if (inputText.text.Length < providerLength) return;

        if (inputText.text == providedText.text)
        {
            computerFinishCoroutine = StartCoroutine(ComputerFinishDelay());
        }
    }
    private IEnumerator ComputerFinishDelay()
    {
        inputText.interactable = false;
        yield return new WaitForSeconds(1f);
        FinishComputerTask();
        computerFinishCoroutine = null;
    }

    public void RequestSetErrorState(bool state)
    {
        if (IsServer)
        {
            isError.Value = state;
        }
        else
        {
            SetErrorStateServerRpc(state);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetErrorStateServerRpc(bool state)
    {
        isError.Value = state;
    }
    private void HandleErrorStateChanged(bool oldState, bool newState)
    {
        errorBubble.enabled = newState;
        RequestSetRenderer(newState);
        if (newState) Task_TypeError.Instance.ShowErrorTask();
    }
    public void RequestSetRenderer(bool state)
    {
        if (IsServer)
        {
            computerRenderer.sprite = computerSprites[state ? 1 : 0];
        }
        else
        {
            SetRendererServerRpc(state);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetRendererServerRpc(bool state)
    {
        computerRenderer.sprite = computerSprites[state ? 1 : 0];
    }
}