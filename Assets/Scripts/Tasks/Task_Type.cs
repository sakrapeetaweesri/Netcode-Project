using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class Task_Type : NetworkBehaviour
{
    [SerializeField] private RectTransform computerPanel;
    [SerializeField] private TextMeshProUGUI providedText;
    [SerializeField] private TMP_InputField inputText;
    [SerializeField] private float interactDistance;
    private readonly string TextProvider = "WERTYUOPASDFGHJKLZXCVBNM wertyuiopasdfghjkzxcvbnm";
    private bool computerActive;
    private Coroutine computerCoroutine;
    private Coroutine computerFinishCoroutine;
    private NetworkObject playerInteracting;

    private int providerLength = 10;

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
            playerInteracting = NetworkManager.SpawnManager?.GetLocalPlayerObject();
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

    public void RequestComputerTask()
    {
        if (computerActive) return;

        var taskObject = playerInteracting.GetComponentInChildren<TaskObject>();
        if (taskObject != null)
        {
            if (taskObject.taskState.Value != TaskState.SignedDocument) return;
            taskObject.RequestSetInteractionServerRpc(true);
        }
        else return;

        if (computerCoroutine != null) StopCoroutine(computerCoroutine);

        computerActive = true;
        playerInteracting.GetComponent<NetworkPlayerController>().SetBlockMovement(true);

        providedText.SetText("");
        inputText.text = "";
        inputText.interactable = true;
        for (int i = 0; i < providerLength; i++)
        {
            providedText.text += TextProvider[UnityEngine.Random.Range(0, TextProvider.Length)];
        }

        computerCoroutine = StartCoroutine(Utils.SlideCoroutine(computerPanel, computerPanel.anchoredPosition, Vector2.zero, 20f));
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
}