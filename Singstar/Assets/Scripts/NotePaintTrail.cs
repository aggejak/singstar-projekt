using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotePaintTrail : MonoBehaviour
{
    private class Stroke
    {
        public RectTransform rect;
        public float start;
        public float end;
    }

    private readonly List<Stroke> strokes = new List<Stroke>();
    private int usedStrokes;

    private Color paintColor;
    private float glowSize;
    private float glowOpacity;

    public void Initialize(Color color, float size, float opacity)
    {
        paintColor = color;
        glowSize = size;
        glowOpacity = opacity;
    }

    public void Clear()
    {
        foreach (Stroke stroke in strokes)
        {
            stroke.rect.gameObject.SetActive(false);
        }

        usedStrokes = 0;
    }

    // from och to är positioner längs baren: 0 = vänster, 1 = höger.
    public void Paint(float from, float to)
    {
        from = Mathf.Clamp01(from);
        to = Mathf.Clamp01(to);

        if (to <= from)
            return;

        Stroke stroke = usedStrokes > 0
            ? strokes[usedStrokes - 1]
            : null;

        // Förläng senaste sträckan om träffen fortsätter utan glapp.
        bool continues = stroke != null &&
                         from >= stroke.start &&
                         from <= stroke.end + 0.00001f;

        if (continues)
        {
            stroke.end = Mathf.Max(stroke.end, to);
        }
        else
        {
            if (usedStrokes == strokes.Count)
            {
                strokes.Add(CreateStroke());
            }

            stroke = strokes[usedStrokes++];
            stroke.start = from;
            stroke.end = to;
            stroke.rect.gameObject.SetActive(true);
        }

        // Relativa ankare gör att spåret följer barens storlek.
        stroke.rect.anchorMin = new Vector2(stroke.start, 0f);
        stroke.rect.anchorMax = new Vector2(stroke.end, 1f);
        stroke.rect.offsetMin = Vector2.zero;
        stroke.rect.offsetMax = Vector2.zero;
    }

    private Stroke CreateStroke()
    {
        GameObject obj = new GameObject(
            "PaintStroke",
            typeof(RectTransform)
        );

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(transform, false);

        // Rita yttersta skenet först, därefter lager närmare baren.
        AddLayer(rect, "OuterGlow", glowSize,
            glowOpacity * 0.20f);

        AddLayer(rect, "MiddleGlow", glowSize * 0.65f,
            glowOpacity * 0.30f);

        AddLayer(rect, "InnerGlow", glowSize * 0.30f,
            glowOpacity * 0.50f);

        // Den tydliga färgen inne i själva notbaren.
        AddLayer(rect, "Paint", 0f, 1f);

        return new Stroke { rect = rect };
    }

    private void AddLayer(
        RectTransform parent,
        string layerName,
        float expansion,
        float opacity)
    {
        GameObject obj = new GameObject(
            layerName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.one * -expansion;
        rect.offsetMax = Vector2.one * expansion;

        Image image = obj.GetComponent<Image>();
        image.raycastTarget = false;
        image.color = new Color(
            paintColor.r,
            paintColor.g,
            paintColor.b,
            opacity
        );
    }
}