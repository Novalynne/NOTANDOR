using UnityEngine;
// Questo script rappresenta un riflettore che può essere colpito da un raggio laser.
public class Reflector : LaserNode, ILaserReceiver
{
    [Header("Reflector Settings")]
    // Questa direzione è definita in LOCALE rispetto al cubo.
    public Vector3 reflectDirection = Vector3.right;
    // Se true, il riflettore rifletterà il raggio nella direzione specificata da reflectDirection.
    public bool reflecting = true;
    // Variabili per tenere traccia dello stato del raggio in questo frame
    protected bool isHitThisFrame = false;
    protected Vector3 currentEmitDirection;
    protected LaserColor currentIncomingColor;

    public virtual void OnLaserHit(Vector3 hitPoint, LaserColor incomingColor)
    {
        // Segnaliamo che siamo stati colpiti in questo frame e memorizziamo il colore del raggio in arrivo
        isHitThisFrame = true;
        currentIncomingColor = incomingColor;
        // Chiamiamo la funzione di calcolo della direzione di uscita basata sul punto di impatto
        currentEmitDirection = CalculateOutputDirection(hitPoint);
    }

    // Questa funzione calcola la direzione di uscita del raggio in base al punto di impatto e alla configurazione del riflettore.
    protected Vector3 CalculateOutputDirection(Vector3 hitPoint)
    {
        if (reflecting)
        {
            // Se stiamo riflettendo, usiamo la direzione di riflessione definita in locale.
            return transform.TransformDirection(reflectDirection).normalized;
        }
        else
        {
            // Se non stiamo riflettendo, vogliamo che il raggio esca perpendicolarmente alla faccia colpita.
            Vector3 localHit = transform.InverseTransformPoint(hitPoint);

            Vector3 localNormal = Vector3.zero;

            // Determiniamo quale faccia del cubo è stata colpita confrontando le coordinate locali del punto di impatto.
            if (Mathf.Abs(localHit.x) > Mathf.Abs(localHit.z))
                localNormal = new Vector3(Mathf.Sign(localHit.x), 0, 0);
            else
                localNormal = new Vector3(0, 0, Mathf.Sign(localHit.z));

            // La direzione di uscita sarà opposta alla normale locale, trasformata in spazio mondiale.
            return transform.TransformDirection(-localNormal).normalized;
        }
    }
    // Nel LateUpdate, se siamo stati colpiti in questo frame emettiamo il raggio nella direzione calcolata
    protected virtual void LateUpdate()
    {
        if (isHitThisFrame)
            FireRay(LaserOrigin, currentEmitDirection, currentIncomingColor);
        else
            ClearLaser();

        isHitThisFrame = false;
    }
}