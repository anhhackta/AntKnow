using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelSliderManager : MonoBehaviour
{
    public RectTransform panelsContainer;
    public List<RectTransform> panels;
    public List<Button> buttons;
    public int startIndex = 0;
    [Range(0.1f, 1.5f)] public float duration = 0.35f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0,0,1,1);

    int current = -1;
    bool animating;

    IEnumerator Start()
    {
        if (!panelsContainer) yield break;

        if (panels == null || panels.Count == 0)
        {
            panels = new List<RectTransform>();
            for (int i = 0; i < panelsContainer.childCount; i++)
                panels.Add(panelsContainer.GetChild(i) as RectTransform);
        }

        for (int i = 0; i < buttons.Count; i++)
        {
            int idx = i;
            if (buttons[i]) buttons[i].onClick.AddListener(() => Show(idx));
        }

        foreach (var rt in panels) Stretch(rt);

        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelsContainer);

        for (int i = 0; i < panels.Count; i++)
        {
            panels[i].anchoredPosition = Vector2.zero;
            panels[i].gameObject.SetActive(i == startIndex);
        }
        current = Mathf.Clamp(startIndex, 0, panels.Count - 1);
    }

    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    float PixelPerfectWidth()
    {
        var canvas = panelsContainer.GetComponentInParent<Canvas>();
        float scale = canvas ? canvas.scaleFactor : 1f;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelsContainer);

        float w = panelsContainer.rect.width;
        if (w <= 0f) w = Screen.width;

        float px = Mathf.Round(w * scale);
        return px / scale;
    }

    Vector2 Snap(Vector2 v)
    {
        var canvas = panelsContainer.GetComponentInParent<Canvas>();
        float s = canvas ? canvas.scaleFactor : 1f;
        return new Vector2(Mathf.Round(v.x * s) / s, Mathf.Round(v.y * s) / s);
    }

    public void Show(int target)
    {
        if (animating || target == current || target < 0 || target >= panels.Count) return;
        int dir = target > current ? 1 : -1;
        StartCoroutine(SwitchCo(current, target, dir));
    }

    IEnumerator SwitchCo(int from, int to, int dir)
    {
        animating = true;

        float width = PixelPerfectWidth();

        var fromRt = panels[from];
        var toRt   = panels[to];

        toRt.gameObject.SetActive(true);

        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(panelsContainer);

        fromRt.anchoredPosition = Vector2.zero;
        toRt.anchoredPosition   = Snap(new Vector2(dir * width, 0f));

        float t = 0f;
        Vector2 fromStart = fromRt.anchoredPosition;
        Vector2 fromEnd   = Snap(new Vector2(-dir * width, 0f));
        Vector2 toStart   = toRt.anchoredPosition;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / duration;
            float k = ease.Evaluate(Mathf.Clamp01(t));
            fromRt.anchoredPosition = Snap(Vector2.Lerp(fromStart, fromEnd, k));
            toRt.anchoredPosition   = Snap(Vector2.Lerp(toStart, Vector2.zero, k));
            yield return null;
        }

        fromRt.anchoredPosition = Vector2.zero;
        fromRt.gameObject.SetActive(false);
        toRt.anchoredPosition   = Vector2.zero;

        current = to;
        animating = false;
    }

    void OnRectTransformDimensionsChange()
    {
        if (!isActiveAndEnabled || current < 0 || current >= panels.Count) return;
        Stretch(panels[current]);
        panels[current].anchoredPosition = Vector2.zero;
    }
}
