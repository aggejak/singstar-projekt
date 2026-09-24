using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject startMenu;
    [SerializeField] private GameObject gameView;
    [SerializeField] private MicrophoneInput microphoneInput;
    [SerializeField] private AudioSource songAudioSource;
    [SerializeField, Range(0f, 1f)] private float pitchTolerance = 0.3f;

    public bool IsPlaying { get; private set; }
    public bool HasPitch { get; private set; }
    public float CurrentMidi { get; private set; }
    public TargetNote CurrentTarget { get; private set; }
    public bool OnPitch { get; private set; }
    public float SongTime => songAudioSource.time;
    // Starta menun
    private void Start()
    {
        startMenu.SetActive(true);
        gameView.SetActive(false);
    }

    // Starta spel
    public void StartGame()
    {
        bool microphoneStarted = microphoneInput.StartSelectedMicrophone();

        if (!microphoneStarted)
        {
            return;
        }

        //Byt till GameView
        startMenu.SetActive(false);
        gameView.SetActive(true);

        // Starta låt
        songAudioSource.time = 0f;
        songAudioSource.Play();

        IsPlaying = true;
    }

    private void Update()
    {
        if (!IsPlaying)
        {
            return;
        }

        // Nollställ så att ett gammalt "rätt" inte ligger kvar.
        HasPitch = false;
        OnPitch = false;
        CurrentMidi = float.NaN;
        CurrentTarget = SongNotes.GetCurrentNote(SongTime);

        if (!songAudioSource.isPlaying)
        {
            IsPlaying = false;
            return;
        }

        float detectedHz = microphoneInput.CurrentPitchHz;

        if (detectedHz <= 0f || float.IsNaN(detectedHz) || float.IsInfinity(detectedHz))
        {
            return;
        }

        HasPitch = true;
        CurrentMidi = FrequencyToPitch.ConvertedPitch(detectedHz);

        float songTime = songAudioSource.time;
        OnPitch = SongNotes.CheckPitch(CurrentTarget, CurrentMidi, pitchTolerance);

        Debug.Log(
            $"Tid: {songTime:F2} s | " +
            $"MIDI: {CurrentMidi:F2} | " +
            $"Rätt ton: {OnPitch}"
        );
    }
}
