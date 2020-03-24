using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResultRow : MonoBehaviour
{
    [SerializeField] float drawTime = .3f;

    [SerializeField] Text songText;
    [SerializeField] Image correctResultImage;
    [SerializeField] Text speedText;

    [SerializeField] Sprite checkImage;
    [SerializeField] Sprite exImage;

    private void Awake()
    {
        correctResultImage.fillAmount = 0f;
    }

    public void SetSong(string song) {
        songText.text = song;
    }

    public void SetCorrect(bool correct, float delay)
    {
        correctResultImage.sprite = correct ? checkImage : exImage;
        StartCoroutine(DrawCorrectImage(delay));
    }

    public void SetSpeed(string speed)
    {
        speedText.text = speed;
    }

    IEnumerator DrawCorrectImage(float delay) {
        yield return new WaitForSeconds(delay);
        float timePassed = 0f;
        while (timePassed < drawTime) {
            correctResultImage.fillAmount = timePassed / drawTime;
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
}
