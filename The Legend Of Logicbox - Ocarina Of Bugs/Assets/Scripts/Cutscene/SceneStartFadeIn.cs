using System.Collections;
using UnityEngine;

public class SceneStartFadeIn : MonoBehaviour
{
    public float fadeDuration = 1f;

    IEnumerator Start()
    {
        var fader = FindObjectOfType<ScreenFader>();
        if (fader != null)
            yield return StartCoroutine(fader.FadeIn(fadeDuration));
    }
}