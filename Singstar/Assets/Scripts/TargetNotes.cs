using UnityEngine;

public class TargetNote
{
    public float startTime;
    public float endTime;
    public float targetMidi;

    public TargetNote(float startTime, float endTime, float targetMidi)
    {
        this.startTime = startTime;
        this.endTime = endTime;
        this.targetMidi = targetMidi;
    }
}

public static class SongNotes
{
    // MIDI-tonnummer
    private const float G3 = 55f;
    private const float A3 = 57f;
    private const float B3 = 59f;
    private const float C4 = 60f;
    private const float D4 = 62f;
    private const float E4 = 64f;
    private const float Fs4 = 66f;
    private const float G4 = 67f;

    // Tider i sekunder från ljudfilens början.
    // Ej uppmätta tider är fortfarande uppskattningar.
    public static readonly TargetNote[] Notes =
    {
        // Första refrängen
        new TargetNote(12.24f, 13.88f, B3),  // Stad
        new TargetNote(13.88f, 15.56f, C4),  // i
        new TargetNote(15.56f, 18.00f, D4),  // ljus
        new TargetNote(18.00f, 18.44f, E4),  // i
        new TargetNote(18.44f, 19.00f, Fs4), // ett
        new TargetNote(19.00f, 20.67f, G4),  // land
        new TargetNote(20.67f, 21.50f, C4),  // u-
        new TargetNote(21.50f, 22.33f, B3),  // -tan
        new TargetNote(22.33f, 24.83f, A3),  // namn

        // Paus 24.83–25.67
        new TargetNote(25.67f, 27.33f, B3),  // Ge
        new TargetNote(27.33f, 29.00f, Fs4), // mig
        new TargetNote(29.00f, 31.08f, G4),  // liv
        new TargetNote(31.08f, 31.50f, G4),  // där
        new TargetNote(31.50f, 31.92f, E4),  // all-
        new TargetNote(31.92f, 32.33f, C4),  // -ting
        new TargetNote(32.33f, 34.00f, B3),  // föds
        new TargetNote(34.00f, 35.67f, A3),  // på
        new TargetNote(35.67f, 39.00f, G3),  // nytt

        // Andra refrängen
        new TargetNote(39.00f, 40.68f, B3),  // Stad
        new TargetNote(40.68f, 42.36f, C4),  // i
        new TargetNote(42.36f, 44.88f, D4),  // ljus
        new TargetNote(44.88f, 45.30f, E4),  // i
        new TargetNote(45.30f, 45.72f, Fs4), // ett
        new TargetNote(45.72f, 47.40f, G4),  // land
        new TargetNote(47.40f, 48.24f, C4),  // u-
        new TargetNote(48.24f, 49.08f, B3),  // -tan
        new TargetNote(49.08f, 51.60f, A3),  // namn

        // Paus 51.60–52.44
        new TargetNote(52.44f, 54.12f, B3),  // Ge
        new TargetNote(54.12f, 55.80f, Fs4), // mig
        new TargetNote(55.80f, 57.90f, G4),  // liv
        new TargetNote(57.90f, 58.32f, G4),  // där
        new TargetNote(58.32f, 58.74f, E4),  // all-
        new TargetNote(58.74f, 59.16f, C4),  // -ting
        new TargetNote(59.16f, 60.84f, D4),  // föds
        new TargetNote(60.84f, 62.52f, Fs4), // på
        new TargetNote(62.52f, 67.28f, G4),  // nytt
    };

    public static bool CheckPitch(TargetNote target, float detectedMidi, float tolerance)
    {
        if (target == null ||
            float.IsNaN(detectedMidi) ||
            float.IsInfinity(detectedMidi))
        {
            return false;
        }

        float difference = detectedMidi - target.targetMidi;

        // Flytta skillnaden hela oktaver mot noll.
        while (difference > 6f)
            difference -= 12f;

        while (difference < -6f)
            difference += 12f;

        return Mathf.Abs(difference) <= tolerance;
    }
    public static TargetNote GetCurrentNote(float songTime)
    {
        foreach (TargetNote note in Notes)
        {
            if (songTime >= note.startTime && songTime < note.endTime)
            {
                return note;
            }
        }

        return null;
    }
}