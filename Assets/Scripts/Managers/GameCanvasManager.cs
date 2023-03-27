using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class GameCanvasManager : NetworkBehaviour
{
    [Header("Desk")]
    [SerializeField] private RectTransform deskPanel;
    [SerializeField] private GameObject sign;
    [SerializeField] private RectTransform penRect;
    [SerializeField] private UI_Pen pen;
    [SerializeField] private RectTransform[] penSpawnPoints;
    private Rect[] penSpawnAreas;
    private bool deskActive;
    private Coroutine deskCoroutine;
    private ulong deskInteractor = 99999;

    [Header("Computer")]
    [SerializeField] private RectTransform computerPanel;
    [SerializeField] private TextMeshProUGUI providedText;
    [SerializeField] private TMP_InputField inputText;
    private readonly string TextProvider = "WERTYUIOPASDFGHJKLZXCVBNM wertyuiopasdfghjklzxcvbnm";
    private bool computerActive;
    private Coroutine computerCoroutine;
    private Coroutine computerFinishCoroutine;
    private ulong computerInteractor = 99999;

    [Header("Printer")]
    [SerializeField] private RectTransform printerPanel;
    [SerializeField] private CanvasGroup printerCanvasGroup;
    [SerializeField] private RectTransform printedDoc;
    [SerializeField] private Image[] objectiveOrders;
    [SerializeField] private Color[] orderColors;
    private List<int> printerObjective;
    private List<int> printerPlayerInput;
    private bool printerActive;
    private Coroutine printerCoroutine;
    private Coroutine printerFailedCoroutine;
    private Coroutine printerFinishCoroutine;
    private Coroutine printedDocCoroutine;
    private ulong printerInteractor = 99999;

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI completedTaskCountText;
    private NetworkVariable<int> completedTaskCount = new NetworkVariable<int>();

    public Action<bool> onCanvasEnabled;

    public static GameCanvasManager Instance { get; private set; }
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

        penSpawnAreas = new Rect[penSpawnPoints.Length];
        for (int i = 0; i < penSpawnPoints.Length; i++)
        {
            penSpawnAreas[i] = penSpawnPoints[i].rect;
        }

        printerObjective = new List<int>();
        printerPlayerInput = new List<int>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            completedTaskCount.OnValueChanged += HandleCompletedTaskCount;
        }

        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (deskActive) FinishDeskTask(true);
            if (computerActive) FinishComputerTask(true);
            if (printerActive) FinishPrinterTask(true);
        }
    }

    public void RequestDeskTask(Network_InteractableObject interactor)
    {
        if (deskActive) return;

        if (NetworkPlayerController.Players[interactor.interactorId].holdingTask.Value != TaskState.PlainDocument) return;
        deskInteractor = interactor.interactorId;

        var taskObject = NetworkPlayerController.Players[deskInteractor].transform.GetChild(0).GetComponent<TaskObject>();
        if (taskObject != null)
        {
            taskObject.RequestSetInteractionServerRpc(true);
        }

        if (deskCoroutine != null) StopCoroutine(deskCoroutine);

        deskActive = true;

        int pointIndex = UnityEngine.Random.Range(0, penSpawnAreas.Length);
        var spawnArea = penSpawnAreas[pointIndex];
        var x_Distance = UnityEngine.Random.Range(spawnArea.xMin, spawnArea.xMax);
        var y_Distance = UnityEngine.Random.Range(spawnArea.yMin, spawnArea.yMax);
        var spawnPoint = penSpawnPoints[pointIndex].anchoredPosition;
        spawnPoint.x += x_Distance;
        spawnPoint.y += y_Distance;
        penRect.anchoredPosition = spawnPoint;

        pen.StopAllCoroutines();
        pen.ResetDraggable();
        sign.SetActive(false);

        deskCoroutine = StartCoroutine(Utils.SlideCoroutine(deskPanel, deskPanel.anchoredPosition, Vector2.zero, 20f));

        onCanvasEnabled?.Invoke(true);
    }
    public void FinishDeskTask(bool isCanceled = false)
    {
        if (!NetworkPlayerController.Players.TryGetValue(deskInteractor, out NetworkPlayerController p)) return;
        var taskObject = p.GetComponentInChildren<TaskObject>();
        if (taskObject != null)
        {
            if (!isCanceled) taskObject.SetTaskStateServerRpc(TaskState.SignedDocument);
            taskObject.RequestSetInteractionServerRpc(false);
        }

        deskActive = false;
        if (deskCoroutine != null) StopCoroutine(deskCoroutine);

        deskCoroutine = StartCoroutine(Utils.SlideCoroutine(deskPanel, deskPanel.anchoredPosition, new Vector2(0f, -1100f), 30f));

        deskInteractor = 99999;
        onCanvasEnabled?.Invoke(false);
    }

    public void RequestComputerTask(Network_InteractableObject interactor)
    {
        if (computerActive) return;

        if (NetworkPlayerController.Players[interactor.interactorId].holdingTask.Value != TaskState.SignedDocument) return;
        computerInteractor = interactor.interactorId;

        var taskObject = NetworkPlayerController.Players[computerInteractor].transform.GetChild(0).GetComponent<TaskObject>();
        if (taskObject != null)
        {
            taskObject.RequestSetInteractionServerRpc(true);
        }

        if (computerCoroutine != null) StopCoroutine(computerCoroutine);

        providedText.SetText("");
        inputText.text = "";
        inputText.interactable = true;
        for (int i = 0; i < 10; i++)
        {
            providedText.text += TextProvider[UnityEngine.Random.Range(0, TextProvider.Length)];
        }

        computerActive = true;

        computerCoroutine = StartCoroutine(Utils.SlideCoroutine(computerPanel, computerPanel.anchoredPosition, Vector2.zero, 20f));

        onCanvasEnabled?.Invoke(true);
    }
    public void FinishComputerTask(bool isCanceled = false)
    {
        if (!NetworkPlayerController.Players.TryGetValue(computerInteractor, out NetworkPlayerController p)) return;
        var taskObject = p.GetComponentInChildren<TaskObject>();
        if (taskObject != null)
        {
            if (!isCanceled) taskObject.SetTaskStateServerRpc(TaskState.TypedDocument);
            taskObject.RequestSetInteractionServerRpc(false);
        }

        computerActive = false;
        if (computerCoroutine != null) StopCoroutine(computerCoroutine);

        computerCoroutine = StartCoroutine(Utils.SlideCoroutine(computerPanel, computerPanel.anchoredPosition, new Vector2(0f, -1100f), 30f));

        computerInteractor = 99999;
        onCanvasEnabled?.Invoke(false);
    }
    public void InputValidate()
    {
        if (!computerActive) return;
        if (computerFinishCoroutine != null) return;

        Debug.Log(providedText.text);
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

    public void RequestPrinterTask(Network_InteractableObject interactor)
    {
        if (printerActive) return;

        if (NetworkPlayerController.Players[interactor.interactorId].holdingTask.Value != TaskState.TypedDocument) return;
        printerInteractor = interactor.interactorId;

        var taskObject = NetworkPlayerController.Players[printerInteractor].transform.GetChild(0).GetComponent<TaskObject>();
        if (taskObject != null)
        {
            taskObject.RequestSetInteractionServerRpc(true);
        }

        if (printerCoroutine != null) StopCoroutine(printerCoroutine);

        if (printedDocCoroutine != null) StopCoroutine(printedDocCoroutine);
        printedDoc.anchoredPosition = new Vector2(254, -32f);

        for (int i = 0; i < 5; i++)
        {
            int colorIndex = UnityEngine.Random.Range(0, 4);
            printerObjective.Add(colorIndex);
            objectiveOrders[i].color = orderColors[colorIndex];
        }

        printerCanvasGroup.blocksRaycasts = true;
        printerActive = true;

        printerCoroutine = StartCoroutine(Utils.SlideCoroutine(printerPanel, printerPanel.anchoredPosition, Vector2.zero, 20f));

        onCanvasEnabled?.Invoke(true);
    }
    public void FinishPrinterTask(bool isCanceled = false)
    {
        if (!NetworkPlayerController.Players.TryGetValue(printerInteractor, out NetworkPlayerController p)) return;
        var taskObject = p.GetComponentInChildren<TaskObject>();
        if (taskObject != null)
        {
            if (!isCanceled) taskObject.SetTaskStateServerRpc(TaskState.CopiedDocument);
            taskObject.RequestSetInteractionServerRpc(false);
        }

        printerObjective.Clear();
        printerPlayerInput.Clear();

        printerActive = false;
        if (printerCoroutine != null) StopCoroutine(printerCoroutine);
        if (printerFinishCoroutine != null) StopCoroutine(printerFinishCoroutine);

        printerCoroutine = StartCoroutine(Utils.SlideCoroutine(printerPanel, printerPanel.anchoredPosition, new Vector2(0f, -1100f), 30f));

        printerInteractor = 99999;
        onCanvasEnabled?.Invoke(false);
    }
    public void PrinterButtonPress(int index)
    {
        if (!printerActive || printerFailedCoroutine != null || printerFinishCoroutine != null) return;

        printerPlayerInput.Add(index);
        Debug.Log(printerPlayerInput.Count);

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

    [ServerRpc(RequireOwnership = false)]
    public void CountCompletedTaskServerRpc()
    {
        if (!IsServer) return;

        completedTaskCount.Value++;
    }
    private void HandleCompletedTaskCount(int oldVal, int newVal)
    {
        completedTaskCountText.SetText(newVal.ToString());
    }
}