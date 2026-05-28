using UnityEngine;

// Logica OR: se almeno uno degli ingressi è verde, l'uscita è verde, altrimenti è rossa
public class LogicOr : LogicGate
{
    // numero di ingressi reali ricevuti in questo frame
    private int currentInputs = 0;
    // flag per tenere traccia se abbiamo trovato almeno un ingresso verde in questo frame
    private bool foundGreen = false;
    [Header("OR Gate Settings")]
    // Numero di ingressi reali richiesti per valutare la logica (es. 2 per una porta OR a 2 ingressi)
    public int requiredInputs = 2;
    

    public override void OnLaserHit(Vector3 hitPoint, LaserColor incomingColor)
    {
        // Prima di tutto chiamiamo la logica base per gestire i flag isHit e isPreview
        base.OnLaserHit(hitPoint, incomingColor);

       
        if (incomingColor != LaserColor.Preview)
        {
            // Incrementiamo il contatore degli ingressi reali
            currentInputs++;
            //controlliamo se l'ingresso è verde
            if (incomingColor == LaserColor.Green)
            {
                foundGreen = true;
            }
        }
    }

    protected override LaserColor EvaluateLogic()
    {
        if (currentInputs < requiredInputs)
        {
            // Se non abbiamo ancora ricevuto abbastanza ingressi reali, rimaniamo in stato di preview
            return LaserColor.Preview;
        }
        // Se almeno uno degli ingressi reali era verde, l'uscita è verde altrimenti è rosso
        return foundGreen ? LaserColor.Green : LaserColor.Red;
    }

    protected override void ResetGate()
    {
        // Reset del flag foundGreen e del contatore degli ingressi alla fine di ogni frame
        currentInputs = 0;
    }
}