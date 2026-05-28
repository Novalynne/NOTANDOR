using UnityEngine;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/DialogueData")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class Line
    {
        public string speakerName;
        public LocalizedString localizedText;
    }

    public Line[] lines;
}