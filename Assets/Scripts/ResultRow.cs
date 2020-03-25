using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*
 * Control class for results page row
 */
public class ResultRow : MonoBehaviour
{
    [Tooltip("Animation time for drawing Xs and checks in 'correct' column")]
    [SerializeField] float drawTime = .3f;

    [Header("Scene object references")]
    [SerializeField] Text songText;
    [SerializeField] Image correctResultImage;
    [SerializeField] Text speedText;

    [Header("Graphic references")]
    [SerializeField] Sprite checkImage;
    [SerializeField] Sprite exImage;

    private void Awake()
    {
        correctResultImage.fillAmount = 0f;
    }

    /*
     * Sets song text
     */
    public void SetSong(string song) {
        songText.text = song;
    }

    /*
     * Draws check or ex image
     */
    public void SetCorrect(bool correct, float delay)
    {
        correctResultImage.sprite = correct ? checkImage : exImage;
        StartCoroutine(DrawCorrectImage(delay));
    }

    /*
     * Sets speed text
     */
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
