using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
//collider per collisioni
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Renderer))]

//oggetto che può essere spostato o ruotato dal giocatore, interagendo con esso
public class MovableObject : Interactable
{
    // Se l'oggetto può essere spostato o ruotato
    [Header("Movement Settings")]
    public bool isMovable = true;
    public bool isRotatable = true;
    //riferimento al Rigidbody per gestire il movimento e la rotazione durante l'interazione
    protected Rigidbody rb;
    //riferimento al playerGrab
    private PlayerGrab playerGrab;
    //riferimento allo script della griglia
    private GridRuntime gridRuntime;
    // Dimensione dell'oggetto in celle della griglia, default 1x1, calcolata in base alla dimensione del Renderer
    private Vector2Int size = new Vector2Int(1, 1);

    protected override void Interact()
    {
        if(!isMovable && !isRotatable)
            return;
        // chiama il metodo StartGrab del PlayerGrab per iniziare a spostare o ruotare l'oggetto
        if(playerGrab != null)
            playerGrab.StartGrab(this);
    }
    // Nel metodo Awake, otteniamo il riferimento al Rigidbody
    // e lo impostiamo come kinematic per evitare che la fisica interferisca con il movimento manuale durante l'interazione
    void Awake()
    {
        // Ottiene il Rigidbody
        rb = GetComponent<Rigidbody>();
        // Imposta il Rigidbody come kinematic per evitare interferenze con la fisica durante l'interazione
        rb.isKinematic = true;
    }
    // Nel metodo Start, controlliamo se il PlayerGrab è presente nella scena e logghiamo un errore se non lo è
    private void Start()
    {
        // controllo scena per player grab
        playerGrab = GameObject.FindObjectOfType<PlayerGrab>();
        if (playerGrab == null)
        {
            Debug.LogError(
                $"PlayerGrab non trovato nella scena, disabilito MovableObject"
            );
            enabled = false;
            canInteract = false;
            return;
        }
        // controllo scena per grid runtime
        gridRuntime = GameObject.FindObjectOfType<GridRuntime>();
        if (gridRuntime == null)
        {
            Debug.LogError(
                $"GridRuntime non trovato nella scena, disabilito MovableObject"
            );
            enabled = false;
            canInteract = false;
            return;
        }
        //calcola la dimensione in celle della griglia in base alla dimensione dell'oggetto
        CalculateGridSize();

    }

    // Metodo per calcolare la dimensione dell'oggetto in celle della griglia
    void CalculateGridSize()
    {
        // Ottiene la dimensione di una cella della griglia dal GridRuntime
        float cellSize = gridRuntime.tileDistance;
        
        // Ottiene il Renderer dell'oggetto per calcolare la dimensione del mondo
        Renderer rend = GetComponent<Renderer>();

        if (rend == null)
        {
            Debug.LogError($"{name} has no Renderer!");
            return;
        }
        // Ottiene la dimensione del mondo dell'oggetto dal suo Renderer
        Vector3 worldSize = rend.bounds.size;
        // Calcola la dimensione in celle della griglia arrotondando per eccesso
        size = new Vector2Int(
            Mathf.CeilToInt(worldSize.x / cellSize),
            Mathf.CeilToInt(worldSize.z / cellSize)
        );
    }
}