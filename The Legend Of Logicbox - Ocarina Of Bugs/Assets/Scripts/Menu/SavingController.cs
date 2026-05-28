using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SavingController : MonoBehaviour
{
    [Header("Saving Settings")]
    [SerializeField] private GameObject savingMenu = null;
    [SerializeField] private GameObject saveMenuContainer = null;

    [Header("Level Datestamps")]
    [SerializeField] private TMP_Text levelSlot1Text = null;
    [SerializeField] private TMP_Text levelSlot2Text = null;
    [SerializeField] private TMP_Text levelSlot3Text = null;

    [Header("Time Datestamps")]
    [SerializeField] private TMP_Text timeSlot1Text = null;
    [SerializeField] private TMP_Text timeSlot2Text = null;
    [SerializeField] private TMP_Text timeSlot3Text = null;

    [Header("Overwrite/Delete Confirmation")]
    [SerializeField] private GameObject confirmationMenu = null;
    AudioManager audioManager;
    private int selectedSlot = -1;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    public void PlayButtonSoundEffect()
    {
        if (audioManager)
        {
            audioManager.PlaySoundEffect(audioManager.buttonPressedSoundEffect);
        }
    }

    public void OpenSaveMenu()
    {
        PlayButtonSoundEffect();
        GameState.IsSaveMenuOpen = true;
        LoadValuesFromSlot();
        savingMenu.SetActive(true);
        Time.timeScale = 0f;

        GameState.IsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseSaveMenu()
    {
        PlayButtonSoundEffect();
        GameState.IsSaveMenuOpen = false;
        savingMenu.SetActive(false);
        Time.timeScale = 1f;

        GameState.IsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void LoadValuesFromSlot()
    {
        levelSlot1Text.text = PlayerPrefs.GetString("SaveSlot1_Level", "-");
        levelSlot2Text.text = PlayerPrefs.GetString("SaveSlot2_Level", "-");
        levelSlot3Text.text = PlayerPrefs.GetString("SaveSlot3_Level", "-");

        timeSlot1Text.text = PlayerPrefs.GetString("SaveSlot1_Time", "-");
        timeSlot2Text.text = PlayerPrefs.GetString("SaveSlot2_Time", "-");
        timeSlot3Text.text = PlayerPrefs.GetString("SaveSlot3_Time", "-");
    }

    public void AskOverwriteSave(int slot)
    {
        PlayButtonSoundEffect();
        if (PlayerPrefs.HasKey($"SaveSlot{slot}_Level"))
        {
            selectedSlot = slot;
            confirmationMenu.SetActive(true);
            saveMenuContainer.SetActive(false);
        }
        else
        {
            SaveToSlot(slot);
        }
    }

    public void ConfirmOverwrite()
    {
        PlayButtonSoundEffect();
        if(selectedSlot == -1)
            return;

        SaveToSlot(selectedSlot);
        selectedSlot = -1;
        confirmationMenu.SetActive(false);
        saveMenuContainer.SetActive(true);
    }

    public void CancelOverwrite()
    {
        PlayButtonSoundEffect();
        selectedSlot = -1;
        confirmationMenu.SetActive(false);
        saveMenuContainer.SetActive(true);
    }

    public void SaveToSlot(int slot)
    {
        string currentLevel = SceneManager.GetActiveScene().name;
        string currentTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        PlayerPrefs.SetString($"SaveSlot{slot}_Level", currentLevel);
        PlayerPrefs.SetString($"SaveSlot{slot}_Time", currentTime);

        LoadValuesFromSlot();
    }

    public void AskDeleteSave(int slot)
    {
        PlayButtonSoundEffect();
        if (!PlayerPrefs.HasKey($"SaveSlot{slot}_Level"))
            return;
        selectedSlot = slot;
        confirmationMenu.SetActive(true);
        saveMenuContainer.SetActive(false);
    }

    public void ConfirmDelete()
    {
        PlayButtonSoundEffect();
        if(selectedSlot == -1)
            return;

        DeleteSlot(selectedSlot);
        selectedSlot = -1;
        confirmationMenu.SetActive(false);
        saveMenuContainer.SetActive(true);
    }
    
    public void CancelDelete()
    {
        PlayButtonSoundEffect();
        selectedSlot = -1;
        confirmationMenu.SetActive(false);
        saveMenuContainer.SetActive(true);
    }

    public void DeleteSlot(int slot)
    {
        PlayerPrefs.DeleteKey($"SaveSlot{slot}_Level");
        PlayerPrefs.DeleteKey($"SaveSlot{slot}_Time");
        LoadValuesFromSlot();
    }

    public void LoadGame(int slot)
    {
        PlayButtonSoundEffect();
        GameState.IsPaused = false;
        GameState.IsSaveMenuOpen = false;
        string levelToLoad = PlayerPrefs.GetString($"SaveSlot{slot}_Level", null);
        if (!string.IsNullOrEmpty(levelToLoad))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(levelToLoad);
        }
        else
        {
            Debug.LogWarning($"No saved game found in slot {slot}!");
        }
    }
}
