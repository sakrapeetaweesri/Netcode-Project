using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Task_Sign : NetworkBehaviour
{
    [SerializeField] private RectTransform deskPanel;
    [SerializeField] private GameObject sign;
    [SerializeField] private RectTransform penRect;
    [SerializeField] private UI_Pen pen;
    [SerializeField] private RectTransform[] penSpawnPoints;
    [SerializeField] private float interactDistance;
    private Rect[] penSpawnAreas;
    private Coroutine deskCoroutine;
    private bool deskActive;
    private NetworkObject playerInteracting;

    public static Task_Sign Instance { get; private set; }
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
    }

    private void Update()
    {
        if (!deskActive)
        {
            playerInteracting = NetworkManager.SpawnManager?.GetLocalPlayerObject();
            if (playerInteracting == null) return;

            if ((transform.position - playerInteracting.transform.position).sqrMagnitude <= interactDistance)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    RequestDeskTask();
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                FinishDeskTask(true);
            }
        }
    }

    public void RequestDeskTask()
    {
        if (deskActive) return;

        var taskObject = playerInteracting.GetComponentInChildren<TaskObject>();
        if (taskObject != null)
        {
            if (taskObject.taskState.Value != TaskState.PlainDocument) return;
            taskObject.RequestSetInteractionServerRpc(true);
        }
        else return;

        if (deskCoroutine != null) StopCoroutine(deskCoroutine);

        deskActive = true;
        playerInteracting.GetComponent<NetworkPlayerController>().SetBlockMovement(true);

        int pointIndex = Random.Range(0, penSpawnAreas.Length);
        var spawnArea = penSpawnAreas[pointIndex];
        var x_Distance = Random.Range(spawnArea.xMin, spawnArea.xMax);
        var y_Distance = Random.Range(spawnArea.yMin, spawnArea.yMax);
        var spawnPoint = penSpawnPoints[pointIndex].anchoredPosition;
        spawnPoint.x += x_Distance;
        spawnPoint.y += y_Distance;
        penRect.anchoredPosition = spawnPoint;

        pen.StopAllCoroutines();
        pen.ResetDraggable();
        sign.SetActive(false);

        deskCoroutine = StartCoroutine(Utils.SlideCoroutine(deskPanel, deskPanel.anchoredPosition, Vector2.zero, 20f));
    }
    public void FinishDeskTask(bool isCanceled = false)
    {
        if (!deskActive) return;

        var taskObject = playerInteracting.GetComponentInChildren<TaskObject>();
        if (taskObject != null)
        {
            if (!isCanceled) taskObject.SetTaskStateServerRpc(TaskState.SignedDocument);
            taskObject.RequestSetInteractionServerRpc(false);
        }

        deskActive = false;
        playerInteracting.GetComponent<NetworkPlayerController>().SetBlockMovement(false);
        playerInteracting = null;

        if (deskCoroutine != null) StopCoroutine(deskCoroutine);

        deskCoroutine = StartCoroutine(Utils.SlideCoroutine(deskPanel, deskPanel.anchoredPosition, new Vector2(0f, -1100f), 30f));
    }
}