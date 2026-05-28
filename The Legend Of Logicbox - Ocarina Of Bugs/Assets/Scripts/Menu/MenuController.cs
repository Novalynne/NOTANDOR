using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Localization.Settings;
using System.Security.AccessControl;

public class MenuController : MonoBehaviour
{
    [Header("Volume Settings")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float defaultVolume = 50.0f;
    [SerializeField] private TMP_Text mainVolumeValue = null;
    [SerializeField] private Slider mainVolumeSlider;
    [SerializeField] private TMP_Text musicVolumeValue = null;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private TMP_Text soundEffectsVolumeValue = null;
    [SerializeField] private Slider soundEffectsVolumeSlider;
    AudioManager audioManager;

    [Header("GamePlay Settings")]
    [SerializeField] private Toggle invertYToggle = null;
    [SerializeField] private TMP_Text sensitivityValue = null;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private float defaultSensitivity = 9.0f;
    public float mouseSensitivity = 9.0f;

    [Header("Graphics Settings")]
    [SerializeField] private Toggle fullscreenToggle = null;
    [SerializeField] private TMP_Text brightnessValue = null;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private float defaultBrightness = 50f;
    [SerializeField] private Volume volume;
    private ColorAdjustments colorAdjustments;
    [SerializeField] private TMP_Dropdown qualityDropDown;
    private int qualityLevel;
    private bool isFullScreenValue;
    private float brightnessLevel;
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions; // Array to hold available screen resolutions


    [Header("Levels To Load")]
    public string newgameLevel;
    private string levelToLoad;
    [SerializeField] private GameObject noSavedGamePopUp = null;
    [SerializeField] private GameObject confirmationPrompt = null;
    SavingController savingController;
    [SerializeField] private GameObject saveMenuContainer = null;

    [Header("Camera MovementInput Reference")]
    [SerializeField] private MovementInput movementInput = null;

    // AWAKE METHOD TO FIND THE AUDIO MANAGER FIRST WHEN THE GAME STARTS
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        savingController = GetComponent<SavingController>();
    }

    // START METHOD TO SET THE SETTINGS TO THE SAVED VALUES OR DEFAULTS
    private void Start()
    {
        // RESET CURSOR
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // VOLUME SETTINGS
        if(PlayerPrefs.HasKey("MusicVolume") && PlayerPrefs.HasKey("MainVolume") && PlayerPrefs.HasKey("SoundEffectsVolume"))
        {
            LoadVolume();
        }
        else
        {
            DefaultVolume();
        }
        SetVolume();

        // GAMEPLAY SETTINGS
        if(PlayerPrefs.HasKey("MouseSensitivity") && PlayerPrefs.HasKey("InvertY"))
        {
            LoadGameplaySettings();
        }
        else
        {
            DefaultGameplaySettings();
        }
        SetMouseSensitivity(sensitivitySlider.value);

        // GRAPHICS SETTINGS
        volume.profile.TryGet(out colorAdjustments);
        GetResolutionDevice();
        if(PlayerPrefs.HasKey("Brightness") && PlayerPrefs.HasKey("FullScreen") && PlayerPrefs.HasKey("QualityLevel") && PlayerPrefs.HasKey("ResolutionIndex"))
        {
            LoadVideoSettings();
        }
        else
        {
            DefaultVideoSettings();
        }
        SetGraphicsSettings();

        // LANGUAGE SETTINGS
        if(PlayerPrefs.HasKey("Language"))
        {
            LoadLanguage();
        }
        else
        {
            DefaultLanguage();
        }

        // SAVING SLOTS
        if(savingController != null)
        {
            savingController.LoadValuesFromSlot();
        }
    }
    
    public void NewGame()
    {
        PlayButtonSoundEffect();
        GameState.IsPaused = false;
        SceneManager.LoadScene(newgameLevel);
    }

    public void LoadGame(int slot)
    {
        PlayButtonSoundEffect();
        GameState.IsPaused = false;
        if(PlayerPrefs.HasKey($"SaveSlot{slot}_Level"))
        {
            levelToLoad = PlayerPrefs.GetString($"SaveSlot{slot}_Level");
            SceneManager.LoadScene(levelToLoad);
        }
        else
        {
            noSavedGamePopUp.SetActive(true);
            saveMenuContainer.SetActive(false);
        }
    }

    public void QuitGame()
    {
        PlayButtonSoundEffect();
        Application.Quit();
    }

    // VOLUME SETTINGS METHODS
    public void SetMainVolume(float volume)
    {
        mainVolumeValue.text = volume.ToString("0.0");

        // Convert the linear volume (0-100) to decibels for the audio mixer
        float normalized = volume / 100f;
        normalized = Mathf.Clamp(normalized, 0.0001f, 1f);
        float db = Mathf.Log10(normalized) * 20;
        
        audioMixer.SetFloat("master", db);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolumeValue.text = volume.ToString("0.0");

        // Convert the linear volume (0-100) to decibels for the audio mixer
        float normalized = volume / 100f;
        normalized = Mathf.Clamp(normalized, 0.0001f, 1f);
        float db = Mathf.Log10(normalized) * 20;

        audioMixer.SetFloat("music", db);
    }

    public void SetSoundEffectsVolume(float volume)
    {
        soundEffectsVolumeValue.text = volume.ToString("0.0");

        // Convert the linear volume (0-100) to decibels for the audio mixer
        float normalized = volume / 100f;
        normalized = Mathf.Clamp(normalized, 0.0001f, 1f);
        float db = Mathf.Log10(normalized) * 20;

        audioMixer.SetFloat("soundEffect", db);
    }

    private void LoadVolume()
    {
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        mainVolumeSlider.value = PlayerPrefs.GetFloat("MainVolume");
        soundEffectsVolumeSlider.value = PlayerPrefs.GetFloat("SoundEffectsVolume");
    }

    private void DefaultVolume()
    {
        mainVolumeSlider.value = defaultVolume;
        musicVolumeSlider.value = defaultVolume;
        soundEffectsVolumeSlider.value = defaultVolume;
    }

    private void SetVolume()
    {
        SetMusicVolume(musicVolumeSlider.value);
        SetMainVolume(mainVolumeSlider.value);
        SetSoundEffectsVolume(soundEffectsVolumeSlider.value);
    }

    public void VolumeApply()
    {
        PlayButtonSoundEffect();
        PlayerPrefs.SetFloat("MainVolume", mainVolumeSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SoundEffectsVolume", soundEffectsVolumeSlider.value);
        // don't know if its aestetically pleasing to have the confirmation box pop up
        //StartCoroutine(ConfirmationBox());
    }

    public void PlayButtonSoundEffect()
    {
        if (audioManager)
        {
            audioManager.PlaySoundEffect(audioManager.buttonPressedSoundEffect);
        }
    }

    // GAMEPLAY SETTINGS METHODS
    public void SetMouseSensitivity(float sensitivity)
    {
        sensitivityValue.text = sensitivity.ToString("0.0");
        mouseSensitivity = sensitivity;
    }

    public void GamePlayApply()
    {
        PlayButtonSoundEffect();
        PlayerPrefs.SetFloat("MouseSensitivity", mouseSensitivity);
        PlayerPrefs.SetInt("InvertY", invertYToggle.isOn ? 1 : 0);
        if (movementInput != null)
        {
            movementInput.OnSettingsChanged();
        }
    }

    private void LoadGameplaySettings()
    {
        sensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity");
        invertYToggle.isOn = PlayerPrefs.GetInt("InvertY") == 1 ? true : false;
    }

    private void DefaultGameplaySettings()
    {
        sensitivitySlider.value = defaultSensitivity;
        invertYToggle.isOn = false;
    }

    // GRAPHICS SETTINGS METHODS
    public void SetBrightness(float brightness)
    {
        brightnessValue.text = brightness.ToString("0.0");
        brightnessLevel = brightness;
        if (colorAdjustments == null)
        {
            Debug.LogError("ColorAdjustments non trovato!");
            return;
        }
        float normalized = Mathf.Lerp(-4f, 2f, brightness / 100f);
        colorAdjustments.postExposure.value = normalized;
    }

    public void SetFullScreen(bool isFullScreen)
    {
        isFullScreenValue = isFullScreen;
        Screen.fullScreen = isFullScreenValue;
    }

    public void SetQuality(int qualityIndex)
    {
        qualityLevel = qualityIndex;
        qualityDropDown.value = qualityLevel;
        QualitySettings.SetQualityLevel(qualityLevel, false);
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    private void GetResolutionDevice()
    {
        // GET RESOLUTIONS FOR THE DEVICE AND SET THE DROPDOWN
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>(); // List to hold the resolution options for the dropdown
        int currentResolutionIndex = 0;
        for(int i=0; i<resolutions.Length; i++)
        {
            // Create a string representation of the resolution (e.g., "1920 x 1080") and add it to the options list
            string option = resolutions[i].width + " x " + resolutions[i].height; 
            options.Add(option);

            if(resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    private void LoadVideoSettings()
    {
        brightnessSlider.value = PlayerPrefs.GetFloat("Brightness");
        fullscreenToggle.isOn = PlayerPrefs.GetInt("FullScreen") == 1 ? true : false;
        qualityLevel = PlayerPrefs.GetInt("QualityLevel");
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex");
    }

    private void DefaultVideoSettings()
    {
        brightnessSlider.value = defaultBrightness;
        fullscreenToggle.isOn = true;
        qualityLevel = 1;
        Resolution currentResolution = Screen.currentResolution;
        Screen.SetResolution(currentResolution.width, currentResolution.height, Screen.fullScreen);
        resolutionDropdown.value = resolutions.Length;
    }

    private void SetGraphicsSettings()
    {
        SetBrightness(brightnessSlider.value);
        SetFullScreen(fullscreenToggle.isOn);
        SetQuality(qualityLevel);
        SetResolution(resolutionDropdown.value);
    }

    public void GraphicsApply()
    {
        PlayButtonSoundEffect();
        PlayerPrefs.SetFloat("Brightness", brightnessLevel);
        PlayerPrefs.SetInt("FullScreen", isFullScreenValue ? 1 : 0);
        PlayerPrefs.SetInt("QualityLevel", qualityLevel);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
    }

    // LANGUAGES SETTINGS METHODS
    private bool isLanguageChanging = false;
    IEnumerator SetLanguage(int languageIndex)
    {
        isLanguageChanging = true;
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[languageIndex];
        isLanguageChanging = false;
    }

    public void ChangeLanguage(int languageIndex)
    {
        PlayButtonSoundEffect();
        if (isLanguageChanging) return;
        StartCoroutine(SetLanguage(languageIndex));
        PlayerPrefs.SetInt("Language", languageIndex);
    }

    private void LoadLanguage()
    {
        int languageIndex = PlayerPrefs.GetInt("Language");
        StartCoroutine(SetLanguage(languageIndex));
    }

    private void DefaultLanguage()
    {
        StartCoroutine(SetLanguage(0));
    }

    // RESET SETTINGS METHODS
    public void ResetAudio()
    {
        DefaultVolume();
        SetVolume();
        VolumeApply();
        
    }

    public void ResetVideo()
    {
        DefaultVideoSettings();
        SetGraphicsSettings();
        SetFullScreen(fullscreenToggle.isOn);
        GraphicsApply();
    }

    public void ResetGameplay()
    {
        DefaultGameplaySettings();
        SetMouseSensitivity(sensitivitySlider.value);
        GamePlayApply();
    }

    // CONFIRMATION NOTIFICATION METHOD
    public IEnumerator ConfirmationBox()
    {
        confirmationPrompt.SetActive(true);
        yield return new WaitForSeconds(2);
        confirmationPrompt.SetActive(false);
    }

}
