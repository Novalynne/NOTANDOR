using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class DoorCutsceneLoader : MonoBehaviour
{
    [Header("Trigger")]
    public Collider triggerZone;

    [Header("Cutscene")]
    public PlayableDirector timeline;

    [Header("Fade & Scena")]
    public float fadeDuration = 1f;
    public string nextSceneName;

    private ScreenFader fader;
    private bool triggered = false;

    void Start()
    {
        fader = FindObjectOfType<ScreenFader>();
        if (fader == null)
        {
            GameObject faderGO = new GameObject("ScreenFader");
            fader = faderGO.AddComponent<ScreenFader>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            triggered = true;
            StartCoroutine(PlayCutscene());
        }
    }

    private IEnumerator PlayCutscene()
    {
        // Attiva il GameObject della cutscene prima di avviarla
        if (timeline != null)
        {
            timeline.gameObject.SetActive(true); // <-- aggiunto
            timeline.Play();
        }

        yield return new WaitForSeconds((float)timeline.duration);

        yield return StartCoroutine(fader.FadeOut(fadeDuration));

        SceneManager.LoadScene(nextSceneName);
    }
}