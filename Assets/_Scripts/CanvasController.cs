using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasController : MonoBehaviour
{
    public GameObject[] Canvases;
    RecordManager recordManager = new RecordManager();
    

    void Start()
    {
        GoMainCanvas();
    }
    

    public void GoMainCanvas()
    {
        for (int i = 0; i < Canvases.Length; i++)
        {
            Canvases[i].SetActive(false);
        }
        Canvases[0].SetActive(true);
        recordManager.ResetbyCanvas();
        StateUpdater.MainCanvas = true;
        StateUpdater.SongCanvas = false;
    }
    

    public void GoSongCanvas()
    {
        for (int i = 0; i < Canvases.Length; i++)
        {
            Canvases[i].SetActive(false);
        }
        Canvases[1].SetActive(true);
        recordManager.ResetbyCanvas();
        StateUpdater.MainCanvas = false;
        StateUpdater.SongCanvas = true;
    }
    
    
    
    
}
