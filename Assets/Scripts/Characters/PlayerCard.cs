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

    public void UpdateDisplay(PlayerState playerState)
    {
        characterIconImage.enabled = true;

        playerNameText.SetText($"Player {playerState.ClientID}");
        characterNameText.SetText("Office Worker");

        characterVisuals.SetActive(true);
    }

    public void DisableVisuals()
    {
        characterVisuals.SetActive(false);
    }
}