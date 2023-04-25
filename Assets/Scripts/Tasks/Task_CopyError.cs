using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class Task_CopyError : NetworkBehaviour
{
    [SerializeField] private RectTransform errorScreen;
    [SerializeField] private RectTransform paperPack;
    [SerializeField] private RectTransform doubleFPack;
    [SerializeField] private RectTransform printerPaperPack;
    [SerializeField] private Image doubleFTop;
    [SerializeField] private Sprite[] tops;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private string[] instructions;
    [SerializeField] private Task_CopyError_TopPack drag;
    [SerializeField] private Task_CopyError_Paper paper;
    private Coroutine doubleFPackCoroutine;
    private Coroutine paperPackCoroutine;
    private Coroutine errorScreenCoroutine;
    private Coroutine printerPaperCoroutine;
    private Coroutine errorCoroutine;

    private bool errorHit;
    private readonly int initialErrorChance = 4;
    private int errorChance;

    public static Task_CopyError Instance { get; private set; }
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

        if (!errorHit && !Task_Copy.Instance.isError.Value)
            return;

        HandleDragging();
    }

    public void ManageError()
    {
        if (Random.Range(0, errorChance) != 0)
        {
            errorChance--;
            return;
        }

        Task_Copy.Instance.RequestSetErrorState(true);
        errorHit = true;
        errorChance = initialErrorChance;
    }

    public void ShowErrorTask()
    {
        Task_Copy.Instance.playerInteracting.SetBlockMovement(true);

        paperPack.anchoredPosition = new Vector2(-495f, -825.6801f);
        paperPack.gameObject.SetActive(true);
        doubleFPack.anchoredPosition = new Vector2(-495f, -806f);

        instructionText.SetText(instructions[0]);

        drag.enabled = true;
        drag.ClearSectionCount();
        doubleFTop.sprite = tops[0];
        doubleFTop.enabled = true;

        printerPaperPack.anchoredPosition = new Vector2(273f, -154.68f);
        printerPaperPack.gameObject.SetActive(false);

        if (errorCoroutine != null) StopCoroutine(errorCoroutine);
        errorCoroutine = StartCoroutine(ShowDoubleFPack());
    }
    private IEnumerator ShowDoubleFPack()
    {
        if (errorScreenCoroutine != null) StopCoroutine(errorScreenCoroutine);

        errorScreenCoroutine = StartCoroutine(Utils.SlideCoroutine
            (errorScreen, errorScreen.anchoredPosition, Vector2.zero, 20f));

        yield return new WaitForSeconds(0.2f);

        if (doubleFPackCoroutine != null) StopCoroutine(doubleFPackCoroutine);
        if (paperPackCoroutine != null) StopCoroutine(paperPackCoroutine);

        doubleFPackCoroutine = StartCoroutine(Utils.SlideCoroutine
            (doubleFPack, doubleFPack.anchoredPosition, new Vector2(-495f, -118f), 20f));
        paperPackCoroutine = StartCoroutine(Utils.SlideCoroutine
            (paperPack, paperPack.anchoredPosition, new Vector2(-495f, -137.6801f), 20f));

        errorCoroutine = null;
    }

    private void HandleDragging()
    {
        if (!drag.enabled) return;

        if (drag.sectionPassed < tops.Length)
        {
            doubleFTop.enabled = true;
            doubleFTop.sprite = tops[drag.sectionPassed];
        }
        else
        {
            doubleFTop.enabled = false;
            drag.enabled = false;
            StartCoroutine(PackOpened());
        }
    }

    private IEnumerator PackOpened()
    {
        yield return new WaitForSeconds(0.5f);

        if (doubleFPackCoroutine != null) StopCoroutine(doubleFPackCoroutine);

        doubleFPackCoroutine = StartCoroutine(Utils.SlideCoroutine
            (doubleFPack, doubleFPack.anchoredPosition, new Vector2(-495f, -806f), 10f));

        instructionText.SetText(instructions[1]);
        paper.ResetDraggable();
    }

    public void FinishErrorTask()
    {
        if (errorCoroutine != null) return;
        errorCoroutine = StartCoroutine(ErrorFinishDelay());
        Task_Copy.Instance.RequestSetErrorState(false);
    }
    private IEnumerator ErrorFinishDelay()
    {
        printerPaperPack.gameObject.SetActive(true);
        printerPaperPack.anchoredPosition = new Vector2(273f, -154.68f);
        if (printerPaperCoroutine != null) StopCoroutine(printerPaperCoroutine);
        printerPaperCoroutine = StartCoroutine(Utils.SlideCoroutine
            (printerPaperPack, printerPaperPack.anchoredPosition, new Vector2(273f, -193.17f), 10f));

        yield return new WaitForSeconds(1.2f);

        if (errorScreenCoroutine != null) StopCoroutine(errorScreenCoroutine);

        errorScreenCoroutine = StartCoroutine(Utils.SlideCoroutine
            (errorScreen, errorScreen.anchoredPosition, new Vector2(0f, -1100f), 30f));

        Task_Copy.Instance.playerInteracting.SetBlockMovement(false);

        errorHit = false;
        errorCoroutine = null;
    }
}