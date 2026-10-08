using UnityEngine;
using UnityEngine.UI;

public class CharacterController : MonoBehaviour
{
    // Game controller script
    [SerializeField] private GameController gameController;
    // Bilder på tommy
    [SerializeField] private Sprite neutralSprite;
    [SerializeField] private Sprite singingSprite;
    [SerializeField] private Sprite sadSprite;
    // delay innan bild ändras
    [SerializeField, Min(0f)] private float confirmationTime = 0.5f; // hur länge måste on ton hållas innan tommy byter
    [SerializeField, Min(0f)] private float neutralDelay = 0.5f;

    private enum Emotion { Neutral, Singing, Sad }

    private Image characterImage;
    private Emotion currentEmotion;
    private Emotion pendingEmotion;
    private double pendingSince;

    private bool inBreak;
    private double breakStarted;

    private void Awake()
    {
        characterImage = GetComponent<Image>();
        characterImage.preserveAspect = true;
        characterImage.raycastTarget = false;
    }

    private void OnEnable()
    {
        ResetToNeutral(); // starta neutralt
    }

    private void LateUpdate()
    {
        // återställ om kontrollern saknas, är inaktiv eller spelet inte körs
        if (gameController == null ||
            !gameController.isActiveAndEnabled ||
            !gameController.IsPlaying)
        {
            ResetToNeutral();
            return; // avsluta för denna frame
        }

        double now = Time.timeAsDouble; // räkna i tid, inte frames

        if (gameController.CurrentTarget == null) // ingen ton ska sjungas
        {
            if (!inBreak)
            {
                inBreak = true;
                breakStarted = now; // pausen börjar och sparas
            }

            // avbryt pågående bekräftelse, men behåll nuvarande uttryck
            pendingEmotion = currentEmotion;
            pendingSince = now;

            if (now - breakStarted >= neutralDelay) // pausen har varat tillräckligt länge
            {
                SetEmotion(Emotion.Neutral); // byt till neutralt uttryck
                pendingEmotion = Emotion.Neutral;
            }

            return; // kontrollera inte sången under pausen
        }

        // en ny ton har börjat efter pausen
        if (inBreak)
        {
            inBreak = false;
            pendingEmotion = currentEmotion;
            pendingSince = now;
        }

        // använd spelets befintliga kontroll för om spelaren sjunger rätt
        bool correct = gameController.HasPitch && gameController.OnPitch;
        Emotion wanted = correct ? Emotion.Singing : Emotion.Sad; // rätt => sjunger, annars ledsen

        if (wanted == currentEmotion)
        {
            pendingEmotion = currentEmotion;
            pendingSince = now;
            return;
        }

        if (wanted != pendingEmotion) // ett annat uttryck ska börja bekräftas
        {
            pendingEmotion = wanted; // spara uttrycket vi väntar på
            pendingSince = now;   // starta om tiden för bekräftelsen
        }

        if (now - pendingSince >= confirmationTime) // resultatet har hållit i sig tillräckligt länge
        {
            SetEmotion(wanted); // byt uttryck
        }
    }

    private void OnDisable()
    {
        ResetToNeutral();
    }

    private void ResetToNeutral() // nollställ & ta bort allt
    {
        inBreak = false;
        breakStarted = 0d;
        pendingEmotion = Emotion.Neutral;
        pendingSince = 0d;
        SetEmotion(Emotion.Neutral);
    }

    private void SetEmotion(Emotion emotion)
    {
        currentEmotion = emotion;

        if (characterImage == null)
            return;

        Sprite sprite = neutralSprite;

        if (emotion == Emotion.Singing)
            sprite = singingSprite;
        else if (emotion == Emotion.Sad)
            sprite = sadSprite;

        if (sprite == null)
            sprite = neutralSprite;

        if (characterImage.sprite != sprite)
            characterImage.sprite = sprite;
    }
}
