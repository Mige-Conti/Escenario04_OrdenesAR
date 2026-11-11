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

    void Start()
    {
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.RegisterTarget(this);
            currentOrder = InventoryUI.Instance.GetOrderById(orderId);
            RefreshCardDisplay();
        }
    }

    // Este método se conecta al evento OnTargetFound de Vuforia
    public void OnOrderDetected()
    {
        if (currentOrder == null && InventoryUI.Instance != null)
            currentOrder = InventoryUI.Instance.GetOrderById(orderId);

        if (currentOrder != null)
        {
            RefreshCardDisplay();
            InventoryUI.Instance.MarkOrderScanned(orderId);

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