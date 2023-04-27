using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class Wardrobe : NetworkBehaviour
{
    [SerializeField] private GameObject chooseScreen;
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Button[] characterButtons;
    [SerializeField] private Button selectButton;
    [SerializeField] private float interactDistance;
    private PlayerController privatePlayerInteracting;
    private NetworkObject playerInteracting;
    private bool isActive;
    private int characterSelected;

    public static Wardrobe Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        privatePlayerInteracting = FindObjectOfType<PlayerController>();
    }

    private void Update()
    {
        if (!isActive)
        {
            playerInteracting = NetworkManager.SpawnManager?.GetLocalPlayerObject();
            if (playerInteracting != null) privatePlayerInteracting = null;

            if (Input.GetKeyDown(KeyCode.E))
            {
                if ((privatePlayerInteracting != null &&
                    (transform.position - privatePlayerInteracting.transform.position).sqrMagnitude <= interactDistance) ||
                    (playerInteracting != null &&
                    (transform.position - playerInteracting.transform.position).sqrMagnitude <= interactDistance))
                ToggleScreen(true);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q))
            {
                ToggleScreen(false);
            }
        }
    }

    public void ToggleScreen(bool active)
    {
        isActive = active;
        mainCanvas.enabled = active;
        chooseScreen.SetActive(active);
        UpdateUI();

        if (privatePlayerInteracting != null)
        {
            privatePlayerInteracting.SetBlockMovement(active);
        }
        else
        {
            playerInteracting.GetComponent<NetworkPlayerController>().SetBlockMovement(active);
        }
    }

    public void ChooseCharacter(int index)
    {
        characterSelected = index;

        UpdateUI();
    }
    public void SelectCharacter()
    {
        if (privatePlayerInteracting != null)
        {
            privatePlayerInteracting.characterId = characterSelected;
        }
        else
        {
            playerInteracting.GetComponent<NetworkPlayerController>().SetCharacter(characterSelected);
        }

        UpdateUI();
    }
    public void UpdateUI()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            characterButtons[i].interactable = i != characterSelected;
        }

        if (privatePlayerInteracting != null)
        {
            selectButton.interactable = characterSelected != privatePlayerInteracting.characterId;
        }
        else
        {
            selectButton.interactable = characterSelected != playerInteracting.GetComponent<NetworkPlayerController>().characterId.Value;
        }
    }
}