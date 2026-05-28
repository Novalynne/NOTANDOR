using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenu = null;
    [SerializeField] private InputAction pause;
    private MenuController menuController = null;

    // AWAKE METHOD TO FIND THE MENU CONTROLLER FIRST WHEN THE GAME STARTS
    private void Awake()
    {
        menuController = GetComponent<MenuController>();
    }

    // ON ENABLE AND DISABLE FOR THE PAUSE INPUT ACTION
    private void OnEnable()
    {
        if (pause == null || pause.bindings.Count == 0)
        {
            pause = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
        }
        pause.Enable();
    }

    // UPDATE METHOD TO CHECK FOR THE PAUSE INPUT
    private void Update()
    {
        if (GameState.IsSaveMenuOpen)
        {
            return;
        }

        if (pause.WasPressedThisFrame())
        {
            if (!pauseMenu.activeSelf)
            {
                Pause();
            }
        }
    }

    public void BackToMainMenu()
    {
        menuController.PlayButtonSoundEffect();
        Time.timeScale = 1f;
        // CONSIDER ADDING A CONFIRMATION POP UP FOR THIS
        SceneManager.LoadScene("MainMenu");
    }

    public void Pause()
    {
        menuController.PlayButtonSoundEffect();
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;

        GameState.IsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        menuController.PlayButtonSoundEffect();
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        
        GameState.IsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void RestartLevel()
    {
        menuController.PlayButtonSoundEffect();
        Time.timeScale = 1f;
        GameState.IsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
