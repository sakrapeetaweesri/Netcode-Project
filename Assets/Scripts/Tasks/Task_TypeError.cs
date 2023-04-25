using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class Task_TypeError : NetworkBehaviour
{
    [SerializeField] private GameObject errorScreen;
    [SerializeField] private Image restartButton;
    [SerializeField] private Image restartGauge;
    private bool isHolding;
    private Coroutine errorCoroutine;

    private readonly float holdSpeed = 0.25f;
    private float holdValue;

    private bool errorHit;
    private float timer;
    private readonly int initialErrorChance = 50;
    private int errorChance;

    public static Task_TypeError Instance { get; private set; }
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

        errorChance = initialErrorChance;
    }

    private void Update()
    {
        if (!Network_MainLobbyManager.Instance.gameStarted.Value) return;

        if (!errorHit)
        {
            ManageError();
            return;
        }

        if (isHolding)
        {
            holdValue += Time.deltaTime * holdSpeed * (Task_Type.Instance.playerInteracting.characterId.Value == 1 ? 4f : 1f);
            restartGauge.fillAmount = holdValue;
        }

        if (holdValue >= 1f)
        {
            FinishErrorTask();
        }
    }

    private void ManageError()
    {
        timer += Time.deltaTime;

        if (timer >= 1f)
        {
            timer = 0f;

            if (Random.Range(0, errorChance) != 0)
            {
                errorChance--;
                return;
            }

            errorHit = true;
            Task_Type.Instance.RequestSetErrorState(true);
            errorChance = initialErrorChance;
        }
    }

    public void ShowErrorTask()
    {
        holdValue = 0f;
        restartGauge.fillAmount = 0f;
        restartButton.raycastTarget = true;

        errorScreen.SetActive(true);
    }
    public void FinishErrorTask()
    {
        isHolding = false;
        holdValue = 0f;

        if (errorCoroutine != null) return;
        errorCoroutine = StartCoroutine(ErrorFinishDelay());
        Task_Type.Instance.RequestSetErrorState(false);
    }
    private IEnumerator ErrorFinishDelay()
    {
        restartButton.raycastTarget = false;

        restartGauge.fillAmount = 1f;
        restartGauge.enabled = false;
        yield return new WaitForSeconds(0.15f);
        restartGauge.enabled = true;
        yield return new WaitForSeconds(0.15f);
        restartGauge.enabled = false;
        yield return new WaitForSeconds(0.15f);
        restartGauge.enabled = true;

        yield return new WaitForSeconds(0.5f);
        Task_Type.Instance.FinishComputerTask(true);
        yield return new WaitForSeconds(0.8f);
        errorScreen.SetActive(false);

        errorHit = false;
        errorCoroutine = null;
    }

    public void RestartMouseOver()
    {
        if (holdValue >= 1f) return;
        restartButton.color = new Color(0.7830189f, 0.7830189f, 0.7830189f);
    }
    public void RestartMouseDown()
    {
        if (holdValue >= 1f) return;
        isHolding = true;
        restartButton.color = new Color(0.4622642f, 0.4622642f, 0.4622642f);
    }
    public void RestartMouseUp()
    {
        if (holdValue >= 1f) return;
        isHolding = false;
        restartButton.color = new Color(1f, 1f, 1f);

        holdValue = 0f;
        restartGauge.fillAmount = 0f;
    }
}