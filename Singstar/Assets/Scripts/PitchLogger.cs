using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class PitchLogger : MonoBehaviour
{
    [SerializeField] private AudioSource songAudioSource;

    private readonly List<string> rows = new List<string>();
    private bool isRecording;

    // Ger decimalpunkt oavsett datorns språkinställning.
    private static string Number(float value)
    {
        return value.ToString("G9", CultureInfo.InvariantCulture);
    }

    public void BeginRecording()
    {
        rows.Clear();

        rows.Add(
            "timeSeconds,frequencyHz,detectedMidi,targetMidi,hasPitch," +
            "smoothedFrequencyHz,smoothedMidi"
        );

        isRecording = true;
    }

    public void RecordMeasurement(float frequencyHz, float smoothedFrequencyHz)
    {
        if (!isRecording || !songAudioSource.isPlaying)
        {
            return;
        }

        float songTime = songAudioSource.time;

        bool hasPitch =
            frequencyHz > 0f &&
            !float.IsNaN(frequencyHz) &&
            !float.IsInfinity(frequencyHz);

        float detectedMidi = hasPitch
            ? FrequencyToPitch.ConvertedPitch(frequencyHz)
            : float.NaN;

        TargetNote target = SongNotes.GetCurrentNote(songTime);

        float targetMidi = target != null
            ? target.targetMidi
            : float.NaN;

        float savedFrequency = hasPitch ? frequencyHz : float.NaN;

        bool hasSmoothedPitch =
            smoothedFrequencyHz > 0f &&
            !float.IsNaN(smoothedFrequencyHz) &&
            !float.IsInfinity(smoothedFrequencyHz);

        float smoothedMidi = hasSmoothedPitch
            ? FrequencyToPitch.ConvertedPitch(smoothedFrequencyHz)
            : float.NaN;

        float savedSmoothedFrequency = hasSmoothedPitch
            ? smoothedFrequencyHz
            : float.NaN;

        rows.Add(
             $"{Number(songTime)}," +
             $"{Number(savedFrequency)}," +
             $"{Number(detectedMidi)}," +
             $"{Number(targetMidi)}," +
             $"{(hasPitch ? 1 : 0)}," +
             $"{Number(savedSmoothedFrequency)}," +
             $"{Number(smoothedMidi)}"
 );
    }

    public void SaveRecording()
    {
        if (!isRecording)
        {
            return;
        }

        isRecording = false;

        string filename =
            $"pitch_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.csv";

        string projectFolder = Directory.GetParent(Application.dataPath).FullName;
        string recordingsFolder = Path.Combine(projectFolder, "Recordings");

        string path = Path.Combine(recordingsFolder, filename);

        try
        {
            // Skapar mappen om den inte redan finns.
            Directory.CreateDirectory(recordingsFolder);

            File.WriteAllLines(path, rows);
            Debug.Log($"Pitchlogg sparad: {path}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"Kunde inte spara pitchloggen: {exception.Message}");
        }
    }

    private void OnDisable()
    {
        // Sparar även om du stoppar spelet i Unity i förtid.
        SaveRecording();
    }
}
