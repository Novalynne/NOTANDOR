using UnityEngine;
using UnityEngine.Localization;

public class NPCInteractable : Interactable
{
    [Header("Dialogo")]
    public DialogueData dialogue;

    [Header("Localizzazione NPC")]
    public LocalizedString promptAfterLocalized;

    [Header("Comportamento")]
    public Transform playerTransform;

    private PlayerInteract playerInteract;
    private MovementInput movementInput; // ← aggiunto
    private bool dialogueActive = false;
    private bool hasSpoken = false;

    void Awake()
    {
        isNPC = true;
        playerInteract = FindObjectOfType<PlayerInteract>();
        movementInput = FindObjectOfType<MovementInput>(); // ← aggiunto
    }

    public override void OnEnterRange()
    {
        if (playerTransform != null)
            transform.LookAt(new Vector3(
                playerTransform.position.x,
                transform.position.y,
                playerTransform.position.z));

        base.OnEnterRange();
    }

    protected override void Interact()
    {
        if (!dialogueActive)
        {
            dialogueActive = true;
            playerInteract?.SetInteracting(true);
            movementInput?.SetMovementEnabled(false); // ← blocca
            DialogueManager.Instance.StartDialogue(dialogue, OnDialogueEnd);
        }
        else
        {
            DialogueManager.Instance.NextLine();
        }
    }

    public void AdvanceDialogue()
    {
        if (dialogueActive)
            DialogueManager.Instance.NextLine();
    }

    void OnDialogueEnd()
    {
        dialogueActive = false;
        hasSpoken = true;
        playerInteract?.SetInteracting(false);
        movementInput?.SetMovementEnabled(true); // ← sblocca

        promptAfterLocalized.GetLocalizedStringAsync().Completed += handle =>
        {
            promptMessage = handle.Result;
        };
    }
}