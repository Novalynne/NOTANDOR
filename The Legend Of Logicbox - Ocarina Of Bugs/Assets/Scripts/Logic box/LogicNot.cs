using UnityEngine;

// Logica NOT: inverte il colore del raggio reale che colpisce la porta
public class LogicNot : LogicGate
{
    // Memorizza l'ultimo colore reale che ha colpito la porta
    private LaserColor lastColor = LaserColor.Red;

    public override void OnLaserHit(Vector3 hitPoint, LaserColor incomingColor)
    {
        //aggiorna il colore reale solo se non è un raggio di anteprima
        if (incomingColor != LaserColor.Preview)
        {
            lastColor = incomingColor; 
        }
        // Chiama la logica base
        base.OnLaserHit(hitPoint, incomingColor);
    }

    protected override LaserColor EvaluateLogic()
    {
        // Inverte il colore reale memorizzato
        return lastColor == LaserColor.Red ? LaserColor.Green : LaserColor.Red;
    }
    // La porta NOT non ha uno stato interno da resettare, quindi questa funzione è vuota
    protected override void ResetGate() { }
}