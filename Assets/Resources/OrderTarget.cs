using UnityEngine;
using TMPro;

public class OrderTarget : MonoBehaviour
{
    [Header("ID de la Orden (Ej: ORDEN_01 hasta ORDEN_08)")]
    public string orderId = "ORDEN_01";

    [Header("Textos 3D en el Prefab Ficha_Orden")]
    public TextMeshPro titleText;
    public TextMeshPro detailsText;
    public TextMeshPro statusText;
    public Renderer cardBackgroundRenderer;

    private OrderData currentOrder;
    private HologramDiorama3D hologramDiorama;

    void Start()
    {
        _ = AudioManager.Instance;
        AudioManager.Instance.EnsureHUDExists();
        EnsureHologramDiorama();

        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.RegisterTarget(this);
            currentOrder = InventoryUI.Instance.GetOrderById(orderId);
            RefreshCardDisplay();
        }
    }

    private HologramDiorama3D EnsureHologramDiorama()
    {
        if (hologramDiorama != null)
            return hologramDiorama;

        // Buscar el objeto Ficha_Orden dentro del marcador AR (o usar el padre de Base_Tarjeta)
        Transform targetHolder = transform.Find("Ficha_Orden");
        if (targetHolder == null && cardBackgroundRenderer != null && cardBackgroundRenderer.transform.parent != null)
        {
            targetHolder = cardBackgroundRenderer.transform.parent;
        }
        if (targetHolder == null)
        {
            targetHolder = transform;
        }

        hologramDiorama = targetHolder.GetComponent<HologramDiorama3D>();
        if (hologramDiorama == null)
        {
            hologramDiorama = targetHolder.gameObject.AddComponent<HologramDiorama3D>();
        }

        return hologramDiorama;
    }

    // Este método se conecta al evento OnTargetFound de Vuforia
    public void OnOrderDetected()
    {
        AudioManager.Instance.PlaySFX("sfx_scan");

        if (currentOrder == null && InventoryUI.Instance != null)
            currentOrder = InventoryUI.Instance.GetOrderById(orderId);

        if (currentOrder != null)
        {
            RefreshCardDisplay();
            InventoryUI.Instance.MarkOrderScanned(orderId);

            // Animación pop-in suave de 0 a 1 en 0.35 segundos al detectar la tarjeta
            HologramDiorama3D diorama = EnsureHologramDiorama();
            if (diorama != null)
            {
                diorama.PlayPopInAnimation();
            }

            bool isAttackMode = InventoryUI.Instance.ruleEngine == null || !InventoryUI.Instance.ruleEngine.isDefenseMode;
            bool isPaidOrFandation = currentOrder.realState == "Pagada" ||
                                     (!string.IsNullOrEmpty(currentOrder.beneficiary) && currentOrder.beneficiary.ToUpper().Contains("FANDATION"));

            if (isAttackMode && isPaidOrFandation)
            {
                AudioManager.Instance.PlaySFX("sfx_alerta");
            }

            string cleanOrderId = string.IsNullOrEmpty(orderId) ? currentOrder.id : orderId.Trim();
            string voiceClipName = "voz_" + cleanOrderId.ToLower();
            string forensicSubtitle = GetForensicSubtitleForOrder(cleanOrderId.ToUpper(), currentOrder);
            AudioManager.Instance.PlayVoice(voiceClipName, forensicSubtitle);

            if (ClassificationHUD.Instance != null)
            {
                ClassificationHUD.Instance.ShowHUD(this);
            }

            // Verificar si tiene el error de tipeo Fandation
            if (InventoryUI.Instance.typoDetector.HasTypoError(currentOrder.beneficiary, out string typoExplanation))
            {
                InventoryUI.Instance.ShowFeedback(typoExplanation);
            }
            else
            {
                InventoryUI.Instance.ShowFeedback($"Escaneada {currentOrder.id}: {currentOrder.beneficiary} (US$ {currentOrder.amountMillions} M). Clasifícala.");
            }
        }
    }

    private string GetForensicSubtitleForOrder(string id, OrderData order)
    {
        switch (id)
        {
            case "ORDEN_01":
                return "ORDEN 01 [PAGADA · US$ 20 M]: Emitida a las 21:05 hrs hacia RCBC Jupiter St. (Manila) a nombre de Michael Francisco Cruz. Sin doble aprobación ni límite nocturno, los fondos fueron desviados y lavados en casinos.";
            case "ORDEN_02":
                return "ORDEN 02 [PAGADA · US$ 30 M]: Emitida a las 21:42 hrs a favor de Jessie Christopher Lagrosas en RCBC Manila. Superó tres veces el límite seguro de US$ 10 millones sin generar alertas internas.";
            case "ORDEN_03":
                return "ORDEN 03 [PAGADA · US$ 16 M]: Emitida a las 22:15 hrs hacia cuenta ficticia de Alfred Santos Vergara en Manila. La ausencia de lista blanca permitió transferir fondos a una cuenta creada con identidad falsa.";
            case "ORDEN_04":
                return "ORDEN 04 [PAGADA · US$ 15 M]: Emitida a las 22:50 hrs a favor de Enrico Teodoro Vasquez en RCBC Jupiter St. Completa el bloque de 4 órdenes que drenaron US$ 81 millones irreversibles en Filipinas.";
            case "ORDEN_05":
                return "ORDEN_05 [RETENIDA · US$ 20 M]: Dirigida a «SHALIKA FANDATION» en Colombo (23:10 hrs). El error ortográfico en «Fandation» (por «Foundation») despertó sospechas en el banco corresponsal, frenando el desembolso.";
            case "ORDEN_06":
                return "ORDEN 06 [BLOQUEADA · US$ 0 M]: Emitida a las 20:36 hrs hacia Camphil Constructions sin especificar banco intermediario. Rechazada por error de formato SWIFT y nunca reenviada por los atacantes.";
            case "ORDEN_07":
                return "ORDEN 07 [BLOQUEADA · LOTE A · US$ 425 M]: Lote de 15 órdenes emitidas a la 01:15 hrs hacia cuentas en Jupiter St. Congeladas automáticamente por filtros OFAC debido a coincidencia con el petrolero sancionado «Jupiter».";
            case "ORDEN_08":
                return "ORDEN 08 [BLOQUEADA · LOTE B · US$ 425 M]: Segundo lote de 15 órdenes a las 02:40 hrs por US$ 425 millones. Retenido en el exterior gracias a la alerta de lista negra sobre la palabra clave «Jupiter».";
            default:
                return $"ANÁLISIS FORENSE {order.id}: Beneficiario {order.beneficiary} por US$ {order.amountMillions} millones ({order.timestamp} hrs). Estado en ataque: {order.realState}. {order.attackNote}";
        }
    }

    public void RefreshCardDisplay()
    {
        if (currentOrder == null || InventoryUI.Instance == null) return;

        string state = InventoryUI.Instance.ruleEngine.EvaluateOrderState(currentOrder, out string detail);

        if (titleText != null)
            titleText.text = $"{currentOrder.id} · {currentOrder.swiftCode}";

        if (detailsText != null)
            detailsText.text = $"Beneficiario: {currentOrder.beneficiary}\n" +
                               $"Destino: {currentOrder.destinationBank}\n" +
                               $"Monto: US$ {currentOrder.amountMillions} M ({currentOrder.orderCount} orden/es)\n" +
                               $"Hora SWIFT: {currentOrder.timestamp}";

        if (statusText != null)
            statusText.text = $"ESTADO: {state.ToUpper()}\n<size=70%>{detail}</size>";

        // Cambiar color del borde/fondo según el estado (Verde = Bloqueada/Defensa, Rojo = Pagada, Ámbar = Fandation)
        if (cardBackgroundRenderer != null)
        {
            if (state == "Pagada")
                cardBackgroundRenderer.material.color = new Color(0.8f, 0.2f, 0.2f); // Rojo
            else if (currentOrder.beneficiary.Contains("FANDATION") && !InventoryUI.Instance.ruleEngine.isDefenseMode)
                cardBackgroundRenderer.material.color = new Color(0.85f, 0.65f, 0.15f); // Ámbar
            else
                cardBackgroundRenderer.material.color = new Color(0.15f, 0.65f, 0.35f); // Verde
        }

        // Configurar y actualizar en vivo el Mini-Diorama Holográfico 3D sobre la tarjeta
        HologramDiorama3D diorama = EnsureHologramDiorama();
        if (diorama != null && InventoryUI.Instance.ruleEngine != null)
        {
            diorama.ConfigureAnimation(currentOrder.id, currentOrder.realState, InventoryUI.Instance.ruleEngine.isDefenseMode);
        }
    }

    // Llamado por los botones de clasificación del jugador (RF-02)
    public void ClassifyOrder(string playerChoice)
    {
        if (currentOrder == null) return;
        string expected = InventoryUI.Instance.ruleEngine.EvaluateOrderState(currentOrder, out _);

        if (playerChoice.Equals(expected, System.StringComparison.OrdinalIgnoreCase))
        {
            InventoryUI.Instance.ShowFeedback($"¡Correcto! {currentOrder.id} fue clasificada como {expected}.");
        }
        else
        {
            InventoryUI.Instance.ShowFeedback($"Incorrecto. En este modo, {currentOrder.id} está {expected}, no {playerChoice}.");
        }
    }
}