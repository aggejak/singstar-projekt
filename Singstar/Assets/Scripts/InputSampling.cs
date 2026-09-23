using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InputSampling : MonoBehaviour // inherit from MonoBehaviour to be attachable to a GameObject
{
    [SerializeField] private float inputLevel;  // uppmätta ljudets amplitud

    private const int WindowSize = 2048;        // krävs för monoinspelning. stereo: 4096

    private AudioClip microphoneClip;           // ljud buffer som unity spelar in på
    [SerializeField] private string microphoneName; // namnet på mikrofonen som spelar in

    [SerializeField] private TMP_Text frequencyText; // text som visar upptäckt frekvens
    [SerializeField] private TMP_Text pitchText; // text som visar upptäckt tonläge
    [SerializeField] private TMP_Dropdown microphoneDropdown;

    private string[] availableMicrophones;
    
    private float[] samples;                    // array med frekvenser
    private int previousPosition;               // föregående position/tid att jämföra med
    private long capturedFrames;                // samling av allt som spelats in

    //

    private PitchDetection pitchDetection;
    private float[] monoSamples;
    public float CurrentPitchHz
    {
        get;
        private set;
    }

    private void OnEnable()
    {
        pitchDetection = new PitchDetection();

        // kolla att dropdownen är ansluten i Unity
        if (microphoneDropdown == null)
        {
            Debug.LogError("Assign the microphone dropdown in the Inspector.");
            return;
        }

        availableMicrophones = Microphone.devices; // hitta alla tillgängliga mikrofoner

        microphoneDropdown.ClearOptions(); // ta bort gamla alternativ

        var options = new List<string>
    {
        "Välj input..."
    };

        options.AddRange(availableMicrophones);

        microphoneDropdown.AddOptions(options);
        microphoneDropdown.SetValueWithoutNotify(0);
        microphoneDropdown.RefreshShownValue();

        microphoneDropdown.interactable =
            availableMicrophones.Length > 0;

        if (availableMicrophones.Length == 0)
            Debug.LogError("Ingen mikrofon hittad");
    }

    public void StartSelectedMicrophone()
    {
        // Kontrollera att dropdownen och mikrofonlistan finns
        if (microphoneDropdown == null || availableMicrophones == null)
            return;

        // Dropdown-alternativ 0 är instruktionen, därför börjar mic index på 1
        int deviceIndex = microphoneDropdown.value - 1;

        // Kontrollera att användaren har valt en mikrofon
        if (deviceIndex < 0 || deviceIndex >= availableMicrophones.Length)
        {
            Debug.LogWarning("Välj en mikrofon först");
            return;
        }

        // Om en tidigare inspelning fortfarande körs, stoppa den innan den nya inspelningen startas
        if (microphoneClip != null)
        {
            Microphone.End(microphoneName);
            Destroy(microphoneClip);
            microphoneClip = null;
        }

        inputLevel = 0f;
        previousPosition = 0;
        capturedFrames = 0;

        microphoneName = availableMicrophones[deviceIndex];

        microphoneClip =
            Microphone.Start(microphoneName, true, 2, 48000);

        if (microphoneClip == null)
        {
            Debug.LogError("Could not start microphone recording.");
            return;
        }

        samples = new float[WindowSize * microphoneClip.channels];
        monoSamples = new float[WindowSize];

        Debug.Log("Using microphone: " + microphoneName);
    }

    private void Update() // inspelning av ljud
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

        int channels = microphoneClip.channels; // hämta antalet ljudkanaler

        for (int frame = 0; frame < WindowSize; frame++)
        {
            float sum = 0f;

            for (int channel = 0; channel < channels; channel++)
            {
                int index = frame * channels + channel;
                sum += samples[index];
            }
            monoSamples[frame] = sum / channels;
        }

        CurrentPitchHz = pitchDetection.DetectPitch(monoSamples, microphoneClip.frequency);

        Debug.Log(
            "Upptäckt pitch: " +
            CurrentPitchHz.ToString("F1") +
            " Hz"
        );

        if (frequencyText != null)
        {
            if (CurrentPitchHz > 0)
            {
                frequencyText.text = $"{CurrentPitchHz:F0} Hz";
                pitchText.text = $"{FrequencyToPitch.ConvertedPitch(CurrentPitchHz):F0} cent";
            }
            else
            {
                frequencyText.text = "0 Hz";
                pitchText.text = "0 cent";
            }
        }

        // Beräkna RMS amplitud
        //float sumOfSquares = 0f;

        //for (int i = 0; i < samples.Length; i++)
        //{
        //    sumOfSquares += samples[i] * samples[i];
        //}

        //inputLevel = Mathf.Sqrt(sumOfSquares / samples.Length);
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
        CurrentPitchHz = 0f;
    }
}
