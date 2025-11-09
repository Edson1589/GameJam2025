using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

/// <summary>
/// FinalVideoController - Controla la reproducción del video final del juego
/// Se reproduce cuando el jugador derrota al jefe final
/// </summary>
public class FinalVideoController : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoClip finalVideoClip;
    
    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField] private float delayAfterVideo = 1f;
    
    [Header("UI (Optional)")]
    [SerializeField] private GameObject skipPrompt;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private bool allowSkip = true;
    
    [Header("Letterbox (Barras Negras)")]
    [SerializeField] private bool createLetterbox = true;
    [SerializeField] private Color letterboxColor = Color.black;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private bool videoStarted = false;
    private bool videoFinished = false;
    private VideoLetterbox letterbox;

    void Start()
    {
        // Buscar VideoPlayer si no está asignado
        if (videoPlayer == null)
        {
            videoPlayer = GetComponentInChildren<VideoPlayer>();
        }

        // Si aún no hay VideoPlayer, crear uno
        if (videoPlayer == null)
        {
            GameObject videoPlayerObj = new GameObject("VideoPlayer");
            videoPlayerObj.transform.SetParent(transform);
            videoPlayer = videoPlayerObj.AddComponent<VideoPlayer>();
        }

        // Configurar VideoPlayer (tanto si es nuevo como si ya existía)
        ConfigureVideoPlayer();

        // Asignar el clip de video
        if (finalVideoClip != null)
        {
            videoPlayer.clip = finalVideoClip;
        }
        else
        {
            // Intentar cargar el video desde Resources o desde la ruta
            LoadVideoFromPath();
        }

        // Configurar evento de finalización
        videoPlayer.loopPointReached += OnVideoEnd;

        // Ocultar prompt de skip inicialmente
        if (skipPrompt != null)
        {
            skipPrompt.SetActive(false);
        }

        // Crear barras negras (letterbox) si está habilitado
        if (createLetterbox)
        {
            CreateLetterbox();
        }

        // Iniciar reproducción
        StartVideo();
    }

    void Update()
    {
        // Verificar si el video terminó (fallback si el evento no funciona)
        if (videoStarted && !videoFinished && videoPlayer != null && videoPlayer.clip != null)
        {
            // Verificar si el video terminó comparando el tiempo
            if (videoPlayer.isPlaying && videoPlayer.time >= videoPlayer.clip.length - 0.1f)
            {
                if (showDebugLogs)
                {
                    Debug.Log("FinalVideoController: Video detectado como terminado (fallback check)");
                }
                OnVideoEnd(videoPlayer);
            }
            // Verificar si el video se detuvo inesperadamente
            else if (!videoPlayer.isPlaying && videoPlayer.time > 0.1f && videoPlayer.time < videoPlayer.clip.length - 0.1f)
            {
                if (showDebugLogs)
                {
                    Debug.LogWarning("FinalVideoController: Video se detuvo inesperadamente. Reanudando...");
                }
                videoPlayer.Play();
            }
        }

        // Permitir saltar el video
        if (allowSkip && videoStarted && !videoFinished && Input.GetKeyDown(skipKey))
        {
            SkipVideo();
        }

        // Mostrar prompt de skip después de un segundo
        if (allowSkip && videoStarted && !videoFinished && skipPrompt != null && !skipPrompt.activeSelf)
        {
            skipPrompt.SetActive(true);
        }
    }

    private void ConfigureVideoPlayer()
    {
        if (videoPlayer == null) return;

        // Configurar propiedades básicas
        videoPlayer.playOnAwake = false; // Lo controlamos manualmente
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;
        videoPlayer.isLooping = false;

        // Buscar cámara principal
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            mainCam = FindObjectOfType<Camera>();
        }

        // Configurar para reproducir en la cámara
        if (mainCam != null)
        {
            videoPlayer.targetCamera = mainCam;
            videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
            videoPlayer.targetCameraAlpha = 1f;
            
            if (showDebugLogs)
            {
                Debug.Log($"FinalVideoController: Video Player configurado para reproducir en la cámara: {mainCam.name}");
                Debug.Log($"  - Render Mode: {videoPlayer.renderMode}");
                Debug.Log($"  - Target Camera: {videoPlayer.targetCamera.name}");
            }
        }
        else
        {
            // Si no hay cámara, usar Render Texture como fallback
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            if (showDebugLogs)
            {
                Debug.LogWarning("FinalVideoController: No se encontró cámara. Usando Render Texture como fallback.");
            }
        }

        // Configurar audio
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        if (showDebugLogs)
        {
            Debug.Log($"FinalVideoController: Audio configurado - Output Mode: {videoPlayer.audioOutputMode}");
        }
    }

    private void LoadVideoFromPath()
    {
        // Intentar cargar el video desde Resources si existe
        VideoClip loadedClip = Resources.Load<VideoClip>("VideoLevels/VideoFinalJuego");
        if (loadedClip != null)
        {
            videoPlayer.clip = loadedClip;
            if (showDebugLogs)
            {
                Debug.Log("FinalVideoController: Video cargado desde Resources");
            }
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning("FinalVideoController: No se asignó VideoClip. Por favor, asigna 'VideoFinalJuego.mp4' en el inspector o colócalo en Resources/VideoLevels/");
        }
    }

    private void StartVideo()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("FinalVideoController: No hay VideoPlayer asignado!");
            Invoke("LoadNextScene", 2f);
            return;
        }

        if (videoPlayer.clip == null)
        {
            Debug.LogError("FinalVideoController: No hay VideoClip asignado!");
            Invoke("LoadNextScene", 2f);
            return;
        }

        // Verificar configuración antes de reproducir
        if (videoPlayer.renderMode == VideoRenderMode.CameraFarPlane && videoPlayer.targetCamera == null)
        {
            Debug.LogWarning("FinalVideoController: Target Camera no asignado. Intentando asignar cámara principal...");
            Camera mainCam = Camera.main ?? FindObjectOfType<Camera>();
            if (mainCam != null)
            {
                videoPlayer.targetCamera = mainCam;
            }
        }

        videoStarted = true;
        
        // Asegurar que el video esté preparado
        videoPlayer.Prepare();
        
        // Esperar un frame antes de reproducir para asegurar que todo esté configurado
        StartCoroutine(PlayVideoDelayed());
    }

    private System.Collections.IEnumerator PlayVideoDelayed()
    {
        // Esperar un frame
        yield return null;
        
        // Verificar que el video esté preparado
        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.Play();

        if (showDebugLogs)
        {
            Debug.Log("=== REPRODUCIENDO VIDEO FINAL ===");
            Debug.Log($"  - Clip: {videoPlayer.clip.name}");
            Debug.Log($"  - Duración: {videoPlayer.clip.length} segundos");
            Debug.Log($"  - Render Mode: {videoPlayer.renderMode}");
            Debug.Log($"  - Target Camera: {(videoPlayer.targetCamera != null ? videoPlayer.targetCamera.name : "Ninguna")}");
            Debug.Log($"  - Is Playing: {videoPlayer.isPlaying}");
            if (allowSkip)
            {
                Debug.Log($"  - Presiona {skipKey} para saltar");
            }
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        if (videoFinished) return;
        
        videoFinished = true;
        
        if (vp != null)
        {
            vp.Stop();
        }

        if (showDebugLogs)
        {
            Debug.Log("=== VIDEO FINAL COMPLETADO ===");
            double totalTime = vp != null ? vp.time : 0.0;
            Debug.Log($"Tiempo total reproducido: {totalTime} segundos");
        }

        // Ocultar prompt de skip
        if (skipPrompt != null)
        {
            skipPrompt.SetActive(false);
        }

        // Cargar siguiente escena después de un delay
        if (delayAfterVideo > 0)
        {
            Invoke("LoadNextScene", delayAfterVideo);
        }
        else
        {
            LoadNextScene();
        }
    }

    private void SkipVideo()
    {
        if (videoFinished) return;

        videoFinished = true;
        
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        if (showDebugLogs)
        {
            Debug.Log("=== VIDEO FINAL SALTADO ===");
        }

        // Ocultar prompt de skip
        if (skipPrompt != null)
        {
            skipPrompt.SetActive(false);
        }

        // Cargar siguiente escena inmediatamente
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (showDebugLogs)
        {
            Debug.Log($"=== CARGANDO ESCENA: {nextSceneName} ===");
        }

        // Cancelar cualquier invoke pendiente
        CancelInvoke();

        // Asegurar que el video esté detenido
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        // Cargar la escena
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("FinalVideoController: nextSceneName está vacío! No se puede cargar la escena.");
            return;
        }

        try
        {
            SceneManager.LoadScene(nextSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FinalVideoController: Error al cargar escena '{nextSceneName}': {e.Message}");
            Debug.LogError("Asegúrate de que la escena esté en Build Settings!");
        }
    }

    private void CreateLetterbox()
    {
        // Buscar si ya existe un VideoLetterbox
        letterbox = FindObjectOfType<VideoLetterbox>();
        
        if (letterbox == null)
        {
            // Crear nuevo GameObject con VideoLetterbox
            GameObject letterboxObj = new GameObject("VideoLetterbox");
            letterbox = letterboxObj.AddComponent<VideoLetterbox>();
        }

        // Configurar y crear las barras
        letterbox.SetBarColor(letterboxColor);
        letterbox.CreateLetterbox();

        if (showDebugLogs)
        {
            Debug.Log("FinalVideoController: Barras negras (letterbox) creadas");
        }
    }

    void OnDestroy()
    {
        // Limpiar eventos
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}

