using UnityEngine;

public class NormalTile : GridTileBase
{
    // non fa nulla , ma è una tile su cui il player può camminare normalmente
     public void Start()
    {
        // Fa fermare il movimento del player quando entra in questa tile, se stava scivolando
        continueMovement = false;
    }
}