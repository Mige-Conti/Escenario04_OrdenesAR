using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HologramDiorama3D : MonoBehaviour
{
    private enum DioramaBehavior
    {
        DefenseFirewall,
        AttackPaidManila,
        AttackFandationBounce,
        AttackFormatDisintegrate,
        AttackJupiterFrozen
    }

    private DioramaBehavior currentBehavior = DioramaBehavior.AttackPaidManila;
    private bool isBuilt = false;

    // Contenedores principales
    private Transform dioramaRoot;
    private Transform stageContainer;
    private Transform scanLaserBar;
    private Renderer scanLaserRenderer;

    // Nodo Izquierdo: Terminal SWIFT + Impresora
    private Transform leftNodeRoot;
    private Renderer monitorScreenRenderer;
    private Renderer printerLedRenderer;
    private TextMeshPro etiquetaImpresora3D;

    // Nodo Central: Muro Firewall / Escáner Intermediario
    private Transform centerNodeRoot;
    private Transform firewallShieldMesh;
    private Transform scannerRingLeft;
    private Transform scannerRingRight;
    private Renderer firewallShieldRenderer;
    private Renderer scannerRingLeftRenderer;
    private Renderer scannerRingRightRenderer;
    private TextMeshPro etiquetaAccion3D;
    private TextMeshPro etiquetaSubNodoCentral3D;

    // Nodo Derecho: Servidor Destino (Manila / Colombo / NY Fed)
    private Transform rightNodeRoot;
    private Renderer serverBodyRenderer;
    private readonly List<Renderer> serverRackLeds = new List<Renderer>();
    private TextMeshPro etiquetaServidorDerecho3D;

    // Flujo de paquetes de datos (3 cubos luminosos)
    private const int PACKET_COUNT = 3;
    private const float LEFT_X = -0.055f;
    private const float CENTER_X = 0.0f;
    private const float RIGHT_X = 0.055f;
    private const float PACKET_Y = 0.014f;

    private readonly List<Transform> paquetesDatos = new List<Transform>();
    private readonly List<Renderer> paquetesRenderers = new List<Renderer>();

    // Colores Cyberpunk Holográficos
    private static readonly Color ColorNeonGreen = new Color32(0x38, 0xF2, 0x8A, 255); // #38F28A
    private static readonly Color ColorRedAlert = new Color32(0xFF, 0x5A, 0x4E, 255);  // #FF5A4E
    private static readonly Color ColorAmber = new Color32(0xFF, 0xC2, 0x4A, 255);     // #FFC24A
    private static readonly Color ColorCyan = new Color32(0x4D, 0xF7, 0xE2, 255);      // #4DF7E2
    private static readonly Color ColorGray = new Color32(0x75, 0x85, 0x7E, 255);      // Gris
    private static readonly Color ColorDarkChassis = new Color32(0x0B, 0x1E, 0x14, 255);

    private Coroutine popInCoroutine;

    void Awake()
    {
        BuildDiorama();
    }

    public void BuildDiorama()
    {
        if (isBuilt && dioramaRoot != null)
            return;

        Transform existing = transform.Find("Diorama_Holografico_3D");
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        paquetesDatos.Clear();
        paquetesRenderers.Clear();
        serverRackLeds.Clear();

        dioramaRoot = new GameObject("Diorama_Holografico_3D").transform;
        dioramaRoot.SetParent(transform, false);
        dioramaRoot.localPosition = Vector3.zero;
        dioramaRoot.localRotation = Quaternion.identity;
        dioramaRoot.localScale = Vector3.one;

        // a) Láser de Escaneo Forense que recorre toda la Base_Tarjeta de adelante hacia atrás
        scanLaserBar = CreatePrimitiveNoCollider(PrimitiveType.Cube, "Laser_Escaneo_Forense", dioramaRoot);
        scanLaserBar.localPosition = new Vector3(0f, 0.0265f, 0f);
        scanLaserBar.localScale = new Vector3(0.162f, 0.002f, 0.0035f);
        scanLaserRenderer = scanLaserBar.GetComponent<Renderer>();
        ApplyHologramColor(scanLaserRenderer, ColorNeonGreen, 1.8f);

        // Plataforma / Escenario en la parte superior/trasera de la Base_Tarjeta (Z = 0.04, Y = 0.025)
        stageContainer = new GameObject("Maqueta_Nodos_SWIFT").transform;
        stageContainer.SetParent(dioramaRoot, false);
        stageContainer.localPosition = new Vector3(0f, 0.025f, 0.042f);
        stageContainer.localRotation = Quaternion.identity;
        stageContainer.localScale = Vector3.one;

        // Línea guía de red entre los 3 nodos
        Transform netLine = CreatePrimitiveNoCollider(PrimitiveType.Cube, "Linea_Red_SWIFT", stageContainer);
        netLine.localPosition = new Vector3(0f, 0.002f, 0f);
        netLine.localScale = new Vector3(0.115f, 0.0012f, 0.002f);
        ApplyHologramColor(netLine.GetComponent<Renderer>(), new Color32(0x18, 0x55, 0x35, 255), 0.5f);

        // b) Nodo Emisor (Izquierda, X = -0.055): Terminal SWIFT + Impresora con LED
        BuildEmitterNode();

        // c) Nodo Central (Centro, X = 0): Muro Firewall / Escáner + Etiqueta 3D
        BuildCentralNode();

        // d) Nodo Receptor (Derecha, X = +0.055): Torre de Servidor Destino
        BuildReceiverNode();

        // e) Flujo de Paquetes de Datos (3 pequeños cubos luminosos)
        BuildDataPackets();

        isBuilt = true;
    }

    private void BuildEmitterNode()
    {
        leftNodeRoot = new GameObject("Nodo_Emisor_SWIFT").transform;
        leftNodeRoot.SetParent(stageContainer, false);
        leftNodeRoot.localPosition = new Vector3(LEFT_X, 0f, 0f);

        // Base de la computadora
        Transform pcBase = CreatePrimitiveNoCollider(PrimitiveType.Cylinder, "PC_Base", leftNodeRoot);
        pcBase.localPosition = new Vector3(0f, 0.002f, 0f);
        pcBase.localScale = new Vector3(0.014f, 0.002f, 0.014f);
        ApplyHologramColor(pcBase.GetComponent<Renderer>(), ColorDarkChassis, 0.2f);

        // Soporte vertical
        Transform pcStand = CreatePrimitiveNoCollider(PrimitiveType.Cube, "PC_Soporte", leftNodeRoot);
        pcStand.localPosition = new Vector3(0f, 0.006f, 0f);
        pcStand.localScale = new Vector3(0.003f, 0.006f, 0.003f);
        ApplyHologramColor(pcStand.GetComponent<Renderer>(), ColorDarkChassis, 0.2f);

        // Monitor carcasa
        Transform pcMonitor = CreatePrimitiveNoCollider(PrimitiveType.Cube, "PC_Monitor", leftNodeRoot);
        pcMonitor.localPosition = new Vector3(0f, 0.013f, 0f);
        pcMonitor.localRotation = Quaternion.Euler(0f, 15f, 0f);
        pcMonitor.localScale = new Vector3(0.016f, 0.012f, 0.003f);
        ApplyHologramColor(pcMonitor.GetComponent<Renderer>(), ColorDarkChassis, 0.3f);

        // Pantalla luminosa del monitor
        Transform pcScreen = CreatePrimitiveNoCollider(PrimitiveType.Cube, "PC_Pantalla", pcMonitor);
        pcScreen.localPosition = new Vector3(0f, 0f, -0.55f);
        pcScreen.localScale = new Vector3(0.86f, 0.8f, 0.2f);
        monitorScreenRenderer = pcScreen.GetComponent<Renderer>();
        ApplyHologramColor(monitorScreenRenderer, ColorCyan, 1.5f);

        // Mini Impresora al lado de la terminal SWIFT
        Transform printerBody = CreatePrimitiveNoCollider(PrimitiveType.Cube, "Mini_Impresora", leftNodeRoot);
        printerBody.localPosition = new Vector3(-0.015f, 0.0045f, -0.002f);
        printerBody.localScale = new Vector3(0.010f, 0.007f, 0.009f);
        ApplyHologramColor(printerBody.GetComponent<Renderer>(), new Color32(0x1E, 0x26, 0x22, 255), 0.2f);

        // LED rojo parpadeante sobre la impresora
        Transform printerLed = CreatePrimitiveNoCollider(PrimitiveType.Sphere, "LED_Impresora", printerBody);
        printerLed.localPosition = new Vector3(0.28f, 0.6f, -0.25f);
        printerLed.localScale = new Vector3(0.32f, 0.32f, 0.32f);
        printerLedRenderer = printerLed.GetComponent<Renderer>();
        ApplyHologramColor(printerLedRenderer, ColorRedAlert, 2.0f);

        // Etiqueta 3D de la impresora / terminal
        etiquetaImpresora3D = CreateFloatingText3D(
            "Txt_Impresora_3D",
            leftNodeRoot,
            new Vector3(-0.006f, 0.025f, 0f),
            0.032f,
            ColorRedAlert,
            "TERMINAL SWIFT\nIMPRESORA: OFF"
        );
    }

    private void BuildCentralNode()
    {
        centerNodeRoot = new GameObject("Nodo_Central_Validacion").transform;
        centerNodeRoot.SetParent(stageContainer, false);
        centerNodeRoot.localPosition = new Vector3(CENTER_X, 0f, 0f);

        // Muro / Escudo de Firewall central
        firewallShieldMesh = CreatePrimitiveNoCollider(PrimitiveType.Cube, "Escudo_Firewall", centerNodeRoot);
        firewallShieldMesh.localPosition = new Vector3(0f, 0.014f, 0f);
        firewallShieldMesh.localScale = new Vector3(0.004f, 0.024f, 0.022f);
        firewallShieldRenderer = firewallShieldMesh.GetComponent<Renderer>();
        ApplyHologramColor(firewallShieldRenderer, ColorNeonGreen, 1.8f);

        // Pilares / Anillo de escaneo lateral del nodo central
        scannerRingLeft = CreatePrimitiveNoCollider(PrimitiveType.Cylinder, "Pilar_Escaner_A", centerNodeRoot);
        scannerRingLeft.localPosition = new Vector3(0f, 0.014f, -0.012f);
        scannerRingLeft.localScale = new Vector3(0.0035f, 0.013f, 0.0035f);
        scannerRingLeftRenderer = scannerRingLeft.GetComponent<Renderer>();
        ApplyHologramColor(scannerRingLeftRenderer, ColorNeonGreen, 1.6f);

        scannerRingRight = CreatePrimitiveNoCollider(PrimitiveType.Cylinder, "Pilar_Escaner_B", centerNodeRoot);
        scannerRingRight.localPosition = new Vector3(0f, 0.014f, 0.012f);
        scannerRingRight.localScale = new Vector3(0.0035f, 0.013f, 0.0035f);
        scannerRingRightRenderer = scannerRingRight.GetComponent<Renderer>();
        ApplyHologramColor(scannerRingRightRenderer, ColorNeonGreen, 1.6f);

        // Etiqueta principal flotante (2 líneas explicando la acción en tiempo real)
        etiquetaAccion3D = CreateFloatingText3D(
            "Etiqueta_Accion_3D",
            centerNodeRoot,
            new Vector3(0f, 0.040f, 0f),
            0.038f,
            ColorNeonGreen,
            "FIREWALL ZERO TRUST ACTIVO\n[Doble Firma + Límite + Lista Blanca: RETENIDA]"
        );

        // Sub-etiqueta de estado del muro central
        etiquetaSubNodoCentral3D = CreateFloatingText3D(
            "Etiqueta_Estado_Muro_3D",
            centerNodeRoot,
            new Vector3(0f, 0.028f, 0f),
            0.028f,
            ColorCyan,
            "NODO CENTRAL"
        );
    }

    private void BuildReceiverNode()
    {
        rightNodeRoot = new GameObject("Nodo_Receptor_Servidor").transform;
        rightNodeRoot.SetParent(stageContainer, false);
        rightNodeRoot.localPosition = new Vector3(RIGHT_X, 0f, 0f);

        // Torre de servidor destino
        Transform serverTower = CreatePrimitiveNoCollider(PrimitiveType.Cube, "Torre_Servidor", rightNodeRoot);
        serverTower.localPosition = new Vector3(0f, 0.013f, 0f);
        serverTower.localScale = new Vector3(0.014f, 0.024f, 0.014f);
        serverBodyRenderer = serverTower.GetComponent<Renderer>();
        ApplyHologramColor(serverBodyRenderer, ColorDarkChassis, 0.3f);

        // 3 bahías LED frontales en la torre de servidor
        for (int i = 0; i < 3; i++)
        {
            Transform ledBar = CreatePrimitiveNoCollider(PrimitiveType.Cube, $"Rack_LED_{i + 1}", serverTower);
            ledBar.localPosition = new Vector3(-0.52f, -0.25f + (i * 0.25f), 0f);
            ledBar.localScale = new Vector3(0.12f, 0.12f, 0.78f);
            Renderer r = ledBar.GetComponent<Renderer>();
            ApplyHologramColor(r, ColorNeonGreen, 1.6f);
            serverRackLeds.Add(r);
        }

        // Etiqueta sobre el servidor receptor
        etiquetaServidorDerecho3D = CreateFloatingText3D(
            "Txt_Servidor_Destino_3D",
            rightNodeRoot,
            new Vector3(0f, 0.029f, 0f),
            0.031f,
            ColorCyan,
            "SERVIDOR DESTINO\n[RCBC / EXTERIOR]"
        );
    }

    private void BuildDataPackets()
    {
        for (int i = 0; i < PACKET_COUNT; i++)
        {
            Transform pkt = CreatePrimitiveNoCollider(PrimitiveType.Cube, $"Paquete_Datos_{i + 1}", stageContainer);
            pkt.localPosition = new Vector3(LEFT_X, PACKET_Y, 0f);
            pkt.localScale = new Vector3(0.0055f, 0.0055f, 0.0055f);

            Renderer r = pkt.GetComponent<Renderer>();
            ApplyHologramColor(r, ColorNeonGreen, 2.0f);

            paquetesDatos.Add(pkt);
            paquetesRenderers.Add(r);
        }
    }

    public void ConfigureAnimation(string orderId, string realState, bool isDefenseMode)
    {
        BuildDiorama();

        string id = string.IsNullOrEmpty(orderId) ? "ORDEN_01" : orderId.Trim().ToUpper();

        if (isDefenseMode)
        {
            currentBehavior = DioramaBehavior.DefenseFirewall;

            // Muro Firewall Verde Neón activo
            firewallShieldMesh.localPosition = new Vector3(0f, 0.014f, 0f);
            firewallShieldMesh.localScale = new Vector3(0.004f, 0.024f, 0.024f);
            ApplyHologramColor(firewallShieldRenderer, ColorNeonGreen, 2.0f);
            ApplyHologramColor(scannerRingLeftRenderer, ColorNeonGreen, 1.8f);
            ApplyHologramColor(scannerRingRightRenderer, ColorNeonGreen, 1.8f);
            ApplyHologramColor(scanLaserRenderer, ColorNeonGreen, 1.8f);
            ApplyHologramColor(monitorScreenRenderer, ColorNeonGreen, 1.5f);
            ApplyHologramColor(printerLedRenderer, ColorNeonGreen, 1.6f);

            SetPacketsColor(ColorCyan);
            SetServerLedsColor(ColorNeonGreen);

            if (etiquetaImpresora3D != null)
            {
                etiquetaImpresora3D.text = "TERMINAL SWIFT\nAUDITORÍA: ON";
                etiquetaImpresora3D.color = ColorNeonGreen;
            }

            if (etiquetaSubNodoCentral3D != null)
            {
                etiquetaSubNodoCentral3D.text = "[ ESCUDO ZERO TRUST ]";
                etiquetaSubNodoCentral3D.color = ColorNeonGreen;
            }

            if (etiquetaServidorDerecho3D != null)
            {
                etiquetaServidorDerecho3D.text = "BÓVEDA PROTEGIDA\n[US$ 951 M SEGUROS]";
                etiquetaServidorDerecho3D.color = ColorNeonGreen;
            }

            if (etiquetaAccion3D != null)
            {
                etiquetaAccion3D.text = "FIREWALL ZERO TRUST ACTIVO\n[Doble Firma + Límite + Lista Blanca: RETENIDA]";
                etiquetaAccion3D.color = ColorNeonGreen;
            }
            return;
        }

        // MODO ATAQUE (isDefenseMode == false)
        if (id == "ORDEN_01" || id == "ORDEN_02" || id == "ORDEN_03" || id == "ORDEN_04")
        {
            currentBehavior = DioramaBehavior.AttackPaidManila;

            // Muro central caído/desactivado
            firewallShieldMesh.localPosition = new Vector3(0f, 0.002f, 0f);
            firewallShieldMesh.localScale = new Vector3(0.004f, 0.0025f, 0.018f);
            ApplyHologramColor(firewallShieldRenderer, new Color32(0x55, 0x15, 0x15, 255), 0.4f);
            ApplyHologramColor(scannerRingLeftRenderer, new Color32(0x55, 0x15, 0x15, 255), 0.4f);
            ApplyHologramColor(scannerRingRightRenderer, new Color32(0x55, 0x15, 0x15, 255), 0.4f);
            ApplyHologramColor(scanLaserRenderer, ColorRedAlert, 1.8f);
            ApplyHologramColor(monitorScreenRenderer, ColorRedAlert, 1.5f);

            SetPacketsColor(ColorRedAlert);

            if (etiquetaImpresora3D != null)
            {
                etiquetaImpresora3D.text = "IMPRESORA: OFF";
                etiquetaImpresora3D.color = ColorRedAlert;
            }

            if (etiquetaSubNodoCentral3D != null)
            {
                etiquetaSubNodoCentral3D.text = "SIN FIREWALL / SIN DOBLE APROBACIÓN";
                etiquetaSubNodoCentral3D.color = ColorRedAlert;
            }

            if (etiquetaServidorDerecho3D != null)
            {
                etiquetaServidorDerecho3D.text = "SERVIDOR MANILA\n[RCBC JUPITER ST.]";
                etiquetaServidorDerecho3D.color = ColorRedAlert;
            }

            if (etiquetaAccion3D != null)
            {
                etiquetaAccion3D.text = "TRANSFERENCIA FUGADA A MANILA\n[Impresora Apagada · Sin Límite Transaccional]";
                etiquetaAccion3D.color = ColorRedAlert;
            }
        }
        else if (id == "ORDEN_05")
        {
            currentBehavior = DioramaBehavior.AttackFandationBounce;

            // Escáner Ámbar del Banco Intermediario en Colombo
            firewallShieldMesh.localPosition = new Vector3(0f, 0.014f, 0f);
            firewallShieldMesh.localScale = new Vector3(0.0045f, 0.022f, 0.022f);
            ApplyHologramColor(firewallShieldRenderer, ColorAmber, 1.9f);
            ApplyHologramColor(scannerRingLeftRenderer, ColorAmber, 1.8f);
            ApplyHologramColor(scannerRingRightRenderer, ColorAmber, 1.8f);
            ApplyHologramColor(scanLaserRenderer, ColorAmber, 1.8f);
            ApplyHologramColor(monitorScreenRenderer, ColorAmber, 1.4f);

            SetPacketsColor(ColorAmber);
            SetServerLedsColor(ColorAmber);

            if (etiquetaImpresora3D != null)
            {
                etiquetaImpresora3D.text = "IMPRESORA: OFF";
                etiquetaImpresora3D.color = ColorAmber;
            }

            if (etiquetaSubNodoCentral3D != null)
            {
                etiquetaSubNodoCentral3D.text = "[ ESCÁNER BANCO COLOMBO ]";
                etiquetaSubNodoCentral3D.color = ColorAmber;
            }

            if (etiquetaServidorDerecho3D != null)
            {
                etiquetaServidorDerecho3D.text = "SRI LANKA\n[SHALIKA FANDATION]";
                etiquetaServidorDerecho3D.color = ColorAmber;
            }

            if (etiquetaAccion3D != null)
            {
                etiquetaAccion3D.text = "RECHAZADA EN NODO COLOMBO\n[Errata detectada: 'FANDATION' != 'FOUNDATION']";
                etiquetaAccion3D.color = ColorAmber;
            }
        }
        else if (id == "ORDEN_06")
        {
            currentBehavior = DioramaBehavior.AttackFormatDisintegrate;

            // Nodo central en espera, paquete se desintegra al salir de la PC
            firewallShieldMesh.localPosition = new Vector3(0f, 0.010f, 0f);
            firewallShieldMesh.localScale = new Vector3(0.003f, 0.014f, 0.016f);
            ApplyHologramColor(firewallShieldRenderer, ColorGray, 0.8f);
            ApplyHologramColor(scannerRingLeftRenderer, ColorGray, 0.8f);
            ApplyHologramColor(scannerRingRightRenderer, ColorGray, 0.8f);
            ApplyHologramColor(scanLaserRenderer, ColorGray, 1.2f);
            ApplyHologramColor(monitorScreenRenderer, ColorGray, 1.0f);

            SetPacketsColor(ColorGray);
            SetServerLedsColor(ColorGray);

            if (etiquetaImpresora3D != null)
            {
                etiquetaImpresora3D.text = "FORMATO INVÁLIDO";
                etiquetaImpresora3D.color = ColorGray;
            }

            if (etiquetaSubNodoCentral3D != null)
            {
                etiquetaSubNodoCentral3D.text = "[ SIN INTERMEDIARIO ]";
                etiquetaSubNodoCentral3D.color = ColorGray;
            }

            if (etiquetaServidorDerecho3D != null)
            {
                etiquetaServidorDerecho3D.text = "DESTINO INCOMPLETO\n[NO REENVIADA]";
                etiquetaServidorDerecho3D.color = ColorGray;
            }

            if (etiquetaAccion3D != null)
            {
                etiquetaAccion3D.text = "ERROR DE FORMATO SWIFT MT103\n[Falta Banco Intermediario · No Reenviada]";
                etiquetaAccion3D.color = ColorGray;
            }
        }
        else
        {
            // ORDEN_07 y ORDEN_08 (Bloqueadas por Alerta Jupiter - US$ 850M)
            currentBehavior = DioramaBehavior.AttackJupiterFrozen;

            firewallShieldMesh.localPosition = new Vector3(0f, 0.014f, 0f);
            firewallShieldMesh.localScale = new Vector3(0.005f, 0.024f, 0.024f);
            ApplyHologramColor(firewallShieldRenderer, ColorCyan, 1.9f);
            ApplyHologramColor(scannerRingLeftRenderer, ColorNeonGreen, 1.8f);
            ApplyHologramColor(scannerRingRightRenderer, ColorNeonGreen, 1.8f);
            ApplyHologramColor(scanLaserRenderer, ColorNeonGreen, 1.8f);
            ApplyHologramColor(monitorScreenRenderer, ColorCyan, 1.4f);

            SetPacketsColor(ColorCyan);
            SetServerLedsColor(ColorNeonGreen);

            if (etiquetaImpresora3D != null)
            {
                etiquetaImpresora3D.text = "LOTE MASIVO (15)";
                etiquetaImpresora3D.color = ColorCyan;
            }

            if (etiquetaSubNodoCentral3D != null)
            {
                etiquetaSubNodoCentral3D.text = "[ INSPECCIÓN NY FED / OFAC ]";
                etiquetaSubNodoCentral3D.color = ColorCyan;
            }

            if (etiquetaServidorDerecho3D != null)
            {
                etiquetaServidorDerecho3D.text = "MANILA BLOQUEADO\n[ALERTA 'JUPITER']";
                etiquetaServidorDerecho3D.color = ColorNeonGreen;
            }

            if (etiquetaAccion3D != null)
            {
                etiquetaAccion3D.text = "CONGELADA EN RESERVA FEDERAL\n[Alerta Lista Negra OFAC: Calle 'JUPITER']";
                etiquetaAccion3D.color = ColorNeonGreen;
            }
        }
    }

    public void PlayPopInAnimation()
    {
        BuildDiorama();
        if (!gameObject.activeInHierarchy)
            return;

        if (popInCoroutine != null)
        {
            StopCoroutine(popInCoroutine);
        }
        popInCoroutine = StartCoroutine(PopInRoutine(0.35f));
    }

    private IEnumerator PopInRoutine(float duration)
    {
        float elapsed = 0f;
        transform.localScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Curva suave con ligero overshoot (ease-out back sutil)
            float smooth = 1f - Mathf.Pow(1f - t, 3f);
            transform.localScale = Vector3.one * smooth;
            yield return null;
        }

        transform.localScale = Vector3.one;
        popInCoroutine = null;
    }

    void Update()
    {
        if (!isBuilt || dioramaRoot == null)
            return;

        float time = Time.time;

        // 1. Animar el Láser de Escaneo Forense de adelante hacia atrás con Mathf.PingPong
        if (scanLaserBar != null)
        {
            float zSweep = Mathf.Lerp(-0.052f, 0.052f, Mathf.PingPong(time * 0.85f, 1f));
            scanLaserBar.localPosition = new Vector3(0f, 0.0265f, zSweep);
        }

        // 2. Parpadeo del LED de la impresora desconectada
        if (printerLedRenderer != null)
        {
            bool blinkOn = Mathf.Sin(time * 8f) > 0f;
            if (currentBehavior == DioramaBehavior.DefenseFirewall)
            {
                ApplyHologramColor(printerLedRenderer, ColorNeonGreen, 1.6f);
            }
            else
            {
                ApplyHologramColor(printerLedRenderer, blinkOn ? ColorRedAlert : new Color32(0x35, 0x08, 0x08, 255), blinkOn ? 2.2f : 0.3f);
            }
        }

        // 3. Animar Nodos y Paquetes según el caso forense activo
        switch (currentBehavior)
        {
            case DioramaBehavior.DefenseFirewall:
                UpdateDefenseFirewall(time);
                break;

            case DioramaBehavior.AttackPaidManila:
                UpdateAttackPaidManila(time);
                break;

            case DioramaBehavior.AttackFandationBounce:
                UpdateAttackFandationBounce(time);
                break;

            case DioramaBehavior.AttackFormatDisintegrate:
                UpdateAttackFormatDisintegrate(time);
                break;

            case DioramaBehavior.AttackJupiterFrozen:
                UpdateAttackJupiterFrozen(time);
                break;
        }
    }

    private void UpdateDefenseFirewall(float time)
    {
        // Muro Firewall verde neón pulsa y gira suavemente
        if (firewallShieldMesh != null)
        {
            float pulse = 0.022f + Mathf.Sin(time * 4f) * 0.003f;
            firewallShieldMesh.localScale = new Vector3(0.0045f, pulse, 0.024f);
            firewallShieldMesh.localRotation = Quaternion.Euler(0f, Mathf.Sin(time * 2.2f) * 18f, 0f);
        }

        // Los paquetes salen de la PC, chocan contra el Muro en el centro y se detienen/reinician
        for (int i = 0; i < paquetesDatos.Count; i++)
        {
            float cycle = Mathf.Repeat(time * 0.9f + (i * 0.33f), 1f);
            float xPos;
            float scaleFactor = 1f;

            if (cycle < 0.72f)
            {
                float moveT = cycle / 0.72f;
                xPos = Mathf.Lerp(LEFT_X + 0.008f, CENTER_X - 0.005f, moveT);
            }
            else
            {
                // Choque y compresión contra el Firewall antes de reiniciar
                float hitT = (cycle - 0.72f) / 0.28f;
                xPos = CENTER_X - 0.005f;
                scaleFactor = Mathf.Lerp(1.25f, 0.1f, hitT);
            }

            paquetesDatos[i].localPosition = new Vector3(xPos, PACKET_Y, 0f);
            paquetesDatos[i].localRotation = Quaternion.Euler(time * 120f, time * 180f, 0f);
            paquetesDatos[i].localScale = Vector3.one * (0.0055f * scaleFactor);
        }
    }

    private void UpdateAttackPaidManila(float time)
    {
        if (firewallShieldMesh != null)
        {
            firewallShieldMesh.localRotation = Quaternion.identity;
        }

        // Los paquetes rojos viajan sin obstáculos de izquierda (SWIFT) a derecha (Servidor Manila)
        bool serverHitPulse = false;
        for (int i = 0; i < paquetesDatos.Count; i++)
        {
            float cycle = Mathf.Repeat(time * 0.95f + (i * 0.33f), 1f);
            float xPos = Mathf.Lerp(LEFT_X + 0.008f, RIGHT_X - 0.006f, cycle);

            paquetesDatos[i].localPosition = new Vector3(xPos, PACKET_Y, 0f);
            paquetesDatos[i].localRotation = Quaternion.Euler(0f, time * 200f, time * 140f);
            paquetesDatos[i].localScale = Vector3.one * 0.0058f;

            if (cycle > 0.82f)
                serverHitPulse = true;
        }

        // El servidor derecho parpadea en rojo al recibir el dinero robado
        bool fastBlink = serverHitPulse || (Mathf.Sin(time * 12f) > 0.2f);
        Color srvColor = fastBlink ? ColorRedAlert : new Color32(0x35, 0x0A, 0x0A, 255);
        SetServerLedsColor(srvColor);
        if (serverBodyRenderer != null)
        {
            ApplyHologramColor(serverBodyRenderer, fastBlink ? new Color32(0x55, 0x12, 0x12, 255) : ColorDarkChassis, fastBlink ? 1.1f : 0.3f);
        }
    }

    private void UpdateAttackFandationBounce(float time)
    {
        // Escáner Ámbar en Colombo
        if (firewallShieldMesh != null)
        {
            firewallShieldMesh.localRotation = Quaternion.Euler(0f, Mathf.Sin(time * 3.5f) * 25f, 0f);
        }

        // Los paquetes salen de la PC, llegan al centro, chocan y REBOTAN de regreso hacia la izquierda
        for (int i = 0; i < paquetesDatos.Count; i++)
        {
            float cycle = Mathf.Repeat(time * 0.75f + (i * 0.33f), 1f);
            float xPos;
            float yArc = PACKET_Y;

            if (cycle < 0.5f)
            {
                // Ida hacia el nodo central de Colombo
                float tOut = cycle / 0.5f;
                xPos = Mathf.Lerp(LEFT_X + 0.008f, CENTER_X - 0.004f, tOut);
            }
            else
            {
                // Rebote de regreso hacia la izquierda con ligero arco
                float tBack = (cycle - 0.5f) / 0.5f;
                xPos = Mathf.Lerp(CENTER_X - 0.004f, LEFT_X + 0.008f, tBack);
                yArc = PACKET_Y + Mathf.Sin(tBack * Mathf.PI) * 0.007f;
            }

            paquetesDatos[i].localPosition = new Vector3(xPos, yArc, 0f);
            paquetesDatos[i].localRotation = Quaternion.Euler(time * 160f, time * 160f, 0f);
            paquetesDatos[i].localScale = Vector3.one * 0.0055f;
        }
    }

    private void UpdateAttackFormatDisintegrate(float time)
    {
        // ORDEN_06: Los paquetes grises salen apenas unos milímetros de la PC emisora y se encogen/desintegran antes de llegar al centro
        for (int i = 0; i < paquetesDatos.Count; i++)
        {
            float cycle = Mathf.Repeat(time * 0.8f + (i * 0.33f), 1f);
            // Avanzan solo desde LEFT_X (+0.007) hasta mitad de camino antes del centro (-0.026)
            float xPos = Mathf.Lerp(LEFT_X + 0.007f, -0.026f, cycle);
            float shrink = Mathf.Clamp01(1f - cycle);

            paquetesDatos[i].localPosition = new Vector3(xPos, PACKET_Y + (cycle * 0.003f), 0f);
            paquetesDatos[i].localRotation = Quaternion.Euler(time * 90f, time * 110f, time * 70f);
            paquetesDatos[i].localScale = Vector3.one * (0.0055f * shrink);
        }
    }

    private void UpdateAttackJupiterFrozen(float time)
    {
        // ORDEN_07 / ORDEN_08: Barrera de inspección NY Fed; los paquetes llegan al centro y quedan flotando congelados girando sobre sí mismos
        if (firewallShieldMesh != null)
        {
            firewallShieldMesh.localRotation = Quaternion.Euler(0f, time * 40f, 0f);
        }

        for (int i = 0; i < paquetesDatos.Count; i++)
        {
            float cycle = Mathf.Repeat(time * 0.45f + (i * 0.33f), 1f);
            float xPos;
            float yPos = PACKET_Y;
            float zOffset = (i - 1) * 0.0065f;

            if (cycle < 0.32f)
            {
                // Llegan desde la terminal hasta la zona de inspección central
                float tArrive = cycle / 0.32f;
                xPos = Mathf.Lerp(LEFT_X + 0.008f, CENTER_X - 0.005f + (i * 0.004f), tArrive);
            }
            else
            {
                // Quedan flotando congelados en el aire en el nodo central girando sobre sí mismos
                xPos = CENTER_X - 0.005f + (i * 0.004f);
                yPos = PACKET_Y + Mathf.Sin(time * 3.5f + i * 1.5f) * 0.0025f;
            }

            paquetesDatos[i].localPosition = new Vector3(xPos, yPos, zOffset);
            paquetesDatos[i].localRotation = Quaternion.Euler(time * 150f, time * 210f, time * 90f);
            paquetesDatos[i].localScale = Vector3.one * 0.0055f;
        }
    }

    private void SetPacketsColor(Color c)
    {
        for (int i = 0; i < paquetesRenderers.Count; i++)
        {
            ApplyHologramColor(paquetesRenderers[i], c, 2.0f);
        }
    }

    private void SetServerLedsColor(Color c)
    {
        for (int i = 0; i < serverRackLeds.Count; i++)
        {
            ApplyHologramColor(serverRackLeds[i], c, 1.8f);
        }
    }

    private Transform CreatePrimitiveNoCollider(PrimitiveType type, string name, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);

        Collider col = go.GetComponent<Collider>();
        if (col != null)
        {
            Destroy(col);
        }

        return go.transform;
    }

    private TextMeshPro CreateFloatingText3D(
        string name,
        Transform parent,
        Vector3 localPos,
        float fontSize,
        Color color,
        string initialText)
    {
        GameObject txtObj = new GameObject(name, typeof(RectTransform));
        txtObj.transform.SetParent(parent, false);
        txtObj.transform.localPosition = localPos;
        // Inclinación de 55 grados para lectura óptima tanto frontal como superior en cámara AR
        txtObj.transform.localRotation = Quaternion.Euler(55f, 0f, 0f);
        txtObj.transform.localScale = Vector3.one;

        RectTransform rt = txtObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0.18f, 0.04f);

        TextMeshPro tmp = txtObj.AddComponent<TextMeshPro>();
        tmp.text = initialText;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Overflow;
        return tmp;
    }

    private void ApplyHologramColor(Renderer r, Color baseColor, float emissionMultiplier)
    {
        if (r == null)
            return;

        Material mat = r.material;
        mat.color = baseColor;

        if (mat.HasProperty("_BaseColor"))
        {
            mat.SetColor("_BaseColor", baseColor);
        }

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", baseColor * emissionMultiplier);
        }
    }
}

