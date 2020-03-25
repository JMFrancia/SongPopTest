using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/*
 * Used as control for BlurPanel object
 */
public class BlurPanelManager : MonoBehaviour
{
    const string BLUR_PARAMETER = "_BumpAmt";

    [Tooltip("Duration for blur animation")]
    [SerializeField] float blurTime = 1.5f;

    public int Blur
    {
        get
        {
            //Solves race condition, albeit a bit clumsily
            if (image == null)
            {
                SetImage();
            }
            return image.materialForRendering.GetInt(BLUR_PARAMETER);
        }
        set
        {
            //Solves race condition, albeit a bit clumsily
            if(image == null) 
            {
                SetImage();
            }
            image.materialForRendering.SetInt(BLUR_PARAMETER, value);
        }
    }

    public Action onBlurInComplete;

    Image image;

    private void Awake()
    {
        SetImage();
    }

    void SetImage() {
        image = GetComponent<Image>();
    }

    /*
     * Transition panel from fully-blurred to not-blurred
     */
    public void BlurIn()
    {
        StartCoroutine(BlurTransition(512, 0, blurTime));
    }

    /*
     * Transition panel from fully-blurred to not-blurred
     */
    public void BlurOut()
    {
        StartCoroutine(BlurTransition(0, 512, blurTime));
    }

    IEnumerator BlurTransition(int start, int finish, float time)
    {
        Blur = start;
        float timePassed = 0f;
        while (!Mathf.Approximately(Blur, finish))
        {
            image.raycastTarget = true;
            Blur = Mathf.RoundToInt(Mathf.Lerp(start, finish, (timePassed / time)));
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
            image.raycastTarget = false;
        }
        if (start > finish && onBlurInComplete != null)
        {
            onBlurInComplete.Invoke();
        }
    }
}
