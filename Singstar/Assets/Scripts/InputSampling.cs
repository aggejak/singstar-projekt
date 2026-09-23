using UnityEngine;

public class InputSampling : MonoBehaviour // inherit from MonoBehaviour to be attachable to a GameObject
{
    [SerializeField] private float inputLevel;  // uppmätta ljudets amplitud

    private const int WindowSize = 2048;        // krävs för monoinspelning. stereo: 4096

    private AudioClip microphoneClip;           // ljud buffer som unity spelar in på
    [SerializeField] private string microphoneName; // namnet på mikrofonen som spelar in
    private float[] samples;                    // array med frekvenser

    private int previousPosition;               // föregående position/tid att jämföra med
    private long capturedFrames;                // samling av allt som spelats in

    private void OnEnable() {
        // återställ alla värden för en ny inspelning
        inputLevel = 0f;
        previousPosition = 0;
        capturedFrames = 0;

        if (Microphone.devices.Length == 0) // ingen mikrofon tillgänglig
        {
            Debug.LogError("Ingen mikrofon hittad");
            return;
        }

        microphoneName = Microphone.devices[0]; // använd den första mikrofonen som är tillgänglig
        microphoneClip = Microphone.Start(microphoneName, true, 2, 48000); // spela in med en 2 sekunders buffert
        // true -> börja om med en ny 2 sekunders buffert när den föregående blir full
        // 48000 -> 48000 samples per sekund per kanal

        if (microphoneClip == null) // avbryt om inspelning inte startas
        {
            Debug.LogError("Kunde inte spela in");
            return;
        }

        samples = new float[WindowSize * microphoneClip.channels]; // array dit dit korta ljudinspelningar kopieras och sparas

        Debug.Log("Använder mikrofon: " + microphoneName);
    }

    private void Update()
    {
        if (microphoneClip == null)
        {
            return;
        }

        int position = Microphone.GetPosition(microphoneName); // återger positionen av inspelningen i bufferten

        if (position < 0)
        {
            return;
        }

        // sparar hur långt inspelningen har kommit
        int newFrames = (position - previousPosition + microphoneClip.samples) % microphoneClip.samples;

        previousPosition = position; // spara position för nästa check
        capturedFrames += newFrames;

        if (newFrames == 0 || capturedFrames < WindowSize) // skippa om inget nytt spelats in
        {
            return;
        }

        // ny startposition
        int startPosition = (position - WindowSize + microphoneClip.samples) % microphoneClip.samples;

        if (!microphoneClip.GetData(samples, startPosition)) // om datan inte kopierats in i samples, avbryt
        {
            return;
        }

        /// Beräkna RMS amplitud
        float sumOfSquares = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            sumOfSquares += samples[i] * samples[i];
        }

        inputLevel = Mathf.Sqrt(sumOfSquares / samples.Length);
    }

    private void OnDisable() // stäng av allt
    {
        if (microphoneClip != null)
        {
            Microphone.End(microphoneName);
            Destroy(microphoneClip);
            microphoneClip = null;
        }

        inputLevel = 0f;
    }
}
