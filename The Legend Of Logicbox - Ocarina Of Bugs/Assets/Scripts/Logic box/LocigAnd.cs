using UnityEngine;
// Logica AND: emette verde solo se riceve i raggi verdi richiesti
public class LogicAnd : LogicGate
{
    [Header("AND Gate Settings")]
    // input richiesti
    public int requiredInputs = 2;
    // contatori per tenere traccia del numero di raggi verdi e del numero totale di raggi ricevuti in questo frame
    private int currentGreen = 0;
    private int currentInputs = 0;

    public override void OnLaserHit(Vector3 hitPoint, LaserColor incomingColor)
    {
        // Chiama il metodo base
        base.OnLaserHit(hitPoint, incomingColor);

        // Conta il verde solo se il raggio non è una preview
        if (incomingColor != LaserColor.Preview)
        {
            if (incomingColor == LaserColor.Green)
                currentGreen++;
            // Incrementa il contatore degli input ricevuti, indipendentemente dal colore
            currentInputs++;
        }
    }

    protected override LaserColor EvaluateLogic()
    {
        // Se il numero di raggi ricevuti è inferiore a quelli richiesti, emetti una preview
        if (currentInputs != requiredInputs)
        {
            return LaserColor.Preview;
        }
        // Se il numero di raggi verdi ricevuti è uguale a quelli richiesti, emetti verde, altrimenti rosso
        return (currentGreen >= requiredInputs) ? LaserColor.Green : LaserColor.Red; 
    }

    protected override void ResetGate()
    {
        // Resetta i contatori dei raggi verdi e degli input quando la logica viene valutata
        currentGreen = 0; 
        currentInputs = 0;
    }
}