using UnityEngine;

public class TorchObserver : MonoBehaviour
{
    [Header("Settings")]
    public bool turnOffAtStart = true; // Se spuntata, la torcia si spegne al Play

    [Header("References")]
    public Light torchLight;
    public GameObject flameVisual;

    private void Start()
    {
        // Se la spunta è attiva, spegniamo tutto all'avvio
        if (turnOffAtStart)
        {
            SetTorchState(false);
        }
    }

    public void Activate() => SetTorchState(true);
    public void Deactivate() => SetTorchState(false);

    private void SetTorchState(bool state)
    {
        // Gestione della luce
        if (torchLight != null)
            torchLight.enabled = state;

        // Gestione degli effetti visivi/fiamme
        if (flameVisual != null && flameVisual != this.gameObject)
        {
            flameVisual.SetActive(state);
        }
        else if (flameVisual == this.gameObject)
        {
            Debug.LogWarning("Attenzione: flameVisual non può essere l'oggetto Torch stesso, altrimenti lo script si disattiva!");
        }
    }
}