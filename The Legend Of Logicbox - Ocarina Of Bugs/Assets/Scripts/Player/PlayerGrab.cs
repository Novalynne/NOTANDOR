using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Burst.Intrinsics.Arm;

[RequireComponent(typeof(PlayerInteract))]
[RequireComponent(typeof(CharacterController))]
// Questo script gestisce il sistema di grab del player, permettendo di afferrare oggetti, muoverli a scatti in base alle tile incontrate lungo il percorso e ruotarli di 90 gradi.
public class PlayerGrab : MonoBehaviour { 
    [Header("Grab Settings")]
    // Layer da assegnare agli oggetti afferrati che possono interagire con i nodi laser (es. blocchi di spostamento)
    public string movableLaserLayerName = "Grabbed";
    public string movableInertLayerName = "GrabbedInert";
    [Header("Movement Settings")]
    public LayerMask obstacleMask;
    public float normalSpeed = 4f;
    // Stato del grab e del movimento
    private bool isGrabbing = false;
    private bool isMoving = false;
    private bool isRotating = false;
    // Riferimento all'oggetto attualmente afferrato
    private MovableObject grabbedObject;
    // Riferimento allo script di interazione del player per aggiornare lo stato di interazione durante il grab
    private PlayerInteract playerInteract;
    private int originalLayer;
    //controller di sicurezza
    private CharacterController controller;
    //Gestione script di movimento del player
    private MovementInput moveScript;
    private float originalVelocity;
    private float originalAllowRotation;
    //mantiene la direzione del grab
    private Vector3 grabDirection;
    //riferimento script griglia
    private GridRuntime grid;
    // Inizializzazione
    void Start()
    {
        // Otteniamo i riferimenti necessari
        playerInteract = GetComponent<PlayerInteract>();
        controller = GetComponent<CharacterController>();
        moveScript = GetComponent<MovementInput>();
        grid = GameObject.FindObjectOfType<GridRuntime>();
        if (grid == null){
            Debug.LogError(
             "PlayerGrab: GridRuntime non trovato! " +
             "Disattivo PlayerGrab."
            );
            enabled = false;
            return;
        }
        // Salviamo i valori originali per ripristinarli dopo
        if (moveScript != null)
        {
            originalVelocity = moveScript.Velocity;
            originalAllowRotation = moveScript.allowPlayerRotation;
        }
    }

    void Update()
    {
        // Se non stiamo afferrando nulla, non gestiamo input di movimento
        if (grabbedObject == null)
        {
            isGrabbing = false;
            return;
        }

        // Tasto per staccarsi
        if (Input.GetKeyDown(KeyCode.Q) && !isMoving && !isRotating) StopGrab();
        // Se stiamo ruotando o muovendo, ignoriamo altri input finché non finisce l'azione
        if (!isMoving && !isRotating)
        {
            if (Input.GetKeyDown(KeyCode.R) && grabbedObject != null && grabbedObject.isRotatable) StartCoroutine(RotateObject());
            if (grabbedObject == null || !grabbedObject.isMovable)
            {
                return; // Se l'oggetto non può essere mosso, non gestiamo input di movimento del player
            }
            // Gestione Input
            Vector3 dir = GetInputDirection();
            if (moveScript != null && moveScript.anim != null)
            {
                // Se stiamo premendo tasti di movimento, forziamo il Blend dell'animazione
                float targetSpeed = (dir != Vector3.zero) ? 0.5f : 0f; // 0.5 è un valore tipico per camminata
                moveScript.anim.SetFloat("Blend", targetSpeed, 0.1f, Time.deltaTime);
            }
            if (dir != Vector3.zero)
            {
                // Se c'è input, gestiamo il movimento
                Debug.Log($"<color=cyan>--- Tasto premuto! Direzione: {dir} ---</color>");
                HandleMovement(dir);
            }
        }
    }
    public bool IsGrabbing()
    {
        // Metodo pubblico per permettere ad altri script di sapere se il player sta afferrando un oggetto
        return isGrabbing;
    }
    // Questo metodo gestisce la logica di movimento a scatti del blocco e del player in base alla direzione di input e alle tile incontrate lungo il percorso
    void HandleMovement(Vector3 dir)
    {
        // Se stiamo già muovendo, non accettiamo nuovi input finché non finisce il movimento attuale
        if (isMoving)
        {
            Debug.Log("<color=grey>Input ignorato: movimento in corso</color>");
            return;
        }
        // Se stiamo ruotando, non accettiamo nuovi input di movimento finché non finisce la rotazione attuale
        if (isRotating)
        {
            Debug.Log("<color=grey>Input ignorato: rotazione in corso</color>");
            return;
        }
        // Se non c'è un oggetto afferrato, non possiamo muovere nulla, quindi logghiamo un avviso e usciamo dal metodo
        if (grabbedObject == null)
        {
            Debug.LogWarning("Tentativo di muovere senza oggetto grab.");
            return;
        }
        //posizione iniziale del blocco
        Vector3 startBlock =
            Snap(grabbedObject.transform.position);

        Vector3 currentBlock = startBlock;
        //cra una lista per tenere traccia del percorso del blocco
        List<Vector3> blockPath = new List<Vector3>();
        // Aggiungiamo la posizione iniziale al percorso
        blockPath.Add(currentBlock);
        // Limitiamo il numero massimo di passi per evitare loop infiniti in caso di errori
        int maxSteps = 50;

        Debug.Log($"<color=white>START MOVE dir: {dir}</color>");
        // Iteriamo fino a maxSteps per costruire il percorso
        for (int i = 0; i < maxSteps; i++)
        {
            // Otteniamo la tile corrente sotto il blocco
            GridTileBase currentTile = GetTileAt(currentBlock);
            
            // Se la tile è null, fermiamo il movimento (es. blocco fuori griglia)
            if (currentTile == null)
            {
                //mostra posizione cercata
                Debug.Log("<color=red>Tile corrente NULL</color>");
                break;
            }
            Debug.Log($"<color=yellow>STEP {i} | Tile: {currentTile.name}</color>");
            //applichiamo la modifica della tile alla posizione del blocco, ottenendo la posizione cercata
            Vector3 nextBlock = currentTile.ModifyMovement(
                currentBlock,
                ref dir,
                grid.tileDistance,
                obstacleMask
            );
            // Se la tile non modifica la posizione, fermiamo il movimento
            if (nextBlock == currentBlock)
            {
                Debug.Log("<color=orange>Blocco fermato</color>");
                break;
            }
            // La posizione cercata per il blocco determina anche la posizione cercata per il player, che deve rimanere nella stessa posizione relativa al blocco
            Vector3 nextPlayer = nextBlock - grabDirection * grid.tileDistance;
            // Verifichiamo se la posizione cercata è valida (es. non fuori griglia, non bloccata da ostacoli)
            bool blockInside = GetTileAt(nextBlock) != null;
            
            Debug.Log($"BlockInside: {blockInside}");
            //  blocco fuori griglia
            if (!blockInside)
            {
                Debug.Log("<color=red>Blocco fuori griglia! Movimento interrotto.</color>");
                break;
            }

            // collisioni blocco
            Collider blockCol = grabbedObject.GetComponent<Collider>();
            // Calcoliamo il centro e le estensioni della scatola di collisione del blocco nella posizione cercata
            Vector3 blockCenter = nextBlock + blockCol.bounds.center - grabbedObject.transform.position;

            Vector3 blockHalfExtents = blockCol.bounds.extents * 0.9f;
            // Usiamo OverlapBox per verificare se il blocco incontrerebbe ostacoli nella posizione cercata
            Collider[] blockHits = Physics.OverlapBox(
                blockCenter,
                blockHalfExtents,
                Quaternion.identity,
                obstacleMask
            );

            if (blockHits.Length > 0)
            {
                // Se ci sono collisioni, fermiamo il movimento e logghiamo gli ostacoli che bloccano il blocco
                Debug.Log("<color=red>Blocco bloccato da:</color>");
                foreach (var h in blockHits)
                    Debug.Log($" - {h.name}");
                break;
            }

            // collisioni player
            float halfHeight = controller.height / 2f;
            // Calcoliamo il centro e le estensioni della scatola di collisione del player nella posizione cercata
            Vector3 checkCenter = nextPlayer + Vector3.up * halfHeight;
            // Usiamo una scatola leggermente più piccola del CharacterController per evitare falsi positivi dovuti a piccole irregolarità del terreno
            Vector3 checkHalfExtents =
                new Vector3(
                    controller.radius,
                    halfHeight,
                    controller.radius
                ) * 0.8f;
            // Usiamo OverlapBox per verificare se il player incontrerebbe ostacoli nella posizione cercata
            Collider[] hits = Physics.OverlapBox(
                checkCenter,
                checkHalfExtents,
                Quaternion.identity,
                obstacleMask
            );

            if (hits.Length > 0)
            {
                // Se ci sono collisioni, fermiamo il movimento e logghiamo gli ostacoli che bloccano il player 
                // Per ogni ostacolo, mostriamo il nome e il layer per aiutare a identificare cosa sta bloccando il player
                foreach (var h in hits)
                    Debug.Log($" - {h.name} | Layer: {LayerMask.LayerToName(h.gameObject.layer)}");
                break;
            }
            // aggiorna
            currentBlock = nextBlock;
            // Aggiungiamo la nuova posizione al percorso
            blockPath.Add(currentBlock);

            GridTileBase destTile = GetTileAt(currentBlock);
            // Se la tile di destinazione è null, fermiamo il movimento (es. blocco fuori griglia)
            if (destTile == null)
            {
                Debug.Log("<color=red>Tile destinazione NULL</color>");
                break;
            }
            // Logghiamo il nome della tile di destinazione e se permette di continuare il movimento
            Debug.Log($"<color=cyan>Arrivato su: {destTile.name}</color>");
            // Se la tile di destinazione non permette di continuare il movimento, fermiamo il percorso qui
            if (!destTile.ShouldContinue())
            {
                Debug.Log("<color=grey>Stop: tile normale</color>");
                break;
            }
        }
        // Se il percorso contiene più di una posizione (cioè c'è un movimento da fare), avviamo la coroutine di movimento
        if (blockPath.Count > 1)
        {
            isMoving = true;
            StartCoroutine(MoveStepByStep(blockPath));
        }
        else
        {
            // Altrimenti, se non c'è movimento da fare, logghiamo che non c'è movimento
            Debug.Log("<color=grey>Nessun movimento</color>");
        }
    }

    // Questo metodo gestisce la logica di input per determinare la direzione di movimento desiderata dal player.
    Vector3 GetInputDirection()
    {
        // Usiamo GetAxisRaw per precisione immediata
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        // Se non c'è input, ritorniamo zero
        if (h == 0 && v == 0) return Vector3.zero;

        // Priorità assoluta: se muovi avanti/dietro, ignora destra/sinistra
        if (Mathf.Abs(v) > 0.1f)
        {
            return transform.forward * Mathf.Sign(v);
        }
        else
        {
            return transform.right * Mathf.Sign(h);
        }
    }

    // Questa coroutine gestisce l'animazione del movimento del blocco e del player lungo il percorso calcolato, sincronizzando la posizione del blocco e del player
    IEnumerator MoveStepByStep(List<Vector3> blockPath)
    {
        Debug.Log("<color=white>START ANIMATION</color>");
        // Iteriamo su ogni passo del percorso (partendo dal secondo, perché il primo è la posizione iniziale)
        for (int i = 1; i < blockPath.Count; i++)
        {
            float baseSpeed = normalSpeed;
            // Per ogni passo, determiniamo la velocità di movimento in base alla tile su cui stiamo arrivando, in modo da far muovere il blocco più lentamente su tile che rallentano il movimento
            Vector3 startBlock = blockPath[i - 1];
            Vector3 endBlock = blockPath[i];
            // Otteniamo la tile su cui stiamo arrivando per determinare la velocità di movimento
            GridTileBase tile = GetTileAt(endBlock);

            float speed = baseSpeed;
            // Se la tile non è null, usiamo il suo metodo GetMoveSpeed per ottenere la velocità modificata in base alla tile (es. tile di fango che rallenta il movimento)
            if (tile != null)
                speed = tile.GetMoveSpeed(baseSpeed);
            // Calcoliamo la durata del movimento in base alla distanza da percorrere e alla velocità
            float distance = Vector3.Distance(startBlock, endBlock);

            float duration = distance / speed;

            float completamentoMovimento = 0f;
            // Logghiamo il passo corrente, la velocità e la durata prevista del movimento per aiutare a debug e bilanciare le tile
            Debug.Log($"<color=green>Step {i} | Speed: {speed} | Duration: {duration}</color>");
            // Animiamo il movimento interpolando la posizione del blocco e del player lungo il percorso tra start e end
            //0f= movimento all'inizio, 1f= movimento alla fine
            while (completamentoMovimento < 1f)
            {
                // Incrementiamo t in base al tempo trascorso e alla durata prevista del movimento
                completamentoMovimento += Time.deltaTime / duration;
                // Interpoliamo la posizione del blocco e del player usando Lerp, in modo che si muovano in sincronia lungo il percorso
                grabbedObject.transform.position =
                    Vector3.Lerp(
                        startBlock,
                        endBlock,
                        completamentoMovimento
                    );
                //il player segue il blocco nel movimento
                transform.position = grabbedObject.transform.position - grabDirection * grid.tileDistance;

                yield return null;
            }
            //assegna le posizioni finali alla fine di ogni step per evitare problemi di precisione dovuti all'interpolazione
            grabbedObject.transform.position = endBlock;
            transform.position = endBlock - grabDirection * grid.tileDistance;
        }
        //aggiorna lo stato
        isMoving = false;

        Debug.Log("<color=white>END ANIMATION</color>");
    }

    // Questo metodo usa Physics.OverlapBox per cercare una tile di tipo GridTileBase nella posizione specificata,
    // in modo da poter interagire con le proprietà e i metodi della tile
    GridTileBase GetTileAt(Vector3 pos)
    {
        // Spostiamo il centro della ricerca leggermente sotto lo 0 (es. -0.2f) 
        // e usiamo una scatola che copre sia sopra che sotto lo zero.
        Collider[] hits = Physics.OverlapBox(pos, new Vector3(0.45f, 0.5f, 0.45f));
        foreach (var hit in hits)
        {   // Per ogni collider trovato, cerchiamo un componente GridTileBase nel suo parent 
            GridTileBase tile = hit.GetComponentInParent<GridTileBase>();
            if (tile != null) return tile;
        }
        return null;
    }

    // Questo metodo serve a "snappare" una posizione alla griglia,
    // arrotondando le coordinate x e z al multiplo più vicino della dimensione della griglia
    Vector3 Snap(Vector3 pos) => new Vector3(Mathf.Round(pos.x / grid.tileDistance) * grid.tileDistance, pos.y, Mathf.Round(pos.z / grid.tileDistance) * grid.tileDistance);
    
    // Questo metodo cambia il layer di un oggetto e di tutti i suoi figli ricorsivamente,
    // in modo da gestire correttamente le interazioni e le collisioni durante il grab
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        // Cambia layer a tutti i figli ricorsivamente
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, newLayer);
    }

    // Questo metodo viene chiamato da PlayerInteract quando il player interagisce con un oggetto afferrabile,
    // e gestisce tutta la logica di inizio del grab, inclusa la posizione iniziale,la rotazione del player verso l'oggetto,
    // la modifica dei layer e l'aggiornamento dello stato di interazione
    public void StartGrab(MovableObject obj)
    {
        // Se stiamo già afferrando un oggetto, ignoriamo il nuovo tentativo di grab
        // per evitare bug e conflitti tra oggetti
        if (isGrabbing)
        {
            Debug.LogWarning("Tentativo di afferrare ridondante.");
            return;
        }

        grabbedObject = obj;

        // snap del blocco nella griglia
        Vector3 objPos = Snap(obj.transform.position);
        grabbedObject.transform.position = objPos;

        // snap del player nella griglia
        // Prima di calcolare la faccia, forziamo anche il player a essere allineato alla griglia.
        // Questo elimina i millimetri di errore accumulati durante il movimento libero.
        Vector3 snappedPlayerPos = Snap(transform.position);

        // calcolo della direzione dopo la pulizia della posizione
        Vector3 directionFromBlockToPlayer = snappedPlayerPos - objPos;
        directionFromBlockToPlayer.y = 0f;
        // Determina il lato corretto del blocco 
        if (Mathf.Abs(directionFromBlockToPlayer.x) > Mathf.Abs(directionFromBlockToPlayer.z))
        {
            grabDirection = new Vector3(Mathf.Sign(directionFromBlockToPlayer.x), 0f, 0f);
        }
        else
        {
            grabDirection = new Vector3(0f, 0f, Mathf.Sign(directionFromBlockToPlayer.z));
        }

        // spostamento del player davanti al blocco
        Vector3 targetPlayerPos = objPos + grabDirection * grid.tileDistance;
        // Mantiene la Y originale del player
        targetPlayerPos.y = transform.position.y; 

        // disabilita temporaneamente il controller per evitare interferenze
        if (controller != null)
            controller.enabled = false;

        transform.position = targetPlayerPos;

        // ruota il player verso il blocco
        Vector3 lookDir = objPos - transform.position;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDir.normalized);
        }

        // riattiva il controller dopo aver posizionato e ruotato il player
        if (controller != null)
            controller.enabled = true;

        // inverte la direzione del grab in modo che punti dal player al blocco, per facilitare i calcoli di movimento e posizionamento durante il grab
        grabDirection = -grabDirection;

        // disabilitiamo il movimento e la rotazione del player durante il grab,
        // per evitare che il player possa muoversi o ruotare liberamente mentre sta afferrando un oggetto
        if (moveScript != null)
        {
            // Portiamo la velocità a 0 così lo script non muova il CharacterController
            moveScript.Velocity = 0;
            // Portiamo la rotazione a un valore impossibile (999) così non ruota il modello
            moveScript.allowPlayerRotation = 999f;
        }
        //aggiorna lo stato e ottiene il layer originale del blocco
        isGrabbing = true;
        originalLayer = obj.gameObject.layer;
        // Cambia il layer dell'oggetto afferrato (e dei suoi figli) in un layer specifico 
        // Se l'oggetto ha un LaserNode come figlio, gli assegniamo il layer "Grabbed".
        // Altrimenti, se è un oggetto inerte, gli assegniamo il layer "GrabbedInert"
        string targetLayer = obj.GetComponentInChildren<LaserNode>() != null
            ? movableLaserLayerName
            : movableInertLayerName;

        SetLayerRecursively(obj.gameObject, LayerMask.NameToLayer(targetLayer));

        // Logghiamo l'inizio del grab, il nome dell'oggetto afferrato e il layer assegnato 
        Debug.Log($"<color=green>Grab iniziato su {obj.name}. Layer assegnato: {targetLayer}</color>");
        // Aggiorniamo lo stato di interazione del player per riflettere che stiamo afferrando un oggetto,
        if (playerInteract != null)
        {
            // Impostiamo lo stato di interazione a true per indicare che il player sta interagendo con un oggetto
            playerInteract.SetInteracting(true);
            playerInteract.ClearCurrentState();
        }
        // Mostriamo la griglia durante il grab per aiutare il player a posizionare l'oggetto in modo preciso,
        if (grid != null)
        {
            //mostriamo la griglia
            grid.SetGridVisible(true);
            //rendiamo la griglia non togglable, in modo che rimanga sempre visibile durante il grab
            //finché non viene rilasciato l'oggetto
            grid.SetGridTogglable(false);
        }
        else
        {
            Debug.LogError("PlayerGrab: GridRuntime instance not found! Assicurati che ci sia un oggetto con GridRuntime nella scena.");
        }
    }
    // Questo metodo gestisce la logica di fine del grab, ripristinando lo stato del sistema
    public void StopGrab()
    {
        // Se non stiamo afferrando nulla, non facciamo nulla
        if (grabbedObject != null)
        {
            // Ripristina il layer originale
            SetLayerRecursively(grabbedObject.gameObject, originalLayer);
            Debug.Log($"<color=red>Grab terminato. Layer ripristinato a {originalLayer}</color>");
        }

        grabbedObject = null;
        // Ripristiniamo lo stato di interazione e movimento del player
        if (playerInteract != null) playerInteract.SetInteracting(false);
        if (moveScript != null)
        {
            // Ripristiniamo i valori originali
            moveScript.Velocity = originalVelocity;
            moveScript.allowPlayerRotation = originalAllowRotation;
        }
        if (grid != null)
        {
            //nascondiamo la griglia
            grid.SetGridVisible(false);
            //rendiamo la griglia togglable di nuovo, in modo che il player possa mostrarla o nasconderla in runtime dopo aver rilasciato l'oggetto
            grid.SetGridTogglable(true);
        }
        else { 
            Debug.LogError("PlayerGrab: GridRuntime non trovato! Assicurati che ci sia un oggetto con GridRuntime nella scena.");
        }
            isGrabbing = false;
    }
    // gestisce la rotazione di un oggetto di 90 gradi attorno all'asse Y, con un'animazione fluida usando Slerp
    IEnumerator RotateObject()
    {
        // Se stiamo già ruotando o muovendo, non accettiamo nuovi input di rotazione finché non finisce l'azione attuale
        if (isRotating || isMoving)
        {
            Debug.LogWarning("Tentativo di ruotare un oggetto mentre è già in movimento o rotazione!");
            yield break;
        }
        // Se non c'è un oggetto afferrato, non possiamo ruotare nulla, quindi logghiamo un avviso e usciamo dalla coroutine
        if (grabbedObject == null)
        {
            Debug.LogWarning("Tentativo di ruotare un oggetto quando non ce n'è uno afferrato!");
            yield break;
        }
        isRotating = true;
        
        Quaternion startRot = grabbedObject.transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 90, 0);
        //0f= movimento all'inizio, 1f= movimento alla fine
        float completamentoMovimento = 0;
        //calcola la durata della rotazione in base alla distanza angolare da percorrere,
        //in modo che la rotazione sia più lenta se l'oggetto è molto disallineato e più veloce se è già quasi allineato
        while (completamentoMovimento < 1)
        {
            completamentoMovimento += Time.deltaTime * 5f;
            grabbedObject.transform.rotation = Quaternion.Slerp(startRot, endRot, completamentoMovimento);
            yield return null;
        }
        // Assicuriamoci che alla fine della rotazione l'oggetto sia esattamente alla rotazione finale,
        // per evitare problemi di precisione dovuti all'interpolazione
        grabbedObject.transform.rotation = endRot;
        isRotating = false;
    }
}