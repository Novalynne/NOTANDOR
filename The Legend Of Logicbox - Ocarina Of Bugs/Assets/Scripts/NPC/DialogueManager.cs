using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Localization;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Typing Effect")]
    public float typingSpeed = 0.04f;

    private DialogueData currentDialogue;
    private int currentLine;
    private Action onDialogueEnd;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartDialogue(DialogueData dialogue, Action onEnd = null)
    {
        currentDialogue = dialogue;
        currentLine = 0;
        onDialogueEnd = onEnd;
        ShowCurrentLine();
    }

    public void NextLine()
    {
        if (isTyping) { SkipTyping(); return; }

        currentLine++;
        if (currentLine >= currentDialogue.lines.Length)
            EndDialogue();
        else
            ShowCurrentLine();
    }

    void ShowCurrentLine()
    {
        var line = currentDialogue.lines[currentLine];

        // Recupera il testo localizzato, poi avvia il typing
        line.localizedText.GetLocalizedStringAsync().Completed += handle =>
        {
            string localizedBody = handle.Result;
            string display = string.IsNullOrEmpty(line.speakerName)
                ? localizedBody
                : $"<b>{line.speakerName}</b>\n{localizedBody}";

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(display));
        };
    }

    IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        UIManager.Instance.ShowDialog("");

        int startIndex = fullText.IndexOf('\n') + 1;
        string header = startIndex > 0 ? fullText.Substring(0, startIndex) : "";
        string body = startIndex > 0 ? fullText.Substring(startIndex) : fullText;

        for (int i = 0; i <= body.Length; i++)
        {
            UIManager.Instance.ShowDialog(header + body.Substring(0, i));
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void SkipTyping()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);

        var line = currentDialogue.lines[currentLine];
        line.localizedText.GetLocalizedStringAsync().Completed += handle =>
        {
            string localizedBody = handle.Result;
            string display = string.IsNullOrEmpty(line.speakerName)
                ? localizedBody
                : $"<b>{line.speakerName}</b>\n{localizedBody}";

            UIManager.Instance.ShowDialog(display);
            isTyping = false;
        };
    }

    void EndDialogue()
    {
        UIManager.Instance.HideDialog();
        onDialogueEnd?.Invoke();
        currentDialogue = null;
    }
}