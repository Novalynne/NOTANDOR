using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

public class ChestCutsceneTrigger : Interactable
{
    [Header("Cutscene")]
    public PlayableDirector cutsceneDirector;

    [Header("Camera")]
    public CinemachineFreeLook playerCamera;

    [Header("Blocchi")]
    public LogicNot bloccoNot;

    [Header("Player")]
    public MovementInput playerMovement;

    void Start()
    {
        if (cutsceneDirector != null)
        {
            cutsceneDirector.gameObject.SetActive(false);
            cutsceneDirector.stopped += OnCutsceneFinished;
        }
    }

    protected override void Interact()
    {
        canInteract = false;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (cutsceneDirector != null)
        {
            cutsceneDirector.gameObject.SetActive(true);
            cutsceneDirector.Play();
        }
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        if (playerCamera != null)
            playerCamera.Priority = 20;

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (bloccoNot != null)
        {
            MovableObject movable = bloccoNot.GetComponent<MovableObject>();
            if (movable != null)
                movable.isRotatable = true;
        }
    }
}