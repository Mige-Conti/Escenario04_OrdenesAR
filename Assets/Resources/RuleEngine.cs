using System;
using UnityEngine;

[Serializable]
public class OrderData
{
    public string id;
    public string swiftCode;
    public string beneficiary;
    public string destinationBank;
    public int amountMillions;
    public int orderCount;
    public string timestamp;
    public bool hasSecondApproval;
    public string realState; // "Bloqueada", "Pagada", "Retenida"
    public string attackNote;
}

[Serializable]
public class ScenarioDataList
{
    public OrderData[] orders;
}

public class RuleEngine : MonoBehaviour
{
    public bool isDefenseMode = false;
    public int maxAllowedAmountMillions = 10; // Límite de US$ 10M por orden
    public TypoDetector typoDetector;

    // Evalúa el estado de una orden según el modo activo (Ataque 2016 vs Con Defensa)
    public string EvaluateOrderState(OrderData order, out string ruleDetail)
    {
        if (!isDefenseMode)
        {
            ruleDetail = order.attackNote;
            return order.realState;
        }

        // MODO DEFENSA: Aplicamos las 3 defensas requeridas
        string reasons = "";

        // 1. Doble aprobación (un operador crea, otro autoriza)
        if (!order.hasSecondApproval)
            reasons += "• Sin doble aprobación (operador/autorizador).\n";

        // 2. Límites por monto y horario (Ataque fue entre 20:36 y 03:59)
        if (order.amountMillions > maxAllowedAmountMillions)
            reasons += $"• Excede límite de monto (> US${maxAllowedAmountMillions} M).\n";

        if (IsNightSchedule(order.timestamp))
            reasons += $"• Fuera de horario bancario ({order.timestamp} hrs).\n";

        // 3. Lista de beneficiarios autorizados y validación de datos
        if (typoDetector != null && !typoDetector.IsAuthorizedBeneficiary(order.beneficiary))
            reasons += "• Beneficiario no registrado en lista blanca.";

        ruleDetail = "DEFENSA ACTIVA (RETENIDA):\n" + reasons;
        return "Retenida";
    }

    private bool IsNightSchedule(string timeStr)
    {
        if (string.IsNullOrEmpty(timeStr)) return true;
        string[] parts = timeStr.Split(':');
        if (parts.Length > 0 && int.TryParse(parts[0], out int hour))
        {
            // Fuera de horario laboral (antes de las 8:00 o después de las 18:00)
            return (hour >= 18 || hour < 8);
        }
        return true;
    }
}