using System.Collections.Generic;
using UnityEngine;
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

    void Awake()
    {
        Instance = this;
        LoadScenarioData();
    }

    void Start()
    {
        UpdateInventoryUI();
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
        ruleEngine.isDefenseMode = active;
        foreach (var target in activeTargets)
        {
            target.RefreshCardDisplay();
        }
        UpdateInventoryUI();
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