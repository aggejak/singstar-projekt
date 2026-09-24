using UnityEngine;
using UnityEngine.UI;

public class PitchVisualizer : MonoBehaviour
{
    [SerializeField] private GameController gameController;
    [SerializeField] private RectTransform targetLine;
    [SerializeField] private RectTransform playerMarker;
    [SerializeField] private Image playerImage;

    [SerializeField] private float centerMidi = 60f;
    [SerializeField] private float pixelsPerSemitone = 20f;

    private void LateUpdate()
    {
        // Dölj formerna tills vi vet vad som ska visas.
        targetLine.gameObject.SetActive(false);
        playerMarker.gameObject.SetActive(false);

        if (!gameController.IsPlaying)
        {
            return;
        }

        // Hitta måltonen som gäller vid låtens aktuella tid.
        TargetNote currentTarget = gameController.CurrentTarget;

        if (currentTarget != null)
        {
            targetLine.gameObject.SetActive(true);
            SetHeight(targetLine, currentTarget.targetMidi);
        }

        if (!gameController.HasPitch)
        {
            return;
        }

        float displayedMidi = gameController.CurrentMidi;

        if (currentTarget != null)
        {
            // Visa rösten i närmaste oktav till måltonen,
            // eftersom jämförelsen accepterar olika oktaver.
            float difference = displayedMidi - currentTarget.targetMidi;

            while (difference > 6f)
                difference -= 12f;

            while (difference < -6f)
                difference += 12f;

            displayedMidi = currentTarget.targetMidi + difference;
        }

        playerMarker.gameObject.SetActive(true);
        SetHeight(playerMarker, displayedMidi);

        if (currentTarget == null)
        {
            playerImage.color = Color.yellow;
        }
        else
        {
            playerImage.color = gameController.OnPitch
                ? Color.green
                : Color.red;
        }
    }

    private void SetHeight(RectTransform element, float midi)
    {
        Vector2 position = element.anchoredPosition;
        position.y = (midi - centerMidi) * pixelsPerSemitone;
        element.anchoredPosition = position;
    }
}
