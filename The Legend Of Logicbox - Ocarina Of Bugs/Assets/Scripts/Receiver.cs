using UnityEngine;
using System.Collections.Generic;

public class Receiver : LaserNode
{
    public LaserColor requiredColor = LaserColor.Red;

    //public List<Activatable> targets = new List<Activatable>();

    private bool alreadyActivatedThisFrame = false;

    public void ReceiveLaser(LaserColor color)
    {
        if (color != requiredColor)
            return;

        // evita multi-trigger nello stesso frame
        if (alreadyActivatedThisFrame)
            return;

        alreadyActivatedThisFrame = true;

        Illuminate(true);

        /*foreach (var t in targets)
        {
            if (t != null)
                t.Activate();
        }*/
    }

    void LateUpdate()
    {
        alreadyActivatedThisFrame = false;
    }
}