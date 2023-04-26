using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class GameCanvasManager : NetworkBehaviour
{
    private Vector2 lobbySpawnPos;
    private Coroutine backToLobbyCoroutine;

    [SerializeField] private Transform[] docSpawnPoints;
    private TaskObject[] documents;

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI completedTaskCountText;
    private NetworkVariable<int> completedTaskCount = new NetworkVariable<int>();

    [SerializeField] private GameObject completeBanner;
    private int objectiveCount;

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
    }

    private void Start()
    {
        lobbySpawnPos = FindObjectOfType<PlayerController>().transform.position;
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            completedTaskCount.OnValueChanged += HandleCompletedTaskCount;
        }

        documents = FindObjectsOfType<TaskObject>();
        objectiveCount = documents.Length;

        base.OnNetworkSpawn();
    }

    public void ResetDocuments()
    {
        if (IsServer)
        {
            foreach (var d in documents)
            {
                var newPos = docSpawnPoints[UnityEngine.Random.Range(0, docSpawnPoints.Length)].position;
                newPos.x += UnityEngine.Random.Range(-1f, 1f);
                newPos.y += UnityEngine.Random.Range(-1f, 1f);
                d.RequestReset(newPos);
            }
        }
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

        if (newVal >= objectiveCount) RequestSetCompleteBanner();
    }

    public void RequestSetCompleteBanner(bool active = true)
    {
        if (IsServer)
        {
            completeBanner.SetActive(active);
            SetCompleteBannerClientRpc(active);

            if (backToLobbyCoroutine != null) return;
            backToLobbyCoroutine = StartCoroutine(BackToLobby());
        }
        else
        {
            SetCompleteBannerServerRpc(active);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SetCompleteBannerServerRpc(bool active = true)
    {
        completeBanner.SetActive(active);
    }
    [ClientRpc]
    private void SetCompleteBannerClientRpc(bool active = true)
    {
        completeBanner.SetActive(active);
    }

    private IEnumerator BackToLobby()
    {
        yield return new WaitForSeconds(3f);

        foreach (var p in NetworkPlayerController.Players)
        {
            var spawnPos = lobbySpawnPos;
            spawnPos.x += UnityEngine.Random.Range(-1f, 1f);
            spawnPos.y += UnityEngine.Random.Range(-1f, 1f);
            p.Value.TeleportClientRpc(spawnPos);
        }

        RequestSetCompleteBanner(false);

        if (IsServer) completedTaskCount.Value = 0;
        Network_MainLobbyManager.Instance.GameRestart();
        backToLobbyCoroutine = null;
    }
}