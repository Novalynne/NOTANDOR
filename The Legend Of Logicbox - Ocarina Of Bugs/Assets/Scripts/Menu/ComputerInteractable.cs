using UnityEngine;

public class ComputerInteractable : Interactable
{
    [SerializeField] private SavingController savingController = null;

    protected override void Interact()
    {
        if (savingController != null)
        {
            savingController.OpenSaveMenu();
        }
        else
        {
            Debug.LogWarning("SavingController not assigned toComputerInteractable!");
        }
    }
}