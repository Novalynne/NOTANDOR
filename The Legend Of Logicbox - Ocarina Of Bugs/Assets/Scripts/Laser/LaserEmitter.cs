using System.Collections;
using UnityEngine;
// Questo script è responsabile di emettere un raggio laser in una direzione specifica quando attivato.
public class LaserEmitter : LaserNode
{
    // Impostazioni configurabili per il laser emesso
    [Header("Laser Emitter Settings")]
    // La direzione in cui il laser viene emesso
    public Vector3 shootDirection = Vector3.forward;
    // La durata per cui il laser rimane attivo quando viene attivato
    public float duration = 5f;

    // Gestiamo il colore corrente separatamente dal laserColor ufficiale
    private LaserColor currentColorToShoot;

    void Start()
    {
        // Inizialmente, il laser è in modalità "preview" (idle)
        currentColorToShoot = LaserColor.Preview;
    }

    void Update()
    {
        // Delega il raggio alla funzione centralizzata in LaserNode
        Vector3 worldDirection = transform.TransformDirection(shootDirection);
        FireRay(LaserOrigin, worldDirection, currentColorToShoot);
    }

    public void Activate()
    {
        // Quando attivato, emetti il laser del colore specificato per la durata impostata
        StopAllCoroutines();
        StartCoroutine(LaserRoutine());
    }

    // courutine che gestisce la durata del laser emesso
    IEnumerator LaserRoutine()
    {
        // Cambia il colore del laser al colore effettivo (Rosso o Verde) per la durata specificata
        currentColorToShoot = laserColor;
        yield return new WaitForSeconds(duration);
        // Dopo la durata, torna in modalità "preview" (idle)
        currentColorToShoot = LaserColor.Preview;
    }
}