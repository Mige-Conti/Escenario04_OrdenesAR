using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwiftCodeInspectorUI : MonoBehaviour
{
    private static SwiftCodeInspectorUI _instance;
    public static SwiftCodeInspectorUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SwiftCodeInspectorUI>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SwiftCodeInspectorUI");
                    _instance = go.AddComponent<SwiftCodeInspectorUI>();
                }
            }
            return _instance;
        }
    }

    private GameObject openButtonObj;
    private GameObject modalWindowObj;
    private TextMeshProUGUI headerOrderTitleText;
    private TextMeshProUGUI leftColumnSwiftText;
    private TextMeshProUGUI rightColumnMalwareText;

    private string currentInspectedOrderId = "ORDEN_05";
    private bool isModalOpen = false;

    // Paleta Terminal Forense Cyberpunk
    private static readonly Color32 ModalBgColor = new Color32(0x01, 0x08, 0x04, 248);    // #010804
    private static readonly Color32 ColumnBgColor = new Color32(0x04, 0x12, 0x0A, 240);   // Fondo interno columna
    private static readonly Color32 NeonGreen = new Color32(0x38, 0xF2, 0x8A, 255);       // #38F28A
    private static readonly Color32 NeonRed = new Color32(0xFF, 0x5A, 0x4E, 255);         // #FF5A4E
    private static readonly Color32 NeonCyan = new Color32(0x4D, 0xF7, 0xE2, 255);        // #4DF7E2
    private static readonly Color32 ButtonBg = new Color32(0x02, 0x0A, 0x06, 238);        // #020A06

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
        EnsureInspectorCreated();
        RefreshInspectorContent();
    }

    public void EnsureInspectorCreated()
    {
        if (openButtonObj != null && modalWindowObj != null)
            return;

        Canvas targetCanvas = FindTargetCanvas();
        if (targetCanvas == null)
            return;

        Transform existingBtn = targetCanvas.transform.Find("Btn_Ver_Trama_Swift");
        if (existingBtn != null) Destroy(existingBtn.gameObject);

        Transform existingModal = targetCanvas.transform.Find("Modal_Inspector_Swift_Malware");
        if (existingModal != null) Destroy(existingModal.gameObject);

        CreateFloatingTriggerButton(targetCanvas.transform);
        CreateModalWindow(targetCanvas.transform);

        modalWindowObj.SetActive(isModalOpen);
    }

    private void CreateFloatingTriggerButton(Transform canvasTransform)
    {
        openButtonObj = new GameObject(
            "Btn_Ver_Trama_Swift",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button),
            typeof(Outline)
        );
        openButtonObj.transform.SetParent(canvasTransform, false);
        openButtonObj.transform.SetAsLastSibling();

        RectTransform btnRect = openButtonObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0f, 1f);
        btnRect.anchorMax = new Vector2(0f, 1f);
        btnRect.pivot = new Vector2(0f, 1f);
        btnRect.anchoredPosition = new Vector2(20f, -175f);
        btnRect.sizeDelta = new Vector2(280f, 40f);

        Image btnImg = openButtonObj.GetComponent<Image>();
        btnImg.color = ButtonBg;

        Outline outline = openButtonObj.GetComponent<Outline>();
        outline.effectColor = NeonGreen;
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        Button btn = openButtonObj.GetComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(ToggleInspectorModal);

        GameObject txtObj = new GameObject("Texto", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(openButtonObj.transform, false);

        RectTransform txtRect = txtObj.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = new Vector2(6f, 2f);
        txtRect.offsetMax = new Vector2(-6f, -2f);

        TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
        tmp.text = "[ VER TRAMA SWIFT / MALWARE ]";
        tmp.fontSize = 14f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = NeonGreen;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
    }

    private void CreateModalWindow(Transform canvasTransform)
    {
        modalWindowObj = new GameObject(
            "Modal_Inspector_Swift_Malware",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline)
        );
        modalWindowObj.transform.SetParent(canvasTransform, false);
        modalWindowObj.transform.SetAsLastSibling();

        RectTransform modalRect = modalWindowObj.GetComponent<RectTransform>();
        modalRect.anchorMin = new Vector2(0.5f, 0.5f);
        modalRect.anchorMax = new Vector2(0.5f, 0.5f);
        modalRect.pivot = new Vector2(0.5f, 0.5f);
        modalRect.anchoredPosition = new Vector2(0f, -20f);
        modalRect.sizeDelta = new Vector2(950f, 540f);

        Image modalBg = modalWindowObj.GetComponent<Image>();
        modalBg.color = ModalBgColor;

        Outline modalOutline = modalWindowObj.GetComponent<Outline>();
        modalOutline.effectColor = NeonGreen;
        modalOutline.effectDistance = new Vector2(2f, -2f);

        // Barra superior del modal
        GameObject topBar = new GameObject("Barra_Titulo_Modal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        topBar.transform.SetParent(modalWindowObj.transform, false);
        RectTransform topRect = topBar.GetComponent<RectTransform>();
        topRect.anchorMin = new Vector2(0f, 1f);
        topRect.anchorMax = new Vector2(1f, 1f);
        topRect.pivot = new Vector2(0.5f, 1f);
        topRect.anchoredPosition = Vector2.zero;
        topRect.sizeDelta = new Vector2(0f, 48f);
        topBar.GetComponent<Image>().color = new Color32(0x06, 0x1E, 0x10, 255);

        // Título del modal
        GameObject titleObj = new GameObject("Titulo_Inspector", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(topBar.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = new Vector2(18f, 4f);
        titleRect.offsetMax = new Vector2(-150f, -4f);

        headerOrderTitleText = titleObj.GetComponent<TextMeshProUGUI>();
        headerOrderTitleText.text = "INSPECTOR FORENSE DE TRAMA SWIFT MT103 Y MALWARE HEX [ORDEN_05]";
        headerOrderTitleText.fontSize = 16.5f;
        headerOrderTitleText.fontStyle = FontStyles.Bold;
        headerOrderTitleText.color = NeonGreen;
        headerOrderTitleText.alignment = TextAlignmentOptions.MidlineLeft;
        headerOrderTitleText.raycastTarget = false;

        // Botón [ X CERRAR ]
        GameObject closeBtnObj = new GameObject(
            "Btn_Cerrar_Inspector",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button),
            typeof(Outline)
        );
        closeBtnObj.transform.SetParent(topBar.transform, false);
        RectTransform closeRect = closeBtnObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 0.5f);
        closeRect.anchorMax = new Vector2(1f, 0.5f);
        closeRect.pivot = new Vector2(1f, 0.5f);
        closeRect.anchoredPosition = new Vector2(-12f, 0f);
        closeRect.sizeDelta = new Vector2(125f, 34f);

        Image closeImg = closeBtnObj.GetComponent<Image>();
        closeImg.color = new Color32(0x28, 0x08, 0x08, 240);

        Outline closeOutline = closeBtnObj.GetComponent<Outline>();
        closeOutline.effectColor = NeonRed;
        closeOutline.effectDistance = new Vector2(1.2f, -1.2f);

        Button closeBtn = closeBtnObj.GetComponent<Button>();
        closeBtn.targetGraphic = closeImg;
        closeBtn.onClick.AddListener(CloseInspectorModal);

        GameObject closeTxtObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
        RectTransform closeTxtRect = closeTxtObj.GetComponent<RectTransform>();
        closeTxtRect.anchorMin = Vector2.zero;
        closeTxtRect.anchorMax = Vector2.one;
        closeTxtRect.offsetMin = Vector2.zero;
        closeTxtRect.offsetMax = Vector2.zero;

        TextMeshProUGUI closeTmp = closeTxtObj.GetComponent<TextMeshProUGUI>();
        closeTmp.text = "[ X CERRAR ]";
        closeTmp.fontSize = 13.5f;
        closeTmp.fontStyle = FontStyles.Bold;
        closeTmp.color = NeonRed;
        closeTmp.alignment = TextAlignmentOptions.Center;
        closeTmp.raycastTarget = false;

        // Contenedor de las 2 columnas
        CreateColumnPanel(
            modalWindowObj.transform,
            "Columna_Izquierda_MT103",
            new Vector2(0f, 0f),
            new Vector2(0.495f, 1f),
            new Vector2(16f, 16f),
            new Vector2(-8f, -58f),
            "COLUMNA 1 · TRAMA SWIFT MT103 EN CRUDO (FIN-COPY)",
            out leftColumnSwiftText
        );

        CreateColumnPanel(
            modalWindowObj.transform,
            "Columna_Derecha_MalwareHex",
            new Vector2(0.505f, 0f),
            new Vector2(1f, 1f),
            new Vector2(8f, 16f),
            new Vector2(-16f, -58f),
            "COLUMNA 2 · ANÁLISIS DE MEMORIA Y MALWARE LAZARUS (evtdiag.exe)",
            out rightColumnMalwareText
        );
    }

    private void CreateColumnPanel(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 offsetMin,
        Vector2 offsetMax,
        string columnHeader,
        out TextMeshProUGUI bodyTextComponent)
    {
        GameObject colObj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
        colObj.transform.SetParent(parent, false);

        RectTransform colRect = colObj.GetComponent<RectTransform>();
        colRect.anchorMin = anchorMin;
        colRect.anchorMax = anchorMax;
        colRect.offsetMin = offsetMin;
        colRect.offsetMax = offsetMax;

        Image colImg = colObj.GetComponent<Image>();
        colImg.color = ColumnBgColor;

        Outline colOutline = colObj.GetComponent<Outline>();
        colOutline.effectColor = new Color32(0x1E, 0x6E, 0x42, 220);
        colOutline.effectDistance = new Vector2(1f, -1f);

        // Subtítulo de la columna
        GameObject hdrObj = new GameObject("Header_Columna", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        hdrObj.transform.SetParent(colObj.transform, false);
        RectTransform hdrRect = hdrObj.GetComponent<RectTransform>();
        hdrRect.anchorMin = new Vector2(0f, 1f);
        hdrRect.anchorMax = new Vector2(1f, 1f);
        hdrRect.pivot = new Vector2(0.5f, 1f);
        hdrRect.anchoredPosition = new Vector2(0f, -6f);
        hdrRect.sizeDelta = new Vector2(-20f, 28f);

        TextMeshProUGUI hdrTmp = hdrObj.GetComponent<TextMeshProUGUI>();
        hdrTmp.text = columnHeader;
        hdrTmp.fontSize = 13.5f;
        hdrTmp.fontStyle = FontStyles.Bold;
        hdrTmp.color = NeonCyan;
        hdrTmp.alignment = TextAlignmentOptions.MidlineLeft;
        hdrTmp.raycastTarget = false;

        // Cuerpo de texto monoespaciado
        GameObject bodyObj = new GameObject("Body_Monospace_TMP", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        bodyObj.transform.SetParent(colObj.transform, false);
        RectTransform bodyRect = bodyObj.GetComponent<RectTransform>();
        bodyRect.anchorMin = Vector2.zero;
        bodyRect.anchorMax = Vector2.one;
        bodyRect.offsetMin = new Vector2(12f, 12f);
        bodyRect.offsetMax = new Vector2(-12f, -38f);

        bodyTextComponent = bodyObj.GetComponent<TextMeshProUGUI>();
        bodyTextComponent.fontSize = 13.8f;
        bodyTextComponent.enableAutoSizing = true;
        bodyTextComponent.fontSizeMin = 11.5f;
        bodyTextComponent.fontSizeMax = 14.5f;
        bodyTextComponent.color = new Color32(0xD8, 0xFA, 0xE6, 255);
        bodyTextComponent.alignment = TextAlignmentOptions.TopLeft;
        bodyTextComponent.textWrappingMode = TextWrappingModes.Normal;
        bodyTextComponent.richText = true;
        bodyTextComponent.raycastTarget = false;
    }

    public void ToggleInspectorModal()
    {
        EnsureInspectorCreated();
        AudioManager.Instance.PlaySFX("sfx_click");
        isModalOpen = !isModalOpen;
        modalWindowObj.SetActive(isModalOpen);

        if (isModalOpen)
        {
            modalWindowObj.transform.SetAsLastSibling();
            RefreshInspectorContent();
        }
    }

    public void CloseInspectorModal()
    {
        AudioManager.Instance.PlaySFX("sfx_click");
        isModalOpen = false;
        if (modalWindowObj != null)
        {
            modalWindowObj.SetActive(false);
        }
    }

    public void InspectOrder(string orderId)
    {
        if (!string.IsNullOrEmpty(orderId))
        {
            currentInspectedOrderId = orderId.Trim().ToUpper();
        }

        EnsureInspectorCreated();
        RefreshInspectorContent();
    }

    public void RefreshInspectorContent()
    {
        EnsureInspectorCreated();
        if (leftColumnSwiftText == null || rightColumnMalwareText == null)
            return;

        OrderData order = null;
        if (InventoryUI.Instance != null)
        {
            order = InventoryUI.Instance.GetOrderById(currentInspectedOrderId);
            if (order == null)
            {
                order = InventoryUI.Instance.GetOrderById("ORDEN_05");
            }
        }

        if (order == null)
        {
            order = new OrderData
            {
                id = "ORDEN_05",
                swiftCode = "BBHOBDDH/FEB04/22",
                beneficiary = "SHALIKA FANDATION",
                destinationBank = "PAN ASIA BANKING CORP COLOMBO (Sri Lanka)",
                amountMillions = 20,
                orderCount = 1,
                timestamp = "23:10",
                hasSecondApproval = false,
                realState = "Retenida",
                attackNote = "Frenada en Colombo por error de tipeo: «Fandation» en vez de «Foundation»."
            };
        }

        bool isDefenseMode = InventoryUI.Instance != null &&
                             InventoryUI.Instance.ruleEngine != null &&
                             InventoryUI.Instance.ruleEngine.isDefenseMode;

        if (headerOrderTitleText != null)
        {
            string modeLabel = isDefenseMode ? "MODO DEFENSA ZERO TRUST" : "MODO ATAQUE 2016";
            headerOrderTitleText.text = $"INSPECTOR FORENSE SWIFT MT103 Y MALWARE HEX · [{order.id} · {modeLabel}]";
        }

        leftColumnSwiftText.text = BuildLeftColumnMT103(order, isDefenseMode);
        rightColumnMalwareText.text = BuildRightColumnMalware(order, isDefenseMode);
    }

    private string BuildLeftColumnMT103(OrderData order, bool isDefenseMode)
    {
        string amountFormatted = $"{order.amountMillions:00},000,000.00";
        string line57A = $":57A: {order.destinationBank}";
        string line59 = $":59:  {order.beneficiary}";
        string line72;

        if (!isDefenseMode)
        {
            // Resaltar con <color=#FF5A4E> las líneas vulnerables en Modo Ataque
            if (order.beneficiary.ToUpper().Contains("FANDATION"))
            {
                line59 = $"<color=#FF5A4E>:59:  {order.beneficiary} [ERRATA DETECTADA]</color>";
            }
            else if (order.id == "ORDEN_01" || order.id == "ORDEN_02" || order.id == "ORDEN_03" || order.id == "ORDEN_04")
            {
                line59 = $"<color=#FF5A4E>:59:  {order.beneficiary} [CUENTA FICTICIA MANILA]</color>";
            }

            if (order.id == "ORDEN_07" || order.id == "ORDEN_08" || order.destinationBank.ToUpper().Contains("JUPITER"))
            {
                line57A = $"<color=#FF5A4E>:57A: {order.destinationBank} [LISTA NEGRA OFAC]</color>";
            }
            else if (order.id == "ORDEN_06" || order.destinationBank.ToUpper().Contains("SIN BANCO"))
            {
                line57A = $"<color=#FF5A4E>:57A: {order.destinationBank} [CAMPO OBLIGATORIO AUSENTE]</color>";
            }

            line72 = "<color=#FF5A4E>[SIN FIRMA :72: DOBLE APROBACIÓN]</color>";
        }
        else
        {
            // En Modo Defensa todo se valida y retiene con firma doble
            line57A = $"<color=#38F28A>:57A: {order.destinationBank} [AUDITADO]</color>";
            line59 = $"<color=#38F28A>:59:  {order.beneficiary} [FILTRO LISTA BLANCA]</color>";
            line72 = "<color=#38F28A>:72: /AUTH/ VERIFICADO POR 2 SUPERVISORES - RETENIDA</color>";
        }

        return "<mspace=0.56em>" +
               "{1:F01BBHOBDDHAXXX0000000000}{2:I103FRNYUS33XXXXN}{4:\n" +
               $":20:  {order.swiftCode}\n" +
               ":23B: CRED\n" +
               $":32A: 160204USD{amountFormatted}\n" +
               ":50K: /021083459 BANGLADESH BANK DHAKA\n" +
               $"{line57A}\n" +
               $"{line59}\n" +
               ":70:  PAYMENT CONSULTANCY INFRASTRUCTURE\n" +
               ":71A: SHA\n" +
               $"{line72}\n" +
               "-}{5:{CHK:9F4A2C81B0E3}}</mspace>\n\n" +
               $"<b>NOTA FORENSE ({order.id}):</b>\n{order.attackNote}";
    }

    private string BuildRightColumnMalware(OrderData order, bool isDefenseMode)
    {
        if (!isDefenseMode)
        {
            return "<mspace=0.56em>" +
                   "<b>[BINARIO COMPROMETIDO: evtdiag.exe (SWIFT Alliance Access)]</b>\n\n" +
                   "<color=#FF5A4E>OFFSET 0x0041A8F2: [75 08] -> [90 90]\n" +
                   "(2 BYTES MODIFICADOS - BYPASS DE VALIDACIÓN SQL)</color>\n\n" +
                   "<color=#FF5A4E>OFFSET 0x0042C110: [E8 4B 12] -> [B8 00 00]\n" +
                   "(DESACTIVACIÓN DE CHEQUEO DE SALDO Y FIRMA)</color>\n\n" +
                   "<color=#FF5A4E>PRINTER_SPOOLER_SERVICE: [TERMINATED]</color>\n" +
                   "LOG_FILE: gpget.dat [BORRADO POR ROOTKIT LAZARUS]\n" +
                   "</mspace>\n" +
                   "<b>VULNERABILIDADES EXPLOTADAS:</b>\n" +
                   $"• Emisión nocturna sin supervisión ({order.timestamp} hrs).\n" +
                   $"• Monto de orden: US$ {order.amountMillions} M ({order.orderCount} orden/es) sin tope.\n" +
                   $"• Estado resultante en 2016: <color=#FF5A4E><b>{order.realState.ToUpper()}</b></color>.";
        }
        else
        {
            return "<mspace=0.56em>" +
                   "<b>[AUDITORÍA DE MEMORIA: evtdiag.exe PROTEGIDO]</b>\n\n" +
                   "<color=#38F28A>INTEGRIDAD DE ARCHIVOS: [HASH SHA-256 VERIFICADO]</color>\n" +
                   "OFFSET 0x0041A8F2: [75 08] (INSTRUCCIÓN JNZ INTACTA)\n\n" +
                   "<color=#38F28A>PRINTER_SPOOLER_SERVICE: [ONLINE & LOGGING]</color>\n\n" +
                   $"<color=#38F28A>REGLA HORARIO (08:00-18:00): [BLOQUEO ACTIVO (Hora orden: {order.timestamp})]</color>\n\n" +
                   "<color=#38F28A>REGLA MONTO MAX (US$ 10M): [RETENIDA]</color>\n" +
                   "<color=#38F28A>LISTA BLANCA BENEFICIARIOS: [BLOQUEO PREVENTIVO]</color>\n" +
                   "</mspace>\n" +
                   $"<b>RESULTADO ZERO TRUST ({order.id}):</b>\n" +
                   $"Orden por US$ {order.amountMillions} M interceptada antes de salir a la red SWIFT.";
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
