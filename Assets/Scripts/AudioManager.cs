using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AudioManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                }
            }
            return _instance;
        }
    }

    private AudioSource ambientSource;
    private AudioSource sfxSource;
    private AudioSource voiceSource;

    private readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
    private float[] sampleBuffer = new float[256];

    private string lastVoiceClipName = "voz_intro";
    private string lastSubtitleText = "";
    private float simulatedSpeechEndTime = 0f;

    public const string INTRO_SUBTITLE =
        "SISTEMA FORENSE A.N.D.E.S. ACTIVO. Caso Banco Andino: 36 órdenes SWIFT fraudulentas por US$ 951 millones emitidas fuera de horario hacia Manila y Colombo. Escanea cada marcador AR para auditar la trazabilidad del ataque.";

    public const string ATTACK_ANALYSIS_SUBTITLE =
        "ANÁLISIS DE ATAQUE (MODO 2016): El grupo Lazarus comprometió credenciales SWIFT y operó de noche sin límites de monto ni doble aprobación. 30 órdenes cayeron por la alerta «Jupiter», 1 por error ortográfico «Fandation» y 4 órdenes drenaron US$ 81 millones a Manila.";

    public const string DEFENSE_ACTIVE_SUBTITLE =
        "PROTOCOLO ZERO TRUST ACTIVADO: Controles de defensa en línea: 1) Doble aprobación mancomunada Operador-Autorizador, 2) Bloqueo automático por límite de US$ 10 millones y horario nocturno, y 3) Lista blanca estricta de beneficiarios SWIFT. Las 36 órdenes (US$ 951 M) han sido retenidas.";

    public bool IsVoiceMuted => voiceSource != null && voiceSource.mute;

    public bool IsVoicePlaying
    {
        get
        {
            bool sourcePlaying = voiceSource != null && voiceSource.isPlaying && !voiceSource.mute;
            bool simulatedPlaying = Time.unscaledTime < simulatedSpeechEndTime && !IsVoiceMuted;
            return sourcePlaying || simulatedPlaying;
        }
    }

    public float CurrentVoiceAmplitude
    {
        get
        {
            if (!IsVoicePlaying)
                return 0f;

            if (voiceSource != null && voiceSource.isPlaying && !voiceSource.mute)
            {
                voiceSource.GetOutputData(sampleBuffer, 0);
                float sum = 0f;
                for (int i = 0; i < sampleBuffer.Length; i++)
                {
                    float s = sampleBuffer[i];
                    sum += s * s;
                }
                float rms = Mathf.Sqrt(sum / sampleBuffer.Length);
                float normalized = Mathf.Clamp01(rms * 6.5f);
                if (normalized > 0.01f)
                    return normalized;
            }

            // Onda simulada de respaldo si el audio está en curso o silenciado por hardware
            float t = Time.unscaledTime * 12f;
            float wave = (Mathf.Sin(t) * 0.35f) + (Mathf.Sin(t * 2.3f) * 0.25f) + 0.45f;
            return Mathf.Clamp01(wave);
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        InitializeAudioSources();
    }

    void Start()
    {
        EnsureHUDExists();
        StartAmbientLoop();
        StartCoroutine(PlayIntroDelayed(1.0f));
    }

    private void InitializeAudioSources()
    {
        if (ambientSource == null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.volume = 0.25f;
            ambientSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.volume = 0.7f;
            sfxSource.playOnAwake = false;
        }

        if (voiceSource == null)
        {
            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.loop = false;
            voiceSource.volume = 1.0f;
            voiceSource.playOnAwake = false;
        }
    }

    public void EnsureHUDExists()
    {
        if (FindFirstObjectByType<VoiceAssistantHUD>() == null)
        {
            GameObject hudObj = new GameObject("VoiceAssistantHUD");
            hudObj.AddComponent<VoiceAssistantHUD>();
        }
    }

    private AudioClip LoadClipSafe(string clipName)
    {
        if (string.IsNullOrEmpty(clipName))
            return null;

        if (clipCache.TryGetValue(clipName, out AudioClip cachedClip) && cachedClip != null)
            return cachedClip;

        AudioClip clip = Resources.Load<AudioClip>("Audio/" + clipName);
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] No se encontró el clip 'Resources/Audio/{clipName}'. Continuando sin excepción.");
            return null;
        }

        clipCache[clipName] = clip;
        return clip;
    }

    private void StartAmbientLoop()
    {
        InitializeAudioSources();
        AudioClip ambientClip = LoadClipSafe("sfx_ambiente");
        if (ambientClip != null)
        {
            ambientSource.clip = ambientClip;
            ambientSource.loop = true;
            ambientSource.volume = 0.25f;
            ambientSource.Play();
        }
    }

    private IEnumerator PlayIntroDelayed(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        PlayVoice("voz_intro", INTRO_SUBTITLE);
    }

    public void PlayVoice(string clipName, string subtitleText)
    {
        InitializeAudioSources();
        EnsureHUDExists();

        lastVoiceClipName = clipName;
        lastSubtitleText = subtitleText;

        if (VoiceAssistantHUD.Instance != null && !string.IsNullOrEmpty(subtitleText))
        {
            VoiceAssistantHUD.Instance.DisplaySubtitle(subtitleText);
        }

        AudioClip clip = LoadClipSafe(clipName);
        if (clip != null)
        {
            voiceSource.Stop();
            voiceSource.clip = clip;
            voiceSource.volume = 1.0f;
            voiceSource.Play();
            simulatedSpeechEndTime = 0f;
        }
        else
        {
            // Si el archivo de audio no existe, simulamos actividad de onda durante la lectura del subtítulo
            float duration = Mathf.Clamp((subtitleText != null ? subtitleText.Length : 60) * 0.04f, 2.5f, 10f);
            simulatedSpeechEndTime = Time.unscaledTime + duration;
        }
    }

    public void PlaySFX(string sfxName)
    {
        InitializeAudioSources();
        AudioClip clip = LoadClipSafe(sfxName);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, 0.7f);
        }
    }

    public void ToggleMuteVoice()
    {
        InitializeAudioSources();
        voiceSource.mute = !voiceSource.mute;

        if (VoiceAssistantHUD.Instance != null)
        {
            VoiceAssistantHUD.Instance.UpdateMuteButtonVisual(voiceSource.mute);
        }
    }

    public void ReplayLastVoice()
    {
        if (string.IsNullOrEmpty(lastVoiceClipName))
        {
            PlayVoice("voz_intro", INTRO_SUBTITLE);
            return;
        }

        PlayVoice(lastVoiceClipName, string.IsNullOrEmpty(lastSubtitleText) ? INTRO_SUBTITLE : lastSubtitleText);
    }
}

