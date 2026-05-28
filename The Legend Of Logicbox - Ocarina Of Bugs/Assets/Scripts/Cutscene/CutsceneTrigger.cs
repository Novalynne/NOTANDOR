using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

public class CutsceneTrigger : MonoBehaviour
{
    [Header("Cutscene")]
    public PlayableDirector cutsceneDirector;

    [Header("Camera")]
    public CinemachineFreeLook playerCamera;

    [Tooltip("Tutte le Virtual Camera usate nella cutscene (in ordine)")]
    public CinemachineVirtualCamera[] cutsceneCameras;

    [Header("Player")]
    public MovementInput playerMovement;

    [Header("Cleanup dopo cutscene")]
    public GameObject[] objectsToDeactivate;

    void Start()
    {
        if (cutsceneDirector != null)
            cutsceneDirector.stopped += OnCutsceneFinished;
    }

    public void PlayCutscene()
    {
        if (cutsceneDirector != null)
        {
            // Blocca il giocatore
            if (playerMovement != null)
                playerMovement.SetMovementEnabled(false);

            cutsceneDirector.gameObject.SetActive(true);
            cutsceneDirector.Play();
        }
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        // Abbassa la priorità di TUTTE le camere della cutscene
        // così Cinemachine parte dall'ultima attiva (Virtual Camera 2) per il blend
        foreach (var cam in cutsceneCameras)
        {
            if (cam != null)
                cam.Priority = 0;
        }

        // Ripristina la camera del giocatore con priorità alta
        if (playerCamera != null)
            playerCamera.Priority = 20;

        // Riabilita il movimento del giocatore
        if (playerMovement != null)
            playerMovement.SetMovementEnabled(true);

        // Disattiva oggetti residui
        foreach (var obj in objectsToDeactivate)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}