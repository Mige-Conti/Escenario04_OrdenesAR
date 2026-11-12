using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class VoiceAssistantHUD : MonoBehaviour
{
    public static VoiceAssistantHUD Instance { get; private set; }

    private const int BAR_COUNT = 8;
    private readonly RectTransform[] waveBars = new RectTransform[BAR_COUNT];
    private readonly Image[] waveBarImages = new Image[BAR_COUNT];

    private GameObject panelRoot;
    private TextMeshProUGUI headerStatusText;
    private TextMeshProUGUI subtitleTextUI;
    private TextMeshProUGUI muteButtonLabel;
    private Coroutine typewriterCoroutine;

    // Paleta Cyberpunk Banco Andino
    private static readonly Color32 BgDarkGreen = new Color32(0x02, 0x0A, 0x06, 230);      // #020A06 alpha 230
    private static readonly Color32 NeonGreen = new Color32(0x38, 0xF2, 0x8A, 255);        // #38F28A
    private static readonly Color32 NeonCyan = new Color32(0x4D, 0xF7, 0xE2, 255);         // #4DF7E2
    private static readonly Color32 ButtonBg = new Color32(0x09, 0x26, 0x18, 235);         // Botón oscuro
    private static readonly Color32 ButtonHover = new Color32(0x14, 0x4A, 0x2E, 255);
    private static readonly Color32 SubtitleColor = new Color32(0xE0, 0xFA, 0xEC, 255);

    [SerializeField] private float typewriterCharDelay = 0.018f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        EnsureUIInitialized();
    }

    void Update()
    {
        AnimateVoiceWaveform();
    }

    public void EnsureUIInitialized()
    {
        if (panelRoot != null)
            return;

        Canvas targetCanvas = FindTargetCanvas();
        if (targetCanvas == null)
            return;

        EnsureEventSystem();

        Transform existing = targetCanvas.transform.Find("Panel_Asistente_IA");
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        // 1. Panel superior principal (140px de alto, anclado arriba, #020A06 alpha 230)
        panelRoot = new GameObject("Panel_Asistente_IA", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelRoot.transform.SetParent(targetCanvas.transform, false);
        panelRoot.transform.SetAsLastSibling();

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(0f, 140f);

        Image panelImage = panelRoot.GetComponent<Image>();
        panelImage.color = BgDarkGreen;
        panelImage.raycastTarget = true;

        // Borde inferior verde neón (#38F28A)
        GameObject bottomBorder = new GameObject("Borde_Inferior_Neon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bottomBorder.transform.SetParent(panelRoot.transform, false);
        RectTransform borderRect = bottomBorder.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0f, 0f);
        borderRect.anchorMax = new Vector2(1f, 0f);
        borderRect.pivot = new Vector2(0.5f, 0f);
        borderRect.anchoredPosition = Vector2.zero;
        borderRect.sizeDelta = new Vector2(0f, 3f);

        Image borderImg = bottomBorder.GetComponent<Image>();
        borderImg.color = NeonGreen;
        borderImg.raycastTarget = false;

        // 2a. Indicador visual "I.A. FORENSE A.N.D.E.S. [EN LÍNEA]" + 8 barras de onda de voz
        CreateHeaderAndWaveform(panelRoot.transform);

        // 2b. Texto TextMeshProUGUI para subtítulos con efecto máquina de escribir
        CreateSubtitleArea(panelRoot.transform);

        // 2c. Tres botones interactivos en la esquina superior derecha
        CreateTopRightButtons(panelRoot.transform);
    }

    private Canvas FindTargetCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Canvas bestCanvas = null;

        foreach (Canvas c in canvases)
        {
            if (!c.isActiveAndEnabled) continue;
            if (c.renderMode == RenderMode.ScreenSpaceOverlay || c.renderMode == RenderMode.ScreenSpaceCamera)
            {
                bestCanvas = c;
                break;
            }
            if (bestCanvas == null)
                bestCanvas = c;
        }

        if (bestCanvas != null)
            return bestCanvas;

        // Si no hubiera Canvas en la escena, creamos uno automáticamente
        GameObject canvasObj = new GameObject("Canvas_HUD_Forense", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas newCanvas = canvasObj.GetComponent<Canvas>();
        newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        return newCanvas;
    }

    private void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }

    private void CreateHeaderAndWaveform(Transform parent)
    {
        // Contenedor de las 8 barras verticales (esquina superior izquierda)
        GameObject waveContainer = new GameObject("Indicador_Onda_Voz", typeof(RectTransform));
        waveContainer.transform.SetParent(parent, false);
        RectTransform waveRect = waveContainer.GetComponent<RectTransform>();
        waveRect.anchorMin = new Vector2(0f, 1f);
        waveRect.anchorMax = new Vector2(0f, 1f);
        waveRect.pivot = new Vector2(0f, 1f);
        waveRect.anchoredPosition = new Vector2(20f, -10f);
        waveRect.sizeDelta = new Vector2(76f, 32f);

        for (int i = 0; i < BAR_COUNT; i++)
        {
            GameObject barObj = new GameObject($"Barra_Onda_{i + 1:00}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            barObj.transform.SetParent(waveContainer.transform, false);

            RectTransform barRect = barObj.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 0.5f);
            barRect.anchorMax = new Vector2(0f, 0.5f);
            barRect.pivot = new Vector2(0.5f, 0.5f);
            barRect.anchoredPosition = new Vector2(6f + i * 9f, 0f);
            barRect.sizeDelta = new Vector2(5f, 6f);

            Image barImg = barObj.GetComponent<Image>();
            barImg.color = NeonGreen;
            barImg.raycastTarget = false;

            waveBars[i] = barRect;
            waveBarImages[i] = barImg;
        }

        // Etiqueta del encabezado: "I.A. FORENSE A.N.D.E.S. [EN LÍNEA]"
        GameObject headerObj = new GameObject("Header_IA_Andes", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(parent, false);
        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(0.55f, 1f);
        headerRect.pivot = new Vector2(0f, 1f);
        headerRect.anchoredPosition = new Vector2(104f, -10f);
        headerRect.sizeDelta = new Vector2(0f, 32f);

        headerStatusText = headerObj.GetComponent<TextMeshProUGUI>();
        headerStatusText.text = "I.A. FORENSE A.N.D.E.S. [EN LÍNEA]";
        headerStatusText.fontSize = 18f;
        headerStatusText.fontStyle = FontStyles.Bold;
        headerStatusText.color = NeonGreen;
        headerStatusText.alignment = TextAlignmentOptions.MidlineLeft;
        headerStatusText.textWrappingMode = TextWrappingModes.NoWrap;
        headerStatusText.raycastTarget = false;
    }

    private void CreateSubtitleArea(Transform parent)
    {
        GameObject subObj = new GameObject("Subtitulos_Asistente_TMP", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        subObj.transform.SetParent(parent, false);

        RectTransform subRect = subObj.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0f, 0f);
        subRect.anchorMax = new Vector2(1f, 1f);
        subRect.pivot = new Vector2(0.5f, 0.5f);
        subRect.offsetMin = new Vector2(22f, 10f);
        subRect.offsetMax = new Vector2(-22f, -46f);

        subtitleTextUI = subObj.GetComponent<TextMeshProUGUI>();
        subtitleTextUI.text = "Inicializando módulo de auditoría forense SWIFT...";
        subtitleTextUI.fontSize = 17.5f;
        subtitleTextUI.color = SubtitleColor;
        subtitleTextUI.alignment = TextAlignmentOptions.TopLeft;
        subtitleTextUI.textWrappingMode = TextWrappingModes.Normal;
        subtitleTextUI.overflowMode = TextOverflowModes.Ellipsis;
        subtitleTextUI.raycastTarget = false;
    }

    private void CreateTopRightButtons(Transform parent)
    {
        GameObject btnGroup = new GameObject("Controles_Voz_Derecha", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        btnGroup.transform.SetParent(parent, false);

        RectTransform groupRect = btnGroup.GetComponent<RectTransform>();
        groupRect.anchorMin = new Vector2(1f, 1f);
        groupRect.anchorMax = new Vector2(1f, 1f);
        groupRect.pivot = new Vector2(1f, 1f);
        groupRect.anchoredPosition = new Vector2(-16f, -8f);
        groupRect.sizeDelta = new Vector2(560f, 34f);

        HorizontalLayoutGroup layout = btnGroup.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleRight;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        CreateCyberButton(btnGroup.transform, "Btn_RepetirVoz", "[ REPETIR VOZ ]", OnRepeatVoiceClicked, out _);
        CreateCyberButton(btnGroup.transform, "Btn_AnalisisTecnico", "[ ANÁLISIS TÉCNICO ]", OnTechnicalAnalysisClicked, out _);
        CreateCyberButton(btnGroup.transform, "Btn_SilenciarVoz", "[ SILENCIAR VOZ ]", OnMuteVoiceClicked, out muteButtonLabel);
    }

    private void CreateCyberButton(
        Transform parent,
        string objectName,
        string labelText,
        UnityEngine.Events.UnityAction onClickAction,
        out TextMeshProUGUI labelComponent)
    {
        GameObject btnObj = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(Outline));
        btnObj.transform.SetParent(parent, false);

        Image btnImage = btnObj.GetComponent<Image>();
        btnImage.color = ButtonBg;

        Outline outline = btnObj.GetComponent<Outline>();
        outline.effectColor = NeonGreen;
        outline.effectDistance = new Vector2(1.2f, -1.2f);

        Button btn = btnObj.GetComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.85f, 1f, 0.9f, 1f);
        colors.pressedColor = new Color(0.6f, 0.95f, 0.75f, 1f);
        colors.selectedColor = Color.white;
        btn.colors = colors;
        btn.targetGraphic = btnImage;
        btn.onClick.AddListener(onClickAction);

        GameObject txtObj = new GameObject("Texto", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(btnObj.transform, false);

        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = new Vector2(4f, 2f);
        txtRect.offsetMax = new Vector2(-4f, -2f);

        labelComponent = txtObj.GetComponent<TextMeshProUGUI>();
        labelComponent.text = labelText;
        labelComponent.fontSize = 13f;
        labelComponent.fontStyle = FontStyles.Bold;
        labelComponent.color = NeonGreen;
        labelComponent.alignment = TextAlignmentOptions.Center;
        labelComponent.textWrappingMode = TextWrappingModes.NoWrap;
        labelComponent.raycastTarget = false;
    }

    private void OnRepeatVoiceClicked()
    {
        AudioManager.Instance.PlaySFX("sfx_click");
        AudioManager.Instance.ReplayLastVoice();
    }

    private void OnTechnicalAnalysisClicked()
    {
        AudioManager.Instance.PlaySFX("sfx_click");
        AudioManager.Instance.PlayVoice("voz_analisis_ataque", AudioManager.ATTACK_ANALYSIS_SUBTITLE);
    }

    private void OnMuteVoiceClicked()
    {
        AudioManager.Instance.PlaySFX("sfx_click");
        AudioManager.Instance.ToggleMuteVoice();
    }

    public void UpdateMuteButtonVisual(bool isMuted)
    {
        if (muteButtonLabel != null)
        {
            muteButtonLabel.text = isMuted ? "[ ACTIVAR VOZ ]" : "[ SILENCIAR VOZ ]";
            muteButtonLabel.color = isMuted ? new Color32(0xFF, 0x76, 0x76, 255) : NeonGreen;
        }

        if (headerStatusText != null)
        {
            headerStatusText.text = isMuted
                ? "I.A. FORENSE A.N.D.E.S. [VOZ SILENCIADA]"
                : "I.A. FORENSE A.N.D.E.S. [EN LÍNEA]";
        }
    }

    public void DisplaySubtitle(string subtitle)
    {
        EnsureUIInitialized();
        if (subtitleTextUI == null)
            return;

        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        typewriterCoroutine = StartCoroutine(TypewriterRoutine(subtitle));
    }

    private IEnumerator TypewriterRoutine(string fullText)
    {
        subtitleTextUI.text = fullText;
        subtitleTextUI.maxVisibleCharacters = 0;

        int totalChars = fullText.Length;
        for (int i = 0; i <= totalChars; i++)
        {
            subtitleTextUI.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(typewriterCharDelay);
        }

        subtitleTextUI.maxVisibleCharacters = 99999;
        typewriterCoroutine = null;
    }

    private void AnimateVoiceWaveform()
    {
        if (waveBars[0] == null)
            return;

        bool isSpeaking = AudioManager.Instance != null && AudioManager.Instance.IsVoicePlaying;
        float amplitude = isSpeaking ? AudioManager.Instance.CurrentVoiceAmplitude : 0f;

        for (int i = 0; i < BAR_COUNT; i++)
        {
            float targetHeight = 5f;
            if (isSpeaking)
            {
                float phase = (Time.unscaledTime * 14f) + (i * 0.85f);
                float barMod = Mathf.Abs(Mathf.Sin(phase)) * 0.65f + 0.35f;
                targetHeight = Mathf.Lerp(5f, 30f, Mathf.Clamp01(amplitude * barMod * 1.35f));
            }

            Vector2 size = waveBars[i].sizeDelta;
            size.y = Mathf.Lerp(size.y, targetHeight, Time.unscaledDeltaTime * 18f);
            waveBars[i].sizeDelta = size;

            if (waveBarImages[i] != null)
            {
                waveBarImages[i].color = isSpeaking ? Color.Lerp(NeonGreen, NeonCyan, (size.y - 5f) / 25f) : new Color32(0x1F, 0x6E, 0x43, 180);
            }
        }
    }
}

