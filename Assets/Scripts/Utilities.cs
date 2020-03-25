using System;
using System.Collections;
using UnityEngine;

/*
 * Static class for general utility functions
 */
public static class Utilities
{

    /*
     * Fades out audio on given source over given time, triggers optional onComplete callbacks
     */
    public static IEnumerator FadeOutAudio(AudioSource audioSource, float time, Action onComplete = null)
    {
        float timePassed = 0f;
        float originalVolume = audioSource.volume;
        while (audioSource.volume > 0f)
        {
            audioSource.volume = Mathf.Max(0f, 1f - timePassed / time);
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        audioSource.Stop();
        audioSource.volume = originalVolume;
        if (onComplete != null)
        {
            onComplete.Invoke();
        }
    }
}