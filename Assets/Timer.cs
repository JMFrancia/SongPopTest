using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] float warningTime = 3f;
    [SerializeField] float warningTimeTransition = 1f;
    [SerializeField] Image bar;
    [SerializeField] Text timerText;
    [SerializeField] Color warningColor;

    public float time { get; private set; }

    Color originalColor;
    float originalTime;
    Coroutine timerCoroutine;

    private void Awake()
    {
        originalColor = bar.color;
    }

    public void Set(float seconds) {
        time = seconds;
        originalTime = seconds;
        bar.color = originalColor;
        timerCoroutine = StartCoroutine(RunTimer());
    }

    public void Stop() {
        StopCoroutine(timerCoroutine);
    }

    IEnumerator RunTimer() {
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
    }
}
