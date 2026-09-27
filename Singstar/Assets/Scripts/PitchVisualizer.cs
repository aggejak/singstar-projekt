using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PitchVisualizer : MonoBehaviour
{
    [SerializeField] private GameController gameController;

    // Den gamla TargetLine används nu som mall för tonstaplarna.
    [SerializeField] private RectTransform targetLine;

    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private Image playerImage;

    // Tonhöjd
    [SerializeField] private float centerMidi = 60f;
    [SerializeField] private float pixelsPerSemitone = 20f;
    [SerializeField] private RectTransform playheadLine;

    // Tidslinje
    [SerializeField] private float pixelsPerSecond = 100f;

    // X-positionen där tonen ska sjungas NU.
    [SerializeField] private float playheadX = -250f;

    // Hur mycket av tidslinjen som visas.
    [SerializeField] private float visibleFutureSeconds = 10f;
    [SerializeField] private float visiblePastSeconds = 1f;

    private readonly List<RectTransform> noteBars =
        new List<RectTransform>();

    private void Start()
    {
        CreateNoteBars();

        // Originalet används bara som mall och ska inte själv visas.
        targetLine.gameObject.SetActive(false);
         Vector2 linePosition = playheadLine.anchoredPosition;
    linePosition.x = playheadX;
    playheadLine.anchoredPosition = linePosition;
    }

    private void CreateNoteBars()
    {
        foreach (TargetNote note in SongNotes.Notes)
        {
            RectTransform bar =
                Instantiate(targetLine, targetLine.parent);

            bar.name =
                $"Note_{note.startTime:F2}_{note.targetMidi:F0}";

            bar.gameObject.SetActive(false);

            noteBars.Add(bar);
        }
    }

    private void LateUpdate()
    {
        if (!gameController.IsPlaying)
        {
            HideEverything();
            return;
        }

        float songTime = gameController.SongTime;

        UpdateNoteBars(songTime);
        UpdatePlayerMarker();
    }

    private void UpdateNoteBars(float songTime)
    {
        for (int i = 0; i < SongNotes.Notes.Length; i++)
        {
            TargetNote note = SongNotes.Notes[i];
            RectTransform bar = noteBars[i];

            // Visa bara toner som ligger nära den aktuella tiden.
            bool visible =
                note.endTime >= songTime - visiblePastSeconds &&
                note.startTime <= songTime + visibleFutureSeconds;

            bar.gameObject.SetActive(visible);

            if (!visible)
            {
                continue;
            }

            // Tonens mittpunkt i tiden.
            float middleTime =
                (note.startTime + note.endTime) / 2f;

            // Flytta tonen åt vänster när tiden går.
            float x =
                playheadX +
                (middleTime - songTime) * pixelsPerSecond;

            // Tonhöjd bestämmer Y-position.
            float y =
                (note.targetMidi - centerMidi) *
                pixelsPerSemitone;

            bar.anchoredPosition =
                new Vector2(x, y);

            // Tonens längd bestämmer stapelns bredd.
            float duration =
                note.endTime - note.startTime;

            Vector2 size = bar.sizeDelta;

            size.x =
                Mathf.Max(4f, duration * pixelsPerSecond);

            bar.sizeDelta = size;
        }
    }

    private void UpdatePlayerMarker()
    {
        playerMarker.gameObject.SetActive(false);

        if (!gameController.HasPitch)
        {
            return;
        }

        float displayedMidi =
            gameController.CurrentMidi;

        TargetNote currentTarget =
            gameController.CurrentTarget;

        if (currentTarget != null)
        {
            // Placera spelarens ton i närmaste oktav
            // till måltonen.
            float difference =
                displayedMidi -
                currentTarget.targetMidi;

            while (difference > 6f)
            {
                difference -= 12f;
            }

            while (difference < -6f)
            {
                difference += 12f;
            }

            displayedMidi =
                currentTarget.targetMidi +
                difference;
        }

        playerMarker.gameObject.SetActive(true);

        float y =
            (displayedMidi - centerMidi) *
            pixelsPerSemitone;

        // Spelaren står kvar vid "nu"-positionen.
        playerMarker.anchoredPosition =
            new Vector2(playheadX, y);

        if (currentTarget == null)
        {
            playerImage.color = Color.yellow;
        }
        else
        {
            playerImage.color =
                gameController.OnPitch
                    ? Color.green
                    : Color.red;
        }
    }

    private void HideEverything()
    {
        foreach (RectTransform bar in noteBars)
        {
            bar.gameObject.SetActive(false);
        }

        playerMarker.gameObject.SetActive(false);
    }
}