using UnityEngine;
using UnityEngine.Localization;

public abstract class Interactable : MonoBehaviour
{
    // Impostazioni di interazione
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public string promptMessage = "";

    [Header("Localizzazione (opzionale)")]
    public LocalizedString localizedPrompt;

    [Header("Interaction State")]
    public bool canInteract = true;
    protected bool playerInRange = false;
    public bool isNPC = false;

    private LocalizedString.ChangeHandler onPromptChanged;

    protected virtual void Start()
    {
        if (!localizedPrompt.IsEmpty)
        {
            onPromptChanged = updatedText =>
            {
                Debug.Log($"[Interactable] Prompt localizzato caricato: {updatedText}");
                promptMessage = updatedText;
            };
            localizedPrompt.StringChanged += onPromptChanged;
        }
        else
        {
            Debug.Log("[Interactable] localizedPrompt è vuoto!");
        }
    }

    protected virtual void OnDestroy()
    {
        if (onPromptChanged != null)
            localizedPrompt.StringChanged -= onPromptChanged;
    }
    // Metodo chiamato quando il giocatore entra nel raggio di interazione
    public virtual void OnEnterRange()
    {
        if (!canInteract) return;
        playerInRange = true;

        // Se la localizzazione non ha ancora caricato, forza il caricamento sincrono
        if (string.IsNullOrEmpty(promptMessage) && !localizedPrompt.IsEmpty)
        {
            Debug.Log("[Interactable] StringChanged non ancora attivato, caricamento forzato...");
            promptMessage = localizedPrompt.GetLocalizedString();
        }

        if (string.IsNullOrEmpty(promptMessage)) return;

        if (isNPC)
            UIManager.Instance.ShowDialog(promptMessage);
        else
            UIManager.Instance.ShowPrompt(promptMessage);
    }
    // Metodo chiamato quando il giocatore esce dal raggio di interazione
    public virtual void OnExitRange()
    {
        playerInRange = false;
        UIManager.Instance.HidePrompt();
        UIManager.Instance.HideDialog();
    }
    // Metodo pubblico per tentare di interagire con l'oggetto
    public void TryInteract()
    {
        if (playerInRange && canInteract)
            Interact();
    }
    // Metodo astratto che deve essere implementato dalle classi derivate per definire il comportamento specifico dell'interazione
    protected abstract void Interact();
}