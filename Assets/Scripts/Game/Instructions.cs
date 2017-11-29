using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class Instructions : MonoBehaviour {

    public Image left;
    public Image right;

    CanvasGroup group;
    bool faded;

    void Awake() {
        group = GetComponent<CanvasGroup>();
        faded = false;

        group.blocksRaycasts = true;
        group.alpha = 1;
    }

    public void Fade(float time) {
        if (!faded) {
            StartCoroutine(FadeAlpha(time));

            faded = true;

            Image i = (CustomInput.GetHorizontalInput() == -1) ? left : right;
            Color c = i.color;
            i.color = new Color(c.r, c.g, c.b, c.a / 2);
        }
    }

    IEnumerator FadeAlpha(float time) {
        float start = Time.time;
        float startAlpha = group.alpha;

        group.blocksRaycasts = false;

        while(Time.time < start + time) {
            group.alpha = Mathf.Lerp(1, 0, Mathf.InverseLerp(start, start + time, Time.time));
            yield return null;
        }

        group.alpha = 0;
    }
}
