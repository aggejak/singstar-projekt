using UnityEngine;

public class GameTest : MonoBehaviour
{
    public InputSampling inputSampling;
    public AudioSource songAudioSource;

    private void Update()
    {
        float detectedHz = inputSampling.CurrentPitchHz;

        // Ingen ton hittad
        if (detectedHz <= 0f)
        {
            return;
        }

        float detectedMidi = FrequencyToPitch.ConvertedPitch(detectedHz);

        float songTime = songAudioSource.time;

        bool onPitch = SongNotes.CheckPitch(songTime, detectedMidi);

        Debug.Log(
            $"Tid: {songTime:F2} s | " +
            $"Hz: {detectedHz:F1} | " +
            $"MIDI: {detectedMidi:F2} | " +
            $"Rätt ton: {onPitch}"
        );
    }
}