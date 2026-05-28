using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Prompt (piccolo)")]
    public GameObject promptUI;
    public TMP_Text promptText;

    [Header("Dialogo NPC")]
    public GameObject dialogUI;
    public TMP_Text dialogText;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        promptUI.SetActive(false);
        dialogUI.SetActive(false);
    }

    //Prompt piccolo (E per interagire)
    public void ShowPrompt(string message, float duration = 2f)
    {
        promptUI.SetActive(true);
        promptText.text = message;

        StopAllCoroutines();
        StartCoroutine(HidePromptAfterTime(duration));
    }

    IEnumerator HidePromptAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        HidePrompt();
    }

    public void HidePrompt()
    {
        promptUI.SetActive(false);
    }

    // Dialogo NPC
    public void ShowDialog(string text)
    {
        dialogUI.SetActive(true);
        dialogText.text = text;
    }

    public void HideDialog()
    {
        dialogUI.SetActive(false);
    }
}