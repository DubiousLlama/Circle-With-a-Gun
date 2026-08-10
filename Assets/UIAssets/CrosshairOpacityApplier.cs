using UnityEngine;

/// <summary>
/// Applies <see cref="CrosshairOpacity"/> to a SpriteRenderer and/or LineRenderer on this object.
/// </summary>
public class CrosshairOpacityApplier : MonoBehaviour
{
    [Tooltip("If set, opacity is applied as sqrt(slider value). Used by the sniper aim line.")]
    [SerializeField] private bool useSquareRootOpacity;

    [Tooltip("If set, this visual only appears for Commando, Specialist, and Miss Micro.")]
    [SerializeField] private bool sniperCharactersOnly;

    private SpriteRenderer spriteRenderer;
    private LineRenderer lineRenderer;
    private Color baseSpriteColor = Color.white;
    private GradientAlphaKey[] baseAlphaKeys;
    private bool allowedForCharacter = true;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lineRenderer = GetComponent<LineRenderer>();

        if (spriteRenderer != null)
            baseSpriteColor = spriteRenderer.color;

        if (lineRenderer != null)
            baseAlphaKeys = lineRenderer.colorGradient.alphaKeys;

        if (sniperCharactersOnly)
            allowedForCharacter = IsSniperCharacter();
    }

    private void OnEnable()
    {
        Apply(CrosshairOpacity.Get());
        CrosshairOpacity.Changed += Apply;
    }

    private void OnDisable()
    {
        CrosshairOpacity.Changed -= Apply;
    }

    private void Apply(float opacity)
    {
        opacity = Mathf.Clamp01(opacity);
        float effectiveOpacity = useSquareRootOpacity ? Mathf.Sqrt(opacity) : opacity;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = allowedForCharacter;
            if (allowedForCharacter)
            {
                Color c = baseSpriteColor;
                c.a = baseSpriteColor.a * effectiveOpacity;
                spriteRenderer.color = c;
            }
        }

        if (lineRenderer != null)
        {
            lineRenderer.enabled = allowedForCharacter;
            if (allowedForCharacter && baseAlphaKeys != null)
            {
                Gradient gradient = lineRenderer.colorGradient;
                GradientAlphaKey[] alphas = new GradientAlphaKey[baseAlphaKeys.Length];
                for (int i = 0; i < baseAlphaKeys.Length; i++)
                {
                    alphas[i] = baseAlphaKeys[i];
                    alphas[i].alpha = baseAlphaKeys[i].alpha * effectiveOpacity;
                }
                gradient.SetKeys(gradient.colorKeys, alphas);
                lineRenderer.colorGradient = gradient;
            }
        }
    }

    /// <summary>
    /// Roster indices: Commando=2, Specialist=5, Miss Microtransaction=8.
    /// </summary>
    private static bool IsSniperCharacter()
    {
        int selected = PlayerPrefs.GetInt("SelectedCharacter", 0);
        return selected == 2 || selected == 5 || selected == 8;
    }
}
