using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CyberMatrixUI : MonoBehaviour
{
    private static CyberMatrixUI _instance;
    public static CyberMatrixUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CyberMatrixUI>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("CyberMatrixUI");
                    _instance = go.AddComponent<CyberMatrixUI>();
                }
            }
            return _instance;
        }
    }

    private const int TOTAL_ORDERS = 36;
    private readonly List<Image> matrixCells = new List<Image>(TOTAL_ORDERS);
    private GameObject matrixPanel;
    private TextMeshProUGUI matrixTitleText;

    // Colores de estado de las 36 órdenes
    private static readonly Color32 ColorNeonGreen = new Color32(0x38, 0xF2, 0x8A, 240); // #38F28A - Bloqueadas / Retenidas por defensa
    private static readonly Color32 ColorRed = new Color32(0xFF, 0x5A, 0x4E, 240);       // #FF5A4E - Pagadas a Manila (4)
    private static readonly Color32 ColorAmber = new Color32(0xFF, 0xC2, 0x4A, 240);     // #FFC24A - Fandation (1)
    private static readonly Color32 ColorDarkGray = new Color32(0x3E, 0x4A, 0x44, 230);  // Gris Oscuro - No reenviada (1)

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
        EnsureMatrixCreated();
        bool initialDefense = InventoryUI.Instance != null &&
                              InventoryUI.Instance.ruleEngine != null &&
                              InventoryUI.Instance.ruleEngine.isDefenseMode;
        UpdateMatrix(initialDefense);
    }

    public void EnsureMatrixCreated()
    {
        if (matrixPanel != null && matrixCells.Count == TOTAL_ORDERS)
            return;

        Canvas targetCanvas = FindTargetCanvas();
        if (targetCanvas == null)
            return;

        Transform existing = targetCanvas.transform.Find("Panel_Matriz_36_Ordenes");
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        matrixCells.Clear();

        // Panel contenedor transparente en el centro-izquierda de la pantalla
        matrixPanel = new GameObject("Panel_Matriz_36_Ordenes", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        matrixPanel.transform.SetParent(targetCanvas.transform, false);

        RectTransform panelRect = matrixPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 0.5f);
        panelRect.anchorMax = new Vector2(0f, 0.5f);
        panelRect.pivot = new Vector2(0f, 0.5f);
        panelRect.anchoredPosition = new Vector2(22f, -10f);
        panelRect.sizeDelta = new Vector2(215f, 250f);

        Image panelBg = matrixPanel.GetComponent<Image>();
        panelBg.color = new Color(0f, 0f, 0f, 0f); // Transparente
        panelBg.raycastTarget = false;

        // Etiqueta superior minimalista de la matriz
        GameObject titleObj = new GameObject("Titulo_Matriz", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(matrixPanel.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, 0f);
        titleRect.sizeDelta = new Vector2(0f, 26f);

        matrixTitleText = titleObj.GetComponent<TextMeshProUGUI>();
        matrixTitleText.text = "MATRIZ SWIFT [36 ÓRDENES]";
        matrixTitleText.fontSize = 13.5f;
        matrixTitleText.fontStyle = FontStyles.Bold;
        matrixTitleText.color = ColorNeonGreen;
        matrixTitleText.alignment = TextAlignmentOptions.MidlineLeft;
        matrixTitleText.raycastTarget = false;

        // Contenedor con GridLayoutGroup (6 columnas, celdas cuadradas, espaciado 5px)
        GameObject gridObj = new GameObject("Grid_36_Ordenes", typeof(RectTransform), typeof(GridLayoutGroup));
        gridObj.transform.SetParent(matrixPanel.transform, false);

        RectTransform gridRect = gridObj.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0f, 0f);
        gridRect.anchorMax = new Vector2(1f, 1f);
        gridRect.pivot = new Vector2(0f, 1f);
        gridRect.offsetMin = new Vector2(0f, 0f);
        gridRect.offsetMax = new Vector2(0f, -30f);

        GridLayoutGroup grid = gridObj.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 6;
        grid.cellSize = new Vector2(28f, 28f);
        grid.spacing = new Vector2(5f, 5f);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;

        for (int i = 0; i < TOTAL_ORDERS; i++)
        {
            GameObject cellObj = new GameObject($"Orden_Cell_{i + 1:00}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Outline));
            cellObj.transform.SetParent(gridObj.transform, false);

            Image cellImg = cellObj.GetComponent<Image>();
            cellImg.color = ColorNeonGreen;
            cellImg.raycastTarget = false;

            Outline cellOutline = cellObj.GetComponent<Outline>();
            cellOutline.effectColor = new Color32(0x02, 0x0A, 0x06, 200);
            cellOutline.effectDistance = new Vector2(1f, -1f);

            matrixCells.Add(cellImg);
        }
    }

    public void UpdateMatrix(bool isDefenseMode)
    {
        EnsureMatrixCreated();
        if (matrixCells.Count < TOTAL_ORDERS)
            return;

        if (isDefenseMode)
        {
            if (matrixTitleText != null)
                matrixTitleText.text = "MATRIZ SWIFT [36 RETENIDAS]";

            for (int i = 0; i < TOTAL_ORDERS; i++)
            {
                matrixCells[i].color = ColorNeonGreen;
            }
        }
        else
        {
            if (matrixTitleText != null)
                matrixTitleText.text = "MATRIZ SWIFT [ATAQUE 2016]";

            // 30 verdes (#38F28A) [Bloqueadas], 4 rojos (#FF5A4E) [Pagadas a Manila],
            // 1 ámbar (#FFC24A) [Fandation], 1 gris oscuro [No reenviada]
            for (int i = 0; i < TOTAL_ORDERS; i++)
            {
                if (i < 30)
                    matrixCells[i].color = ColorNeonGreen;
                else if (i < 34)
                    matrixCells[i].color = ColorRed;
                else if (i == 34)
                    matrixCells[i].color = ColorAmber;
                else
                    matrixCells[i].color = ColorDarkGray;
            }
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

