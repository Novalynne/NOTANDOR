using UnityEngine;
// Questa classe astratta centralizza tutta la logica di sparo, disegno e illuminazione dei laser
//lista dei colori dei laser, incluso il colore semitrasparente per la modalità Preview
public enum LaserColor { Red, Green, Preview }

// Richiede che ogni nodo laser abbia un LineRenderer per disegnare il raggio e un Renderer per gestire l'illuminazione
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(Renderer))]
// Tutte le classi che emettono o riflettono laser erediteranno da questa classe,
public abstract class LaserNode : MonoBehaviour
{
    [Header("Laser Settings")]
    // La distanza massima del laser è ora un campo pubblico che può essere modificato per ogni nodo
    public float maxDistance = 20f;
    // LayerMask per filtrare i colpi del laser, separati in due: uno per i riceventi (laserMask) e uno per gli ostacoli (blockMask)
    public LayerMask laserMask;
    public LayerMask blockMask;
    [Header("Laser Color")]
    // Il colore del laser emesso da questo nodo, che può essere impostato per ogni nodo individualmente
    public LaserColor laserColor = LaserColor.Red;
    [Header("Origin Offset")]
    // L'offset del punto di origine del laser rispetto al centro del nodo
    public Vector3 laserOffset = Vector3.zero;
    // Questo restituisce la posizione esatta da cui parte il laser, combinando la posizione del nodo con l'offset
    public Vector3 LaserOrigin => transform.TransformPoint(laserOffset);
    // Riferimenti al LineRenderer e al Renderer del materiale, per disegnare il laser e gestire l'illuminazione
    protected LineRenderer lineRenderer;
    protected Renderer rend;

    protected virtual void Awake()
    {
        // Inizializziamo i componenti una sola volta, così tutte le classi figlie li avranno già pronti
        lineRenderer = GetComponent<LineRenderer>();
        rend = GetComponent<Renderer>();
        if (lineRenderer != null) lineRenderer.enabled = false;
    }

    // Tutte le classi useranno questo metodo per creare il raggio laser, che gestisce sia il raycast che il disegno e l'illuminazione
    protected void FireRay(Vector3 origin, Vector3 direction, LaserColor color)
    {
        Illuminate(color != LaserColor.Preview, color);
        // Il raycast parte dall'origine fisica (centro del nodo), ma il disegno parte dall'origine visuale (offset)
        Vector3 physicsOrigin = origin;
        Vector3 endPoint = physicsOrigin + direction * maxDistance;
        // Combiniamo i layer mask per colpire sia i riceventi che gli ostacoli
        LayerMask combinedMask = laserMask | blockMask;
        RaycastHit hit;

        // Eseguiamo il raycast 
        if (Physics.Raycast(physicsOrigin, direction, out hit, maxDistance, combinedMask))
        {
            // Se colpisce qualcosa, aggiorniamo il punto finale del laser al punto di impatto
            endPoint = hit.point;
            int layer = hit.collider.gameObject.layer;
            // Controlliamo se l'oggetto colpito è in uno dei layer del laser (riceventi)
            if (((1 << layer) & laserMask) != 0)
            {
                // Se colpisce un oggetto nel layer del laser, proviamo a ottenere il componente ILaserReceiver
                ILaserReceiver receiver = hit.collider.GetComponent<ILaserReceiver>();
                if (receiver != null)
                {
                    receiver.OnLaserHit(hit.point, color);
                }
            }
        }
        // Disegniamo il laser dal punto di origine visuale (offset) al punto finale calcolato
        DrawLaser(origin, endPoint, color);
    }

    // Questo metodo restituisce il colore corretto in base al tipo di laser, incluso il colore semitrasparente per la modalità Preview
    protected Color GetColor(LaserColor color)
    {
        switch (color)
        {
            case LaserColor.Red: return Color.red;
            case LaserColor.Green: return Color.green;
            case LaserColor.Preview: return new Color(1f, 1f, 1f, 0.15f);
            default: return Color.white;
        }
    }
    // Questo metodo disegna il laser usando il LineRenderer, impostando i colori e i punti di partenza e fine
    protected void DrawLaser(Vector3 start, Vector3 end, LaserColor color)
    {
        // Se il LineRenderer non è stato trovato, non facciamo nulla
        if (lineRenderer == null) return;
        lineRenderer.enabled = true;

        Color c = GetColor(color);
        lineRenderer.startColor = c;
        lineRenderer.endColor = c;
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }
    // Questo metodo spegne il laser e l'illuminazione, usato quando il nodo non sta sparando un laser in questo frame
    protected void ClearLaser()
    {
        if (lineRenderer != null) lineRenderer.enabled = false;

        // Spegne l'illuminazione
        Illuminate(false);
    }

    //Ora accetta il colore e lo applica al materiale 
    protected void Illuminate(bool on, LaserColor color = LaserColor.Preview)
    {
        if (rend == null) return;

        if (on && color != LaserColor.Preview)
        {
            // Abilita l'emissione e imposta il colore di emissione in base al colore del laser
            rend.material.EnableKeyword("_EMISSION");
            // Usa GetColor(color) e moltiplica per 2f per ricreare l'effetto "bagliore" luminoso
            rend.material.SetColor("_EmissionColor", GetColor(color) * 2f);
        }
        else
        {
            rend.material.DisableKeyword("_EMISSION");
        }
    }
}