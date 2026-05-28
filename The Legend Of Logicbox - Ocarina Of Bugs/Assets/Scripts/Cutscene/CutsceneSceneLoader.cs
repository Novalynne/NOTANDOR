using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class CutsceneSceneLoader : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector timeline;

    [Header("Scena successiva")]
    public string nextSceneName;

    [Header("Fade (opzionale, solo se vuoi fade to black)")]
    public bool fadeOut = false;
    public float fadeDuration = 1f;

    private ScreenFader fader;

    void Start()
    {
        // Cerca o crea il fader
        fader = FindObjectOfType<ScreenFader>();
        if (fader == null && fadeOut)
        {
            GameObject faderGO = new GameObject("ScreenFader");
            fader = faderGO.AddComponent<ScreenFader>();
        }

        // Fade in all'avvio della scena (schermo che si schiarisce)
        if (fader != null)
            StartCoroutine(fader.FadeIn(fadeDuration));

        // Avvia la Timeline e aspetta che finisca
        if (timeline != null)
        {
            timeline.Play();
            StartCoroutine(WaitForTimeline());
        }
        else
        {
            Debug.LogError("[CutsceneSceneLoader] Nessuna Timeline assegnata!");
        }
    }

    private IEnumerator WaitForTimeline()
    {
        // Aspetta la durata della Timeline
        yield return new WaitForSeconds((float)timeline.duration);

        if (fadeOut && fader != null)
            yield return StartCoroutine(fader.FadeOut(fadeDuration));

        SceneManager.LoadScene(nextSceneName);
    }
}