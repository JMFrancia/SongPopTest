using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*
 * Control script for the timer object
 */
public class Timer : MonoBehaviour
{
    [Tooltip("Time at which timer changes to warning color")]
    [SerializeField] float warningTime = 3f;
    [Tooltip("Transition time fading to warning color")]
    [SerializeField] float warningTimeTransition = 1f;
    [Header("Scene object references")]
    [SerializeField] Image bar;
    [SerializeField] Text timerText;
    [SerializeField] Color warningColor;

    //Current time on the clock
    public float time { get; private set; }

    //Callback for when timer complete
    public Action onComplete;

    Color originalColor;
    float originalTime;
    Coroutine timerCoroutine;

    private void Awake()
    {
        originalColor = bar.color;
    }

    /*
     * Sets and starts timer
     */
    public void Set(float seconds)
    {
        time = seconds;
        originalTime = seconds;
        bar.color = originalColor;
        timerCoroutine = StartCoroutine(RunTimer());
    }

    /*
     * Stops timer in place
     */
    public void Stop()
    {
        StopCoroutine(timerCoroutine);
    }

    IEnumerator RunTimer()
    {
        while (time > 0f)
        {
            float warningTimeEnd = warningTime - warningTimeTransition;
            while (time > 0f)
            {
                time = Mathf.Max(0f, time - Time.deltaTime);
                if (time < warningTime && time >= warningTimeEnd)
                {
                    float t = (time - warningTimeEnd) / (warningTime - warningTimeEnd);
                    bar.color = Vector4.Lerp(warningColor, originalColor, t);
                }
                bar.fillAmount = (time / originalTime);
                timerText.text = time.ToString("00.00");
                yield return new WaitForEndOfFrame();
            }
        }
        onComplete.Invoke();
    }
}
