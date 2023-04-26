using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Task_Copy : NetworkBehaviour
{
    [SerializeField] private RectTransform printerPanel;
    [SerializeField] private CanvasGroup printerCanvasGroup;
    [SerializeField] private RectTransform printedDoc;
    [SerializeField] private Image[] objectiveOrders;
    [SerializeField] private Color[] orderColors;
    [SerializeField] private float interactDistance;
    [SerializeField] private SpriteRenderer errorBubble;
    private List<int> printerObjective = new List<int>();
    private List<int> printerPlayerInput = new List<int>();
    private bool printerActive;
    private Coroutine printerCoroutine;
    private Coroutine printerFailedCoroutine;
    private Coroutine printerFinishCoroutine;
    private Coroutine printedDocCoroutine;
    public NetworkPlayerController playerInteracting;

    public NetworkVariable<bool> isError = new NetworkVariable<bool>();

    public static Task_Copy Instance { get; private set; }
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

    public override void OnNetworkSpawn()
    {
        isError.OnValueChanged += HandleErrorStateChanged;

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!printerActive)
        {
            playerInteracting = NetworkManager.SpawnManager?.GetLocalPlayerObject().GetComponent< NetworkPlayerController>();
            if (playerInteracting == null) return;

            if ((transform.position - playerInteracting.transform.position).sqrMagnitude <= interactDistance)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (isError.Value)
                        Task_CopyError.Instance.ShowErrorTask();
                    else
                        RequestPrinterTask();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Q) && printerFinishCoroutine == null && printerFailedCoroutine == null
                && Task_CopyError.Instance.errorCoroutine == null)
            {
                FinishPrinterTask(true);
            }
        }
    }

    public void RequestPrinterTask()
    {
        if (printerActive) return;

        var taskObject = playerInteracting.GetComponentInChildren<TaskObject>();
        if (taskObject != null)
        {
            if (taskObject.taskState.Value != TaskState.TypedDocument) return;
            taskObject.RequestSetInteractionServerRpc(true);
        }
        else return;

        if (printerCoroutine != null) StopCoroutine(printerCoroutine);

        if (printedDocCoroutine != null) StopCoroutine(printedDocCoroutine);
        printedDoc.anchoredPosition = new Vector2(254, -32f);

        for (int i = 0; i < 5; i++)
        {
            int colorIndex = Random.Range(0, 4);
            printerObjective.Add(colorIndex);
            objectiveOrders[i].color = orderColors[colorIndex];
        }

        printerCanvasGroup.blocksRaycasts = true;
        printerActive = true;
        playerInteracting.SetBlockMovement(true);

        printerCoroutine = StartCoroutine(Utils.SlideCoroutine(printerPanel, printerPanel.anchoredPosition, Vector2.zero, 20f));
    }
    public void FinishPrinterTask(bool isCanceled = false)
    {
        if (!printerActive) return;

        var taskObject = playerInteracting.GetComponentInChildren<TaskObject>();
        if (taskObject != null)
        {
            if (!isCanceled)
            {
                taskObject.SetTaskStateServerRpc(TaskState.CopiedDocument);
                Task_CopyError.Instance.ManageError();
            }
            taskObject.RequestSetInteractionServerRpc(false);
        }

        printerObjective.Clear();
        printerPlayerInput.Clear();

        printerCanvasGroup.blocksRaycasts = false;
        printerActive = false;
        playerInteracting.SetBlockMovement(false);

        if (printerCoroutine != null) StopCoroutine(printerCoroutine);
        if (printerFinishCoroutine != null) StopCoroutine(printerFinishCoroutine);

        printerCoroutine = StartCoroutine(Utils.SlideCoroutine(printerPanel, printerPanel.anchoredPosition, new Vector2(0f, -1100f), 30f));
    }
    public void PrinterButtonPress(int index)
    {
        if (!printerActive || printerFailedCoroutine != null || printerFinishCoroutine != null) return;

        printerPlayerInput.Add(index);

        for (int i = 0; i < printerObjective.Count; i++)
        {
            if (i >= printerPlayerInput.Count) return;

            if (printerPlayerInput[i] != printerObjective[i])
            {
                printerPlayerInput.Clear();
                printerFailedCoroutine = StartCoroutine(PrinterInputFailed());
                return;
            }
        }

        printerFinishCoroutine = StartCoroutine(PrinterInputSucceeded());
    }
    private IEnumerator PrinterInputFailed()
    {
        printerCanvasGroup.interactable = false;
        yield return new WaitForSeconds(0.1f);
        printerCanvasGroup.interactable = true;
        yield return new WaitForSeconds(0.1f);
        printerCanvasGroup.interactable = false;
        yield return new WaitForSeconds(0.1f);
        printerCanvasGroup.interactable = true;

        printerFailedCoroutine = null;
    }
    private IEnumerator PrinterInputSucceeded()
    {
        printerCanvasGroup.blocksRaycasts = false;
        yield return new WaitForSeconds(0.2f);
        printedDocCoroutine = StartCoroutine(Utils.SlideCoroutine(printedDoc, printedDoc.anchoredPosition, new Vector2(254, -300f), 7f));
        yield return new WaitForSeconds(1f);
        FinishPrinterTask();
        printerFinishCoroutine = null;
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
    }
}