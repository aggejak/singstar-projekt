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
    // hur länge måste on ton hållas innan tommy byter
    [SerializeField, Min(0f)] private float singingDelay = 0f;
    [SerializeField, Min(0f)] private float sadDelay = 0.5f;
    [SerializeField, Min(0f)] private float neutralDelay = 0.5f;

    // färger till glow
    [SerializeField]
    private Color singingGlow =
        new Color(0.3f, 1f, 0.55f, 1f);
    [SerializeField]
    private Color sadGlow =
        new Color(1f, 0.4f, 0.4f, 1f);

    // glow
    [SerializeField] private Image glowImage;

    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float minGlowRadius = 8f;
    [SerializeField] private float maxGlowRadius = 40f;

    [SerializeField, Range(0f, 1f)]
    private float maxGlowStrength = 0.8f;

    private Material glowMaterial;

    //

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

        if (glowImage != null && glowImage.material != null)
        {
            // Create an independent material instance.
            glowMaterial = new Material(glowImage.material);
            glowImage.material = glowMaterial;
            glowImage.raycastTarget = false;
            glowImage.enabled = false;
        }
    }

    private void OnEnable()
    {
        ResetToNeutral(); // starta neutralt
    }

    private void OnDestroy()
    {
        if (glowMaterial != null)
            Destroy(glowMaterial);
    }

    private void Update()
    {
        UpdateGlow();
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

        // resultatet har hållit i sig tillräckligt länge
        float requiredDelay = wanted == Emotion.Singing
            ? singingDelay
            : sadDelay;

        if (now - pendingSince >= requiredDelay)
        {
            SetEmotion(wanted);
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

        if (glowImage != null)
        {
            // Keep the glow silhouette synchronized with Tommy.
            if (glowImage.sprite != sprite)
                glowImage.sprite = sprite;

            glowImage.enabled = emotion != Emotion.Neutral;
        }
    }


    private void UpdateGlow()
    {
        if (glowMaterial == null || glowImage == null)
            return;

        if (currentEmotion == Emotion.Neutral)
        {
            glowImage.enabled = false;
            return;
        }

        glowImage.enabled = true;

        // Progress from 0 to 1 repeatedly.
        float progress = Mathf.Repeat(
            Time.time * pulseSpeed, 1f
        );

        // Glow expands gradually.
        float radius = Mathf.Lerp(
            minGlowRadius,
            maxGlowRadius,
            progress
        );

        // Glow fades as it expands.
        float strength = maxGlowStrength *
            Mathf.Pow(1f - progress, 1.5f);

        Color color = currentEmotion == Emotion.Singing
            ? singingGlow
            : sadGlow;

        glowMaterial.SetColor("_GlowColor", color);
        glowMaterial.SetFloat("_Radius", radius);
        glowMaterial.SetFloat("_Strength", strength);
    }

}
