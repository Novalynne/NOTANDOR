using UnityEngine;
// Assicuriamoci che questo script sia sempre associato a un GameObject che ha un LaserEmitter
[RequireComponent(typeof(LaserEmitter))]
// Questo script è un semplice wrapper che permette di attivare l'emettitore di laser quando viene interagito.
public class LaserInteractable : Interactable
{
    // Riferimento all'emettitore di laser che vogliamo attivare
    private LaserEmitter emitter;

    void Awake()
    {
        // Otteniamo il riferimento all'emettitore di laser presente nello stesso GameObject
        emitter = GetComponent<LaserEmitter>();
    }

    protected override void Interact()
    {
        // Quando viene interagito, attiviamo l'emettitore di laser
        emitter.Activate();
    }
}
