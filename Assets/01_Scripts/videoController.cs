using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class videoController : MonoBehaviour
{
    
    public VideoPlayer videoPlayer; // Arrastra aquí el VideoPlayer de VideoManager en el inspector

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd; // Evento cuando termina el video
            videoPlayer.Play();
        }
        else
        {
            Debug.LogWarning("No se asignó VideoPlayer en VideoController");
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("El video terminó.");

        vp.Stop(); // Detener para que no se repita

        // Opcional: hacer algo más, como desactivar VideoManager o cargar otra escena
        // videoPlayer.gameObject.SetActive(false);
    }
}


