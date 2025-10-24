using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class videoController : MonoBehaviour
{
    
    public VideoPlayer videoPlayer; // Arrastra aqu� el VideoPlayer de VideoManager en el inspector

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd; // Evento cuando termina el video
            videoPlayer.Play();
        }
        else
        {
            Debug.LogWarning("No se asign� VideoPlayer en VideoController");
        }
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("El video termin�.");

        vp.Stop(); // Detener para que no se repita

        // Opcional: hacer algo m�s, como desactivar VideoManager o cargar otra escena
        // videoPlayer.gameObject.SetActive(false);
    }
}


