using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("Referencias de Sistemas")]
    public RuleEngine ruleEngine;
    public TypoDetector typoDetector;

    [Header("Textos del Panel_Inventario (UI)")]
    public TextMeshProUGUI modeStatusText;
    public TextMeshProUGUI totalsText;
    public TextMeshProUGUI feedbackText;

    private Dictionary<string, OrderData> database = new Dictionary<string, OrderData>();
    private HashSet<string> scannedOrders = new HashSet<string>();
    private List<OrderTarget> activeTargets = new List<OrderTarget>();

    // Elementos de control del Tablero
    private GameObject floatingToggleButtonObj;
    private TextMeshProUGUI floatingToggleLabel;
    private Image inventoryPanelBgImage;
    private readonly List<GameObject> detailedPanelElements = new List<GameObject>();
    private bool isDashboardVisible = false;

    private static readonly Color32 CyberDarkBg = new Color32(0x02, 0x0A, 0x06, 238);     // #020A06
    private static readonly Color32 CyberNeonGreen = new Color32(0x38, 0xF2, 0x8A, 255);  // #38F28A
    private static readonly Color32 CyberNeonCyan = new Color32(0x4D, 0xF7, 0xE2, 255);   // #4DF7E2

    void Awake()
    {
        Instance = this;
        LoadScenarioData();
        _ = AudioManager.Instance;
        AudioManager.Instance.EnsureHUDExists();
        _ = CyberMatrixUI.Instance;
        _ = ClassificationHUD.Instance;
        _ = SwiftCodeInspectorUI.Instance;
    }

    void Start()
    {
        AudioManager.Instance.EnsureHUDExists();
        CyberMatrixUI.Instance.EnsureMatrixCreated();
        ClassificationHUD.Instance.EnsureHUDCreated();
        SwiftCodeInspectorUI.Instance.EnsureInspectorCreated();

        ArrangeBottomInventoryPanelLayout();
        CacheDetailedPanelElements();
        CreateFloatingDashboardToggleButton();

        // En Start(), el panel detallado empieza OCULTO por defecto para despejar la cámara AR
        SetDashboardVisibility(false);

        UpdateInventoryUI();
        CyberMatrixUI.Instance.UpdateMatrix(ruleEngine != null && ruleEngine.isDefenseMode);
        SwiftCodeInspectorUI.Instance.RefreshInspectorContent();
    }

    private void ArrangeBottomInventoryPanelLayout()
    {
        RectTransform panelRect = GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(0f, 240f);
        }

        Outline panelOutline = GetComponent<Outline>();
        if (panelOutline == null)
        {
            panelOutline = gameObject.AddComponent<Outline>();
        }
        panelOutline.effectColor = CyberNeonGreen;
        panelOutline.effectDistance = new Vector2(0f, 2f);

        // 1. modeStatusText: Fila superior izquierda (sin superponerse con los botones de modo a la derecha)
        if (modeStatusText != null)
        {
            RectTransform rt = modeStatusText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.offsetMin = new Vector2(26f, -46f);
            rt.offsetMax = new Vector2(-460f, -12f);

            modeStatusText.fontSize = 18.5f;
            modeStatusText.fontStyle = FontStyles.Bold;
            modeStatusText.color = CyberNeonGreen;
            modeStatusText.alignment = TextAlignmentOptions.MidlineLeft;
            modeStatusText.textWrappingMode = TextWrappingModes.NoWrap;
            modeStatusText.overflowMode = TextOverflowModes.Ellipsis;
        }

        // 2. totalsText: Mitad izquierda inferior con márgenes limpios
        if (totalsText != null)
        {
            RectTransform rt = totalsText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0.54f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.offsetMin = new Vector2(26f, 14f);
            rt.offsetMax = new Vector2(-16f, -52f);

            totalsText.fontSize = 16f;
            totalsText.enableAutoSizing = true;
            totalsText.fontSizeMin = 13f;
            totalsText.fontSizeMax = 17f;
            totalsText.color = new Color32(0xE0, 0xFA, 0xEC, 255);
            totalsText.alignment = TextAlignmentOptions.TopLeft;
            totalsText.textWrappingMode = TextWrappingModes.Normal;
        }

        // 3. feedbackText: Mitad derecha inferior (debajo de los botones Ataque 2016 y Con defensa)
        if (feedbackText != null)
        {
            RectTransform rt = feedbackText.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.55f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(16f, 14f);
            rt.offsetMax = new Vector2(-26f, -84f);

            feedbackText.fontSize = 15.5f;
            feedbackText.enableAutoSizing = true;
            feedbackText.fontSizeMin = 12.5f;
            feedbackText.fontSizeMax = 16.5f;
            feedbackText.color = CyberNeonCyan;
            feedbackText.alignment = TextAlignmentOptions.TopLeft;
            feedbackText.textWrappingMode = TextWrappingModes.Normal;
        }

        // 4. Alinear limpiamente los botones de Modo ("Ataque 2016" y "Con defensa") en la esquina superior derecha del panel
        Button[] modeButtons = GetComponentsInChildren<Button>(true);
        int btnIdx = 0;
        foreach (Button b in modeButtons)
        {
            if (b.gameObject == floatingToggleButtonObj) continue;

            RectTransform brt = b.GetComponent<RectTransform>();
            if (brt != null)
            {
                brt.anchorMin = new Vector2(1f, 1f);
                brt.anchorMax = new Vector2(1f, 1f);
                brt.pivot = new Vector2(1f, 1f);
                brt.sizeDelta = new Vector2(200f, 52f);
                float rightOffset = (btnIdx == 0) ? -236f : -24f;
                brt.anchoredPosition = new Vector2(rightOffset, -16f);
                btnIdx++;
            }
        }
    }

    private void CacheDetailedPanelElements()
    {
        detailedPanelElements.Clear();

        inventoryPanelBgImage = GetComponent<Image>();
        if (inventoryPanelBgImage != null)
        {
            inventoryPanelBgImage.color = CyberDarkBg;
        }

        if (modeStatusText != null && !detailedPanelElements.Contains(modeStatusText.gameObject))
            detailedPanelElements.Add(modeStatusText.gameObject);

        if (totalsText != null && !detailedPanelElements.Contains(totalsText.gameObject))
            detailedPanelElements.Add(totalsText.gameObject);

        if (feedbackText != null && !detailedPanelElements.Contains(feedbackText.gameObject))
            detailedPanelElements.Add(feedbackText.gameObject);

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (!detailedPanelElements.Contains(child))
            {
                detailedPanelElements.Add(child);
            }
        }
    }

    private void CreateFloatingDashboardToggleButton()
    {
        if (floatingToggleButtonObj != null)
            return;

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
            parentCanvas = FindFirstObjectByType<Canvas>();

        if (parentCanvas == null)
            return;

        Transform existing = parentCanvas.transform.Find("Btn_Tablero_Control_Flotante");
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        floatingToggleButtonObj = new GameObject(
            "Btn_Tablero_Control_Flotante",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button),
            typeof(Outline)
        );
        floatingToggleButtonObj.transform.SetParent(parentCanvas.transform, false);
        floatingToggleButtonObj.transform.SetAsLastSibling();

        // Anclado en Top-Right debajo del Panel_Asistente_IA
        RectTransform btnRect = floatingToggleButtonObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1f, 1f);
        btnRect.anchorMax = new Vector2(1f, 1f);
        btnRect.pivot = new Vector2(1f, 1f);
        btnRect.anchoredPosition = new Vector2(-20f, -175f);
        btnRect.sizeDelta = new Vector2(250f, 40f);

        Image btnImg = floatingToggleButtonObj.GetComponent<Image>();
        btnImg.color = CyberDarkBg;

        Outline outline = floatingToggleButtonObj.GetComponent<Outline>();
        outline.effectColor = CyberNeonGreen;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        Button btn = floatingToggleButtonObj.GetComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(ToggleDashboard);

        GameObject txtObj = new GameObject("Texto", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(floatingToggleButtonObj.transform, false);

        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = new Vector2(6f, 2f);
        txtRect.offsetMax = new Vector2(-6f, -2f);

        floatingToggleLabel = txtObj.GetComponent<TextMeshProUGUI>();
        floatingToggleLabel.text = "[ MOSTRAR TABLERO ▼ ]";
        floatingToggleLabel.fontSize = 14f;
        floatingToggleLabel.fontStyle = FontStyles.Bold;
        floatingToggleLabel.color = CyberNeonGreen;
        floatingToggleLabel.alignment = TextAlignmentOptions.Center;
        floatingToggleLabel.textWrappingMode = TextWrappingModes.NoWrap;
        floatingToggleLabel.raycastTarget = false;
    }

    public void ToggleDashboard()
    {
        AudioManager.Instance.PlaySFX("sfx_click");
        SetDashboardVisibility(!isDashboardVisible);
    }

    private void SetDashboardVisibility(bool visible)
    {
        isDashboardVisible = visible;

        if (inventoryPanelBgImage != null)
        {
            inventoryPanelBgImage.enabled = visible;
        }

        Outline panelOutline = GetComponent<Outline>();
        if (panelOutline != null)
        {
            panelOutline.enabled = visible;
        }

        foreach (GameObject elem in detailedPanelElements)
        {
            if (elem != null && elem != floatingToggleButtonObj)
            {
                elem.SetActive(visible);
            }
        }

        if (floatingToggleLabel != null)
        {
            floatingToggleLabel.text = visible ? "[ OCULTAR TABLERO ▲ ]" : "[ MOSTRAR TABLERO ▼ ]";
        }

        if (floatingToggleButtonObj != null)
        {
            floatingToggleButtonObj.transform.SetAsLastSibling();
        }
    }

    private void LoadScenarioData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("ScenarioData");
        if (jsonFile != null)
        {
            ScenarioDataList data = JsonUtility.FromJson<ScenarioDataList>(jsonFile.text);
            foreach (var order in data.orders)
            {
                database[order.id] = order;
            }
        }
        else
        {
            Debug.LogError("No se encontró ScenarioData.json en Assets/Resources/");
        }
    }

    public void RegisterTarget(OrderTarget target)
    {
        if (!activeTargets.Contains(target))
            activeTargets.Add(target);
    }

    public OrderData GetOrderById(string id)
    {
        return database.ContainsKey(id) ? database[id] : null;
    }

    public void MarkOrderScanned(string id)
    {
        scannedOrders.Add(id);
        UpdateInventoryUI();
    }

    // Llamado por los botones de Modo en la interfaz
    public void SetDefenseMode(bool active)
    {
        AudioManager.Instance.PlaySFX("sfx_click");

        if (active)
        {
            AudioManager.Instance.PlaySFX("sfx_escudo");
            AudioManager.Instance.PlayVoice("voz_defensa_activa", AudioManager.DEFENSE_ACTIVE_SUBTITLE);
        }
        else
        {
            AudioManager.Instance.PlayVoice("voz_analisis_ataque", AudioManager.ATTACK_ANALYSIS_SUBTITLE);
        }

        ruleEngine.isDefenseMode = active;
        foreach (var target in activeTargets)
        {
            target.RefreshCardDisplay();
        }
        UpdateInventoryUI();

        if (CyberMatrixUI.Instance != null)
        {
            CyberMatrixUI.Instance.UpdateMatrix(active);
        }

        if (SwiftCodeInspectorUI.Instance != null)
        {
            SwiftCodeInspectorUI.Instance.RefreshInspectorContent();
        }
    }

    public void ShowFeedback(string message)
    {
        if (feedbackText != null)
            feedbackText.text = message;
    }

    public void UpdateInventoryUI()
    {
        if (ruleEngine.isDefenseMode)
        {
            if (modeStatusText != null)
                modeStatusText.text = "MODO: CON DEFENSA (Doble aprobación, límites y lista blanca activos)";

            if (totalsText != null)
            {
                totalsText.text = $"Marcadores escaneados: {scannedOrders.Count} / {database.Count}\n" +
                                  "• Bloqueadas (Alerta externa): 0 órdenes (US$ 0 M)\n" +
                                  "• Pagadas a Manila: 0 órdenes (US$ 0 M)\n" +
                                  "• Retenidas por Defensa: 36 órdenes (US$ 951 M)\n" +
                                  "TOTAL RESGUARDADO: 36 ÓRDENES · US$ 951 M";
            }
        }
        else
        {
            if (modeStatusText != null)
                modeStatusText.text = "MODO: ATAQUE 2016 (Sin límites, sin doble aprobación)";

            int sumBlockedM = 0, countBlocked = 0;
            int sumPaidM = 0, countPaid = 0;
            int sumRetainedM = 0, countRetained = 0;

            // Suma sobre el caso real (y muestra progreso de escaneo)
            foreach (var kvp in database)
            {
                OrderData o = kvp.Value;
                if (o.realState == "Bloqueada") { sumBlockedM += o.amountMillions; countBlocked += o.orderCount; }
                else if (o.realState == "Pagada") { sumPaidM += o.amountMillions; countPaid += o.orderCount; }
                else if (o.realState == "Retenida") { sumRetainedM += o.amountMillions; countRetained += o.orderCount; }
            }

            if (totalsText != null)
            {
                totalsText.text = $"Marcadores escaneados: {scannedOrders.Count} / {database.Count}\n" +
                                  $"• Bloqueadas (Jupiter + 1 no reenviada): {countBlocked} órdenes (~US$ {sumBlockedM} M)\n" +
                                  $"• Pagadas a Manila: {countPaid} órdenes (US$ {sumPaidM} M)\n" +
                                  $"• Retenida («Fandation»): {countRetained} orden (US$ {sumRetainedM} M)\n" +
                                  "TOTAL CASO REAL: 36 ÓRDENES · US$ 951 M";
            }
        }
    }
}
