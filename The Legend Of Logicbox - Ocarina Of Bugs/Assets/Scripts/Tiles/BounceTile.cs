using UnityEngine;

//Bounce tile: quando il player ci cammina sopra, viene respinto indietro nella direzione da cui è venuto, come se rimbalzasse
public class BounceTile : GridTileBase
{
    public void Start()
    {
        // Non modifichiamo la velocità, ma permettiamo al movimento di continuare (ma invertito)
        continueMovement = true;
    }
    

    // questo metodo per segnala al PlayerGrab che deve tornare indietro
    public override Vector3 ModifyMovement(Vector3 currentPos, ref Vector3 direction, float gridSize, LayerMask obstacleMask)
    {
        direction = -direction; // Invertiamo la direzione
        Debug.Log($"<color=magenta>BounceTile: Invertendo direzione a {direction}</color>");
        return currentPos + direction * gridSize;
    }
}