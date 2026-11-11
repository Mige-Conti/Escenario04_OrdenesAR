using System.Collections.Generic;
using UnityEngine;

public class TypoDetector : MonoBehaviour
{
    // Lista de nombres reales/correctos para detectar la errata y beneficiarios no registrados
    private readonly List<string> validBeneficiaries = new List<string>
    {
        "SHALIKA FOUNDATION",
        "BANCO CENTRAL RESERVA",
        "PROVEEDOR ESTATAL AUTORIZADO"
    };

    public bool HasTypoError(string beneficiary, out string explanation)
    {
        if (beneficiary.ToUpper().Contains("FANDATION"))
        {
            explanation = "ALERTA DE VALIDACIÓN: Error de tipeo detectado en «SHALIKA FANDATION» (debería ser «FOUNDATION»). Un banco intermediario en Colombo frenó los US$20 M al ver una fundación diminuta con falta de ortografía.";
            return true;
        }

        explanation = "Sin error ortográfico evidente en el nombre.";
        return false;
    }

    public bool IsAuthorizedBeneficiary(string beneficiary)
    {
        return validBeneficiaries.Contains(beneficiary.ToUpper().Trim());
    }
}