using UnityEngine;

// Interfaccia oggetti colpibili dal laser.
// Il LaserNode non deve più sapere cosa sta colpendo
public interface ILaserReceiver
{
    void OnLaserHit(Vector3 hitPoint, LaserColor incomingColor);
}