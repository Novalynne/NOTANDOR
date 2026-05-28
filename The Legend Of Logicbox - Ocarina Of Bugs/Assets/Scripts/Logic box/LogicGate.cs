using UnityEngine;

// Questa classe è un template per tutte le porte logiche (AND, OR, XOR, etc.)
public abstract class LogicGate : Reflector
{   
    // flag per distinguere tra hit di preview e hit reali
    protected bool isPreviewFrame = false;
    protected bool isRealHitThisFrame = false;

    public override void OnLaserHit(Vector3 hitPoint, LaserColor incomingColor)
    {
        // Se il raggio è di tipo Preview, controlliamo se c'è già un hit reale in questo frame
        if (incomingColor == LaserColor.Preview)
        {
            // Se c'è già un raggio vero, ignora la preview altrimenti registra questo frame come preview
            if (isRealHitThisFrame) return; 
            isPreviewFrame = true;
        }
        else
        {
            isRealHitThisFrame = true;
        }
        // Se siamo qui, significa che abbiamo un hit valido (reale o preview) da processare
        isHitThisFrame = true;

        // Calcoliamo la direzione una volta sola usando la logica corretta del Reflector
        currentEmitDirection = CalculateOutputDirection(hitPoint);
    }

    protected override void LateUpdate()
    {
        // Se abbiamo un hit valido (reale o preview), emettiamo il raggio con il colore appropriato
        if (isHitThisFrame)
        {
            // Se è un hit reale, usiamo la logica normale, altrimenti usiamo il colore di preview
            LaserColor colorToEmit = (isRealHitThisFrame || !isPreviewFrame)
                                     ? EvaluateLogic()
                                     : LaserColor.Preview;
            // Emittiamo il raggio con il colore calcolato
            FireRay(LaserOrigin, currentEmitDirection, colorToEmit);
        }
        else
        {
            // Se non abbiamo un hit valido, assicuriamoci di non emettere nulla
            ClearLaser();
        }
        // Alla fine del frame, resettiamo tutto per il prossimo frame
        ResetGate();
        isRealHitThisFrame = false;
        isHitThisFrame = false;
        isPreviewFrame = false;
    }
    // Questo metodo astratto deve essere implementato da ogni porta logica per definire la propria logica di valutazione
    protected abstract LaserColor EvaluateLogic();
    // Questo metodo astratto deve essere implementato da ogni porta logica per resettare il proprio stato interno alla fine di ogni frame
    protected abstract void ResetGate();
}