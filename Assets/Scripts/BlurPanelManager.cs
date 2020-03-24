using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlurPanelManager : MonoBehaviour
{
    const string BLUR_PARAMETER = "_BumpAmt";

    [SerializeField] float blurTime = 1.5f;

    public int Blur {
        get {
            return image.materialForRendering.GetInt(BLUR_PARAMETER);
        }
        set {
            image.materialForRendering.SetInt(BLUR_PARAMETER, value);
        }
    }

    public Action onBlurInComplete;

    Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    public void BlurIn() {
        StartCoroutine(BlurTransition(512, 0, blurTime));
    }

    public void BlurOut()
    {
        StartCoroutine(BlurTransition(0, 512, blurTime));
    }

    IEnumerator BlurTransition(int start, int finish, float time) {

        Blur = start;
        float timePassed = 0f;
        while(!Mathf.Approximately(Blur, finish)) {
            image.raycastTarget = true;
            Blur = Mathf.RoundToInt(Mathf.Lerp(start, finish, (timePassed / time)));
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
            image.raycastTarget = false;
        }
        if(start > finish) {
            onBlurInComplete.Invoke();
        }
    }
}
