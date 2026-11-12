using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClassificationHUD : MonoBehaviour
{
    private static ClassificationHUD _instance;
    public static ClassificationHUD Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ClassificationHUD>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ClassificationHUD");
                    _instance = go.AddComponent<ClassificationHUD>();
                }
            }
            return _instance;
        }
    }

    private GameObject hudPanel;
    private TextMeshProUGUI titleText;
    private TextMeshProUGUI alertFeedbackText;
    private OrderTarget currentTarget;
    private Coroutine alertCoroutine;

    // Paleta Cyberpunk
    private static readonly Color32 PanelBg = new Color32(0x02, 0x0A, 0x06, 235);     // #020A06
    private static readonly Color32 NeonGreen = new Color32(0x38, 0xF2, 0x8A, 255);   // #38F28A
    private static readonly Color32 NeonRed = new Color32(0xFF, 0x5A, 0x4E, 255);     // #FF5A4E
    private static readonly Color32 NeonAmber = new Color32(0xFF, 0xC2, 0x4A, 255);   // #FFC24A
    private static readonly Color32 BtnDarkBg = new Color32(0x08, 0x22, 0x15, 240);

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    void Start()
    {
        EnsureHUDCreated();
        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }
    }

    public void EnsureHUDCreated()
    {
        if (hudPanel != null)
            return;

        Canvas targetCanvas = FindTargetCanvas();
        if (targetCanvas == null)
            return;

        Transform existing = targetCanvas.transform.Find("Panel_Clasificar_Orden");
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        // Panel vertical en el centro-derecha de la pantalla
        hudPanel = new GameObject("Panel_Clasificar_Orden", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline), typeof(VerticalLayoutGroup));
        hudPanel.transform.SetParent(targetCanvas.transform, false);
        hudPanel.transform.SetAsLastSibling();

        RectTransform panelRect = hudPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 0.5f);
        panelRect.anchorMax = new Vector2(1f, 0.5f);
        panelRect.pivot = new Vector2(1f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-22f, -10f);
        panelRect.sizeDelta = new Vector2(280f, 330f);

        Image bgImage = hudPanel.GetComponent<Image>();
        bgImage.color = PanelBg;

        Outline panelOutline = hudPanel.GetComponent<Outline>();
        panelOutline.effectColor = NeonGreen;
        panelOutline.effectDistance = new Vector2(1.5f, -1.5f);

        VerticalLayoutGroup layout = hudPanel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 14, 14);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        // Título "CLASIFICAR ORDEN:"
        GameObject titleObj = new GameObject("Titulo_Clasificar", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
        titleObj.transform.SetParent(hudPanel.transform, false);

        LayoutElement titleLayout = titleObj.GetComponent<LayoutElement>();
        titleLayout.preferredHeight = 36f;

        titleText = titleObj.GetComponent<TextMeshProUGUI>();
        titleText.text = "CLASIFICAR ORDEN:";
        titleText.fontSize = 16f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = NeonGreen;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.raycastTarget = false;

        // 3 Botones Cyberpunk de clasificación
        CreateActionButton(hudPanel.transform, "Btn_Clasificar_Bloqueada", "[ BLOQUEADA ]", NeonGreen, () => OnClassificationSelected("Bloqueada"));
        CreateActionButton(hudPanel.transform, "Btn_Clasificar_Pagada", "[ PAGADA ]", NeonRed, () => OnClassificationSelected("Pagada"));
        CreateActionButton(hudPanel.transform, "Btn_Clasificar_Retenida", "[ RETENIDA ]", NeonAmber, () => OnClassificationSelected("Retenida"));

        // Texto de alerta temporal (feedback de clasificación)
        GameObject alertObj = new GameObject("Txt_Alerta_Temporal", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(LayoutElement));
        alertObj.transform.SetParent(hudPanel.transform, false);

        LayoutElement alertLayout = alertObj.GetComponent<LayoutElement>();
        alertLayout.preferredHeight = 58f;

        alertFeedbackText = alertObj.GetComponent<TextMeshProUGUI>();
        alertFeedbackText.text = "Selecciona el estado forense de la orden escaneada.";
        alertFeedbackText.fontSize = 13f;
        alertFeedbackText.color = new Color32(0xC8, 0xF7, 0xDC, 255);
        alertFeedbackText.alignment = TextAlignmentOptions.Center;
        alertFeedbackText.textWrappingMode = TextWrappingModes.Normal;
        alertFeedbackText.raycastTarget = false;

        // Botón [ CERRAR ]
        CreateActionButton(hudPanel.transform, "Btn_Cerrar_HUD", "[ CERRAR ]", new Color32(0x90, 0xB8, 0xA0, 255), CloseHUD, 34f);

        // Empieza oculto
        hudPanel.SetActive(false);
    }

    private void CreateActionButton(
        Transform parent,
        string objName,
        string label,
        Color32 accentColor,
        UnityEngine.Events.UnityAction onClick,
        float height = 40f)
    {
        GameObject btnObj = new GameObject(objName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(Outline), typeof(LayoutElement));
        btnObj.transform.SetParent(parent, false);

        LayoutElement le = btnObj.GetComponent<LayoutElement>();
        le.preferredHeight = height;

        Image img = btnObj.GetComponent<Image>();
        img.color = BtnDarkBg;

        Outline outline = btnObj.GetComponent<Outline>();
        outline.effectColor = accentColor;
        outline.effectDistance = new Vector2(1.2f, -1.2f);

        Button btn = btnObj.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        GameObject txtObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(btnObj.transform, false);

        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 14.5f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = accentColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
    }

    public void ShowHUD(OrderTarget target)
    {
        EnsureHUDCreated();
        currentTarget = target;

        if (hudPanel == null)
            return;

        hudPanel.SetActive(true);
        hudPanel.transform.SetAsLastSibling();

        if (titleText != null)
        {
            string idLabel = (target != null && !string.IsNullOrEmpty(target.orderId)) ? target.orderId : "";
            titleText.text = string.IsNullOrEmpty(idLabel)
                ? "CLASIFICAR ORDEN:"
                : $"CLASIFICAR ORDEN:\n<size=78%>{idLabel}</size>";
        }

        if (alertFeedbackText != null)
        {
            alertFeedbackText.text = "Evalúa la evidencia y clasifica esta orden SWIFT.";
            alertFeedbackText.color = new Color32(0xC8, 0xF7, 0xDC, 255);
        }
    }

    private void OnClassificationSelected(string choice)
    {
        AudioManager.Instance.PlaySFX("sfx_click");

        if (currentTarget == null)
            return;

        currentTarget.ClassifyOrder(choice);

        // Evaluar resultado con el motor de reglas para mostrar alerta temporal visual
        OrderData data = InventoryUI.Instance != null ? InventoryUI.Instance.GetOrderById(currentTarget.orderId) : null;
        if (data != null && InventoryUI.Instance != null && InventoryUI.Instance.ruleEngine != null)
        {
            string expected = InventoryUI.Instance.ruleEngine.EvaluateOrderState(data, out _);
            bool isCorrect = choice.Equals(expected, System.StringComparison.OrdinalIgnoreCase);

            string alertMessage = isCorrect
                ? $"✔ ¡CORRECTO! {data.id} está {expected.ToUpper()}."
                : $"✖ INCORRECTO: {data.id} está {expected.ToUpper()}, no {choice.ToUpper()}.";

            Color32 alertColor = isCorrect ? NeonGreen : NeonRed;
            ShowTemporaryAlert(alertMessage, alertColor);
        }
    }

    private void ShowTemporaryAlert(string message, Color32 color)
    {
        if (alertCoroutine != null)
        {
            StopCoroutine(alertCoroutine);
        }
        alertCoroutine = StartCoroutine(TemporaryAlertRoutine(message, color));
    }

    private IEnumerator TemporaryAlertRoutine(string message, Color32 color)
    {
        if (alertFeedbackText != null)
        {
            alertFeedbackText.text = message;
            alertFeedbackText.color = color;
        }

        yield return new WaitForSecondsRealtime(4.0f);

        if (alertFeedbackText != null)
        {
            alertFeedbackText.text = "Selecciona una opción o escanea otro marcador.";
            alertFeedbackText.color = new Color32(0xC8, 0xF7, 0xDC, 200);
        }
        alertCoroutine = null;
    }

    public void CloseHUD()
    {
        AudioManager.Instance.PlaySFX("sfx_click");
        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }
    }

    private Canvas FindTargetCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas c in canvases)
        {
            if (!c.isActiveAndEnabled) continue;
            if (c.renderMode == RenderMode.ScreenSpaceOverlay || c.renderMode == RenderMode.ScreenSpaceCamera)
                return c;
        }
        return canvases.Length > 0 ? canvases[0] : null;
    }
}

