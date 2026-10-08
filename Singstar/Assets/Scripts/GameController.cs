using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject startMenu;
    [SerializeField] private GameObject gameView;
    [SerializeField] private MicrophoneInput microphoneInput;
    [SerializeField] private AudioSource songAudioSource;
    [SerializeField] private PitchLogger pitchLogger;
    [SerializeField] private PitchAnalyzer pitchAnalyzer;

    [SerializeField, Range(0f, 0.9f)] private float pitchTolerance = 0.5f;
    public enum InputMode
    {
        Microphone,
        AudioFile
    }

    [SerializeField] private InputMode inputMode;
    [SerializeField] private AudioFileInput audioFileInput;

    [Header("Scoring")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private float maxPointsPerSecond = 100f;

    [Header("Slutskärm")]
    [SerializeField] private string creatorNames = "Axel, Nils, Ahmad";

    private GameObject resultView;
    private TMP_Text resultScoreText;
    private TMP_Text resultCreatorsText;
    private Button replayButton;

    public bool IsPlaying { get; private set; }
    public bool HasPitch { get; private set; }
    public float CurrentMidi { get; private set; }
    public TargetNote CurrentTarget { get; private set; }
    public bool OnPitch { get; private set; }
    public float SongTime => songAudioSource.time;

    public float Score { get; private set; }

    private void Start()
    {
        startMenu.SetActive(true);
        gameView.SetActive(false);
        UpdateScoreUI();
    }

    public void StartGame()
    {
        if (IsPlaying)
        {
            return;
        }

        // Stoppa eventuell tidigare källa innan en ny startas.
        audioFileInput.StopInput();
        microphoneInput.enabled = false;

        bool inputStarted;

        if (inputMode == InputMode.Microphone)
        {
            microphoneInput.enabled = true;
            inputStarted = microphoneInput.StartSelectedMicrophone();
        }
        else
        {
            inputStarted = audioFileInput.StartInput();
        }

        if (!inputStarted)
        {
            return;
        }

        startMenu.SetActive(false);
        gameView.SetActive(true);
        if (resultView != null)
        {
            resultView.SetActive(false);
        }

        songAudioSource.time = 0f;

        pitchLogger.BeginRecording(); // Spara ljudsamples

        songAudioSource.Play();

        Score = 0f;
        HasPitch = false;
        OnPitch = false;
        CurrentMidi = float.NaN;
        CurrentTarget = null;
        UpdateScoreUI();

        IsPlaying = true;
    }

    private void Update()
    {
        if (!IsPlaying)
        {
            return;
        }

        HasPitch = false;
        OnPitch = false;
        CurrentMidi = float.NaN;
        CurrentTarget = SongNotes.GetCurrentNote(SongTime);

        if (!songAudioSource.isPlaying)
        {
            audioFileInput.StopInput();
            microphoneInput.enabled = false;

            IsPlaying = false;
            ShowResults();
            pitchLogger.SaveRecording();
            return;
        }

        float detectedHz = pitchAnalyzer.CurrentPitchHz;

        if (detectedHz <= 0f || float.IsNaN(detectedHz) || float.IsInfinity(detectedHz))
        {
            return;
        }

        HasPitch = true;

        CurrentMidi = FrequencyToPitch.ConvertedPitch(detectedHz);




        //OnPitch = SongNotes.CheckPitch(CurrentTarget, CurrentMidi, pitchTolerance);


        if (CurrentTarget != null)
        {
            float difference = PitchScoring.GetPitchDifference(
                CurrentMidi,
                CurrentTarget.targetMidi
            );

            OnPitch = Mathf.Abs(difference) <= pitchTolerance;
        }

        UpdateScore();

        Debug.Log(
            $"Tid: {SongTime:F2} s | " +
            $"MIDI: {CurrentMidi:F2} | " +
            $"Rätt ton: {OnPitch} | " +
            $"Score: {Score:F0}"
        );
    }

    private void UpdateScore()
    {
        if (CurrentTarget == null || !HasPitch)
        {
            return;
        }

        float accuracy = PitchScoring.CalculateAccuracyFromMidi(
            CurrentMidi,
            CurrentTarget.targetMidi,
            pitchTolerance
        );

        float pointsThisFrame =
            accuracy * maxPointsPerSecond * Time.deltaTime;

        Score += pointsThisFrame;

        UpdateScoreUI();
    }

    private void ShowResults()
    {
        if (resultView == null)
        {
            // Skapas under samma Canvas utan nya Inspector-kopplingar.
            resultView = new GameObject(
                "ResultView", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            resultView.transform.SetParent(gameView.transform.parent, false);
            resultView.layer = gameView.transform.parent.gameObject.layer;

            RectTransform panel = resultView.GetComponent<RectTransform>();
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = Vector2.zero;
            resultView.GetComponent<Image>().color = new Color(0.04f, 0.04f, 0.08f, 1f);

            TMP_Text heading = CreateResultText("Heading", 0.62f, 0.75f, 52f);
            heading.text = "Din poäng";
            resultScoreText = CreateResultText("FinalScore", 0.43f, 0.62f, 100f);
            resultScoreText.color = new Color(0.3f, 1f, 0.55f, 1f);

            TMP_Text creditsHeading = CreateResultText("CreditsHeading", 0.29f, 0.38f, 30f);
            creditsHeading.text = "Skapad av";
            resultCreatorsText = CreateResultText("Creators", 0.15f, 0.29f, 42f);
            CreateReplayButton();
        }

        resultScoreText.text = Mathf.RoundToInt(Score).ToString();
        resultCreatorsText.text = creatorNames;
        gameView.SetActive(false);
        startMenu.SetActive(false);
        resultView.SetActive(true);
        resultView.transform.SetAsLastSibling();
        replayButton.Select();
    }

    private void CreateReplayButton()
    {
        GameObject buttonObject = new GameObject(
            "ReplayButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(resultView.transform, false);
        buttonObject.layer = resultView.layer;

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.35f, 0.04f);
        rect.anchorMax = new Vector2(0.65f, 0.13f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image background = buttonObject.GetComponent<Image>();
        background.color = new Color(0.3f, 1f, 0.55f, 1f);
        replayButton = buttonObject.GetComponent<Button>();
        replayButton.targetGraphic = background;
        replayButton.onClick.AddListener(StartGame);

        TMP_Text label = CreateResultText("ReplayLabel", 0f, 1f, 32f);
        label.transform.SetParent(buttonObject.transform, false);
        label.rectTransform.anchorMin = new Vector2(0.05f, 0.1f);
        label.rectTransform.anchorMax = new Vector2(0.95f, 0.9f);
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;
        label.color = new Color(0.04f, 0.04f, 0.08f, 1f);
        label.text = "Spela igen";
    }

    private TMP_Text CreateResultText(string objectName, float bottom, float top, float fontSize)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(resultView.transform, false);
        textObject.layer = resultView.layer;
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        if (scoreText != null && scoreText.font != null)
        {
            text.font = scoreText.font;
        }
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.fontSize = fontSize;
        text.enableAutoSizing = true;
        text.fontSizeMin = 12f;
        text.fontSizeMax = fontSize;
        text.raycastTarget = false;
        text.richText = false;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.1f, bottom);
        rect.anchorMax = new Vector2(0.9f, top);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return text;
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)  
        {
            scoreText.text = $"Score: {Mathf.RoundToInt(Score)}";
        }
    }
}
