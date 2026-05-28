using UnityEngine;

// Tile che rappresenta una superficie di ghiaccio, che fa scivolare il player in avanti finché non incontra un ostacolo o un tile diversa
public class IceTile : GridTileBase
{
    private void Start()
    {
        // Il tile di ghiaccio fa continuare il movimento del player e aumenta la velocità di scivolamento
        alteredSpeed = 1.5f;
        continueMovement = true;
    }
}