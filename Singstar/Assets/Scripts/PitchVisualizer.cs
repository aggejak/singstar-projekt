using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PitchVisualizer : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private RectTransform targetLine;
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private Image playerImage;
    [SerializeField] private RectTransform playheadLine;

    [Header("Fast notgrupp")]
    [SerializeField, Min(0f)] private float horizontalPadding = 40f;
    [SerializeField, Min(0f)] private float verticalPadding = 36f;
    [SerializeField, Min(1f)] private float pixelsPerSemitone = 20f;
    [SerializeField, Min(1f)] private float barHeight = 12f;

    [Header("Pitchfeedback")]
    [SerializeField] private Color noteColor = new Color(0.72f, 0.79f, 0.9f, 1f);
    [SerializeField] private Color activeNoteColor = new Color(1f, 0.9f, 0.55f, 1f);
    [SerializeField] private Color hitColor = new Color(0.3f, 1f, 0.55f, 1f);
    [SerializeField] private Color offPitchColor = new Color(1f, 0.4f, 0.4f, 1f);

    private readonly List<Image> noteBars = new List<Image>();
    private readonly List<Outline> noteGlows = new List<Outline>();
    private RectTransform pitchArea;
    private TargetNoteGroup currentGroup;
    private Outline markerGlow;
    private Vector2 layoutSize;
    private float groupCenterMidi;
    private float pitchScale;
    private float timelineWidth;
    private float markerYLimit;
    private Vector3 markerBaseScale;

    private void Start()
    {
        pitchArea = targetLine.parent as RectTransform;
        markerBaseScale = playerMarker.localScale;
        SetCenteredAnchors(playerMarker);
        playerImage.raycastTarget = false;
        markerGlow = AddGlow(playerImage);

        // Den gamla linjen är endast en mall. Kvadraten visar nu tiden.
        targetLine.gameObject.SetActive(false);
        if (playheadLine != null)
        {
            playheadLine.gameObject.SetActive(false);
        }

        int maxNotes = 0;
        foreach (TargetNoteGroup group in SongNotes.Groups)
        {
            maxNotes = Mathf.Max(maxNotes, group.Notes.Length);
        }

        // Återanvänd samma staplar vid gruppbyten.
        for (int i = 0; i < maxNotes; i++)
        {
            RectTransform bar = Instantiate(targetLine, pitchArea);
            bar.name = $"GroupNote_{i + 1}";
            SetCenteredAnchors(bar);
            Image barImage = bar.GetComponent<Image>();
            barImage.raycastTarget = false;
            noteBars.Add(barImage);
            noteGlows.Add(AddGlow(barImage));
        }

        playerMarker.SetAsLastSibling();
        HideEverything();
    }

    private static void SetCenteredAnchors(RectTransform rect)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static Outline AddGlow(Image image)
    {
        Outline glow = image.GetComponent<Outline>();
        if (glow == null)
        {
            glow = image.gameObject.AddComponent<Outline>();
        }
        glow.effectDistance = new Vector2(3f, -3f);
        glow.enabled = false;
        return glow;
    }

    private void LateUpdate()
    {
        if (!gameController.IsPlaying)
        {
            HideEverything();
            return;
        }

        float songTime = gameController.SongTime;
        TargetNoteGroup group = SongNotes.GetDisplayGroup(songTime);
        if (group == null)
        {
            // Inga kvarvarande staplar under intro, pauser eller efter sista tonen.
            HideEverything();
            return;
        }

        if (group != currentGroup || layoutSize != pitchArea.rect.size)
        {
            currentGroup = group;
            LayoutGroup();
        }

        TargetNote target = SongNotes.GetCurrentNote(songTime);
        bool hasPitch = gameController.HasPitch &&
            !float.IsNaN(gameController.CurrentMidi) &&
            !float.IsInfinity(gameController.CurrentMidi);
        bool hit = hasPitch && target != null && gameController.OnPitch;
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 12f);

        UpdateNoteFeedback(songTime, target, hit, pulse);
        UpdatePlayerMarker(songTime, target, hasPitch, hit, pulse);
    }

    private void LayoutGroup()
    {
        layoutSize = pitchArea.rect.size;
        timelineWidth = Mathf.Max(1f, layoutSize.x - 2f * horizontalPadding);
        float usableHeight = Mathf.Max(1f, layoutSize.y - 2f * verticalPadding);
        markerYLimit = Mathf.Max(0f, (layoutSize.y - playerMarker.rect.height) * 0.5f - 8f);

        float minMidi = currentGroup.Notes[0].targetMidi;
        float maxMidi = minMidi;
        foreach (TargetNote note in currentGroup.Notes)
        {
            minMidi = Mathf.Min(minMidi, note.targetMidi);
            maxMidi = Mathf.Max(maxMidi, note.targetMidi);
        }

        // Centrera hela tonomfånget, med luft för spelarens pitch ovanför/under.
        // Skalan ändras bara vid gruppbyte eller när fönstret ändrar storlek.
        groupCenterMidi = (minMidi + maxMidi) * 0.5f;
        pitchScale = Mathf.Min(pixelsPerSemitone,
            Mathf.Max(1f, usableHeight - barHeight) / (maxMidi - minMidi + 4f));

        for (int i = 0; i < noteBars.Count; i++)
        {
            bool visible = i < currentGroup.Notes.Length;
            noteBars[i].gameObject.SetActive(visible);
            noteGlows[i].enabled = false;
            if (!visible)
            {
                continue;
            }

            TargetNote note = currentGroup.Notes[i];
            float startX = TimeToX(note.startTime);
            float endX = TimeToX(note.endTime);
            RectTransform bar = noteBars[i].rectTransform;
            bar.anchoredPosition = new Vector2((startX + endX) * 0.5f, MidiToY(note.targetMidi));
            // En liten springa skiljer även två intilliggande toner på samma höjd.
            float gap = Mathf.Min(3f, (endX - startX) * 0.15f);
            bar.sizeDelta = new Vector2(endX - startX - gap, barHeight);
            noteBars[i].color = noteColor;
        }
    }

    private float TimeToX(float songTime)
    {
        float progress = Mathf.InverseLerp(
            currentGroup.DisplayStartTime,
            currentGroup.DisplayEndTime,
            songTime
        );

        return (progress - 0.5f) * timelineWidth;
    }

    private float MidiToY(float midi)
    {
        return (midi - groupCenterMidi) * pitchScale;
    }

    private void UpdateNoteFeedback(float songTime, TargetNote target, bool hit, float pulse)
    {
        for (int i = 0; i < currentGroup.Notes.Length; i++)
        {
            TargetNote note = currentGroup.Notes[i];
            bool isCurrent = note == target;
            Color color = isCurrent ? (hit ? hitColor : activeNoteColor) : noteColor;
            if (songTime >= note.endTime)
            {
                color.a *= 0.45f;
            }
            noteBars[i].color = color;
            noteGlows[i].enabled = isCurrent && hit;
            noteGlows[i].effectColor = new Color(hitColor.r, hitColor.g, hitColor.b, 0.25f + pulse * 0.35f);
        }
    }

    private void UpdatePlayerMarker(
        float songTime,
        TargetNote target,
        bool hasPitch,
        bool hit,
        float pulse)
    {
        playerMarker.gameObject.SetActive(hasPitch);
        markerGlow.enabled = hit;

        // Behåll samma storlek så markören inte pulserar.
        playerMarker.localScale = markerBaseScale;

        if (!hasPitch)
        {
            return;
        }

        float referenceMidi = target != null
            ? target.targetMidi
            : groupCenterMidi;

        float difference = PitchScoring.GetPitchDifference(
            gameController.CurrentMidi,
            referenceMidi
        );

        float displayMidi;

        if (hit)
        {
            // Vid träff: exakt samma höjd som målbaren.
            displayMidi = target.targetMidi;
        }
        else
        {
            // Annars: hoppa mellan hela halvtonssteg.
            displayMidi = Mathf.Round(referenceMidi + difference);

            // Visa inte markören på måltonen när vi faktiskt missar.
            // Kan annars hända om toleransen är mindre än 0.5.
            if (target != null &&
                Mathf.Approximately(displayMidi, target.targetMidi))
            {
                displayMidi = target.targetMidi + Mathf.Sign(difference);
            }
        }

        float y = Mathf.Clamp(
            MidiToY(displayMidi),
            -markerYLimit,
            markerYLimit
        );

        playerMarker.anchoredPosition = new Vector2(
            TimeToX(songTime),
            y
        );

        playerImage.color = target == null
            ? activeNoteColor
            : (hit ? hitColor : offPitchColor);

        markerGlow.effectColor = new Color(
            hitColor.r,
            hitColor.g,
            hitColor.b,
            0.4f + pulse * 0.3f
        );
    }

    private void HideEverything()
    {
        currentGroup = null;
        foreach (Image bar in noteBars)
        {
            bar.gameObject.SetActive(false);
        }
        playerMarker.gameObject.SetActive(false);
        playerMarker.localScale = markerBaseScale;
        if (markerGlow != null)
        {
            markerGlow.enabled = false;
        }
    }

    private void OnDisable()
    {
        // GameView kan döljas mellan två spelomgångar.
        if (pitchArea != null)
        {
            HideEverything();
        }
    }
}
