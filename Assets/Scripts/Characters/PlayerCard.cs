using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private GameObject characterVisuals;
    [SerializeField] private Image characterIconImage;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private GameObject kickButton;

    public void UpdateDisplay(PlayerState playerState)
    {
        characterIconImage.enabled = true;
        characterIconImage.sprite = GameAssets.i.CharacterProfiles[playerState.CharacterID];
        characterIconImage.SetNativeSize();

        playerNameText.SetText($"Player {playerState.ClientID}");
        characterNameText.SetText(playerState.CharacterID == 0 ? "Office Worker" : "IT Guy");

        characterVisuals.SetActive(true);
    }
    public void DisableVisuals()
    {
        characterVisuals.SetActive(false);
    }
    public void DisplayKickButton()
    {
        kickButton.SetActive(true);
    }
}