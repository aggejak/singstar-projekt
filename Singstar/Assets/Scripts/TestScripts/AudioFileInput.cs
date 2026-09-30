using UnityEngine;

public class AudioFileInput : MonoBehaviour
{
    [SerializeField] private AudioClip referenceClip;
    [SerializeField] private AudioSource songAudioSource;
    [SerializeField] private PitchAnalyzer pitchAnalyzer;

    private const int WindowSize = 2048;

    private float[] samples;
    private float[] monoSamples;
    private int previousPosition;
    private bool isRunning;

    public bool StartInput()
    {
        isRunning = false;

        if (referenceClip == null ||
            songAudioSource == null ||
            pitchAnalyzer == null)
        {
            Debug.LogError("Koppla referensfil, låtens AudioSource och PitchAnalyzer.");
            return false;
        }

        if (referenceClip.loadState != AudioDataLoadState.Loaded ||
            referenceClip.samples < WindowSize)
        {
            Debug.LogError("Referensfilens ljuddata är inte laddad eller filen är för kort.");
            return false;
        }

        samples = new float[WindowSize * referenceClip.channels];
        monoSamples = new float[WindowSize];

        // Kontrollera att filens samplingar går att läsa.
        if (!referenceClip.GetData(samples, 0))
        {
            Debug.LogError("Kan inte läsa referensfilen. Kontrollera importinställningarna.");
            return false;
        }

        previousPosition = 0;
        pitchAnalyzer.ResetAnalysis();
        isRunning = true;

        return true;
    }

    private void Update()
    {
        if (!isRunning || !songAudioSource.isPlaying)
            return;

        // Samma tid i referensfilen som i låten.
        int position = Mathf.FloorToInt(
            songAudioSource.time * referenceClip.frequency
        );

        if (position >= referenceClip.samples)
        {
            StopInput();
            return;
        }

        // Vänta på ett helt analysfönster och en ny position.
        if (position < WindowSize || position <= previousPosition)
            return;

        previousPosition = position;

        // Läs de senaste 2048 samplingarna per kanal.
        int startPosition = position - WindowSize;

        if (!referenceClip.GetData(samples, startPosition))
        {
            Debug.LogError("Kunde inte läsa samplingar från referensfilen.");
            StopInput();
            return;
        }

        int channels = referenceClip.channels;

        for (int frame = 0; frame < WindowSize; frame++)
        {
            float sum = 0f;

            for (int channel = 0; channel < channels; channel++)
            {
                sum += samples[frame * channels + channel];
            }

            monoSamples[frame] = sum / channels;
        }

        pitchAnalyzer.Analyze(monoSamples, referenceClip.frequency);
    }

    public void StopInput()
    {
        if (!isRunning)
            return;

        isRunning = false;
        pitchAnalyzer.ResetAnalysis();
    }

    private void OnDisable()
    {
        StopInput();
    }
}