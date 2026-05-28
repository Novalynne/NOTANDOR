using UnityEngine;
using UnityEngine.Events;

// Il LaserSensor è un tipo speciale di Reflector che non riflette il laser, ma si attiva quando colpito da un certo colore
public class LaserSensor : Reflector
{
    [Header("Sensor Settings")]
    // Il colore del laser che attiva il sensore
    public LaserColor activationColor = LaserColor.Green;
    // Il colore dell'emissione quando il sensore è attivo
    public Color activeEmissionColor = Color.yellow;

    [Header("Events")]
    // Eventi per quando il sensore si attiva o si disattiva
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    private bool isActive = false;

    public override void OnLaserHit(Vector3 hitPoint, LaserColor incomingColor)
    {
        // Segniamo che siamo stati colpiti in questo frame
        isHitThisFrame = true;

        if (incomingColor == activationColor)
        {
            // Se il colore è quello giusto e non siamo già attivi, attiviamo il sensore
            if (!isActive)
            {
                ActivateSensor();
            }
        }
        else
        {
            // Se il colore è sbagliato e siamo attivi, disattiviamo il sensore
            if (isActive)
            {
                DeactivateSensor();
            }
        }
    }

    private void ActivateSensor()
    {
        isActive = true;
        // Cambiamo il colore dell'emissione in Giallo
        if (rend != null)
        {
            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", activeEmissionColor * 3f);
        }
        // Qui potremmo anche aggiungere effetti sonori o particelle per evidenziare l'attivazione
        OnActivated?.Invoke();
    }

    private void DeactivateSensor()
    {
        // Resettiamo il colore dell'emissione al colore originale (o lo disabilitiamo)
        isActive = false;
        Illuminate(false);
        // Qui potremmo anche aggiungere effetti sonori o particelle per evidenziare la disattivazione
        OnDeactivated?.Invoke();
    }
    // LateUpdate viene chiamato dopo tutti gli Update, utile per gestire lo stato del sensore in base ai laser
    void LateUpdate()
    {
        // Se il laser smette di colpirci, resettiamo tutto
        if (!isHitThisFrame && isActive)
        {
            // Se non siamo stati colpiti in questo frame ma eravamo attivi, significa che il laser è stato interrotto
            DeactivateSensor();
        }
        // Resettiamo il flag per il prossimo frame
        isHitThisFrame = false;
    }
}