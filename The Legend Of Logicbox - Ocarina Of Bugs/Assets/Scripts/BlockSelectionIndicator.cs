using UnityEngine;

/// <summary>
/// Muove una sfera rossa sopra il blocco attualmente selezionato dal player (quello che
/// PlayerInteract sta puntando, prima ancora di premere E).
/// 
/// SETUP:
///   1. Crea una sfera in scena (GameObject > 3D Object > Sphere), colorala di rosso.
///   2. Aggiungi questo script al player (stesso GameObject di PlayerInteract).
///   3. Trascina la sfera nel campo "Indicator" dell'Inspector.
///
/// REQUISITO: aggiungi questo getter pubblico a PlayerInteract.cs
///   public Interactable GetCurrentInteractable() => currentInteractable;
/// </summary>
public class BlockSelectionIndicator : MonoBehaviour
{
    [Header("References")]
    [Tooltip("La sfera rossa da muovere sopra il blocco selezionato")]
    public Transform indicator;

    [Header("Position Settings")]
    [Tooltip("Altezza della sfera sopra il blocco")]
    public float heightOffset = 1.5f;

    [Tooltip("Posizione di 'nascondimento' quando nessun blocco è selezionato")]
    public Vector3 hiddenPosition = new Vector3(100f, 100f, 100f);

    [Tooltip("Velocità con cui la sfera si sposta (lerp). 0 = istantaneo")]
    public float followSpeed = 10f;

    // ----------------------------------------------------------------
    private PlayerInteract playerInteract;

    void Start()
    {
        playerInteract = GetComponent<PlayerInteract>();

        if (playerInteract == null)
            Debug.LogError("BlockSelectionIndicator: PlayerInteract non trovato sullo stesso GameObject!");

        if (indicator == null)
            Debug.LogError("BlockSelectionIndicator: nessuna sfera assegnata al campo 'Indicator'!");
        else
            indicator.position = hiddenPosition;
    }

    void Update()
    {
        if (indicator == null || playerInteract == null) return;

        // Leggiamo il blocco correntemente selezionato tramite il getter pubblico
        Interactable current = playerInteract.GetCurrentInteractable();

        Vector3 targetPos;

        if (current != null)
        {
            // Spostiamo la sfera sopra il blocco selezionato
            targetPos = current.transform.position + Vector3.up * heightOffset;
        }
        else
        {
            // Nessun blocco selezionato: mandiamo la sfera lontano
            targetPos = hiddenPosition;
        }

        // Movimento fluido (lerp), oppure istantaneo se followSpeed <= 0
        if (followSpeed > 0f)
            indicator.position = Vector3.Lerp(indicator.position, targetPos, Time.deltaTime * followSpeed);
        else
            indicator.position = targetPos;
    }
}