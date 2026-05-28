using UnityEngine;

// Questo script gestisce l'interazione del giocatore con oggetti interagibili nel mondo di gioco.
[RequireComponent(typeof(CharacterController))]
public class PlayerInteract : MonoBehaviour
{
    [Header("Detection Settings")]
    // Distanza massima alla quale il giocatore pu� interagire con gli oggetti
    public float interactDistance = 3f;
    // quanto deve essere davanti l'oggetto
    public float angleTolerance = 0.8f; 
    // LayerMask per filtrare solo gli oggetti interagibili
    public LayerMask interactMask;
    private Interactable currentInteractable;
    //character controller
    private CharacterController controller;
    // Flag per indicare se il giocatore � attualmente in un'interazione
    protected bool interacting = false;

    // Riferimenti UI
    [Header("UI References")]
    public GameObject pressEContinueText;

    // Nel metodo Start, nascondi il messaggio "Press E to continue" all'inizio del gioco
    private void Start()
    {
        if (pressEContinueText != null)
            pressEContinueText.SetActive(false);
    }
    
    private void Awake()
    {
        // Ottieni il riferimento al CharacterController
        controller = GetComponent<CharacterController>();
    }
    // Metodo pubblico per impostare lo stato di interazione
    public void SetInteracting(bool value)
    {
        interacting = value;
        if (pressEContinueText != null) // Mostra o nascondi il messaggio "Press E to continue" in base allo stato di interazione
            pressEContinueText.SetActive(value); // Se il giocatore non � pi� in interazione, chiama ClearCurrentState per nascondere eventuali messaggi residui
    }

    void Update()
    {
        // Se il giocatore non � attualmente in un'interazione, rileva gli oggetti interagibili e gestisci l'input per interagire
        if (!interacting)
        {
            DetectInteractable();
            if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
                // Se il tasto E viene premuto e c'� un oggetto interagibile, prova a interagire con esso
                currentInteractable?.TryInteract();
        }
        else
        {
            // Se il giocatore � in un'interazione, gestisci l'input per avanzare il dialogo (se l'interagibile � un NPC)
            if (Input.GetKeyDown(KeyCode.E))
                (currentInteractable as NPCInteractable)?.AdvanceDialogue();
        }
    }
    public void ClearCurrentState()
    {
        // Chiama OnExitRange per nascondere il messaggio tramite l'UIManager
        currentInteractable?.OnExitRange();
    }
    // Metodo per rilevare l'oggetto interagibile pi� vicino e davanti al giocatore
    void DetectInteractable()
    {
        // Calcola l'origine della sfera di rilevamento basata sulla posizione del CharacterController
        Vector3 origin = transform.TransformPoint(controller.center);
        // Usa Physics.OverlapSphere per trovare tutti i collider entro la distanza di interazione e filtrati dal LayerMask
        Collider[] hits = Physics.OverlapSphere(
            origin,
            interactDistance,
            interactMask
        );
        // Variabili per tenere traccia dell'oggetto interagibile migliore trovato
        Interactable bestInteractable = null;
        // Valore minimo di dot product per considerare un oggetto come "davanti" al giocatore
        float bestDot = angleTolerance;
        // Itera su tutti i collider rilevati
        foreach (Collider col in hits)
        {
            // Ottieni il componente Interactable dal collider
            Interactable interactable = col.GetComponent<Interactable>();
            // Se non c'� un componente Interactable o se l'oggetto non pu� essere interagito, salta al prossimo
            if (interactable == null || !interactable.canInteract)
                continue;
            // Calcola il punto pi� vicino del collider all'origine e la direzione verso quel punto
            Vector3 closestPoint = col.ClosestPoint(origin);
            // Normalizza la direzione per ottenere un vettore unitario
            Vector3 dirToTarget =
                (closestPoint - origin).normalized;
            // Calcola il dot product tra la direzione del giocatore e la direzione verso l'oggetto interagibile
            float dot = Vector3.Dot(transform.forward, dirToTarget);
            // Se il dot product � maggiore del miglior valore trovato finora, aggiorna l'oggetto interagibile migliore
            if (dot > bestDot)
            {
                bestDot = dot;
                bestInteractable = interactable;
            }
        }
        // Se l'oggetto interagibile migliore trovato � diverso da quello attualmente selezionato, aggiorna lo stato di interazione
        if (bestInteractable != currentInteractable)
        {
            // Se c'� un oggetto interagibile attualmente selezionato, chiama OnExitRange per nascondere il messaggio tramite l'UIManager
            currentInteractable?.OnExitRange();

            currentInteractable = bestInteractable;
            // Se c'� un nuovo oggetto interagibile selezionato, chiama OnEnterRange per mostrare il messaggio tramite l'UIManager
            currentInteractable?.OnEnterRange();
        }
        // Se l'oggetto interagibile attualmente selezionato non � pi� interagibile, chiama OnExitRange e resetta la variabile
        if (currentInteractable != null &&
            !currentInteractable.canInteract)
        {
            // Se l'oggetto interagibile attualmente selezionato non � pi� interagibile, chiama OnExitRange per nascondere il messaggio tramite l'UIManager
            currentInteractable.OnExitRange();
            currentInteractable = null;
        }
    }

    // Metodo pubblico per ottenere l'oggetto interagibile attualmente selezionato
    public Interactable GetCurrentInteractable() => currentInteractable;
}