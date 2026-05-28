using UnityEngine;

// Base class per tile, con funzioni virtuali per modificare il movimento, bloccare o no, e alterare la velocità
public abstract class GridTileBase : MonoBehaviour
{
    public float alteredSpeed = 1.0f;
    [SerializeField] protected bool continueMovement = false;


    // modifica il movimento
    public virtual Vector3 ModifyMovement(Vector3 currentPos, ref Vector3 direction, float gridSize, LayerMask obstacleMask)
    {
        return currentPos + direction * gridSize;
    }
    // continua o no il movimento
    public virtual bool ShouldContinue()
    {
        return continueMovement;
    }
    // blocca o no il movimento
    public virtual bool IsBlocking()
    {
        return false;
    }

    // altera la velocità
    public virtual float GetMoveSpeed(float baseSpeed)
    {
        return baseSpeed * alteredSpeed;
    }
}