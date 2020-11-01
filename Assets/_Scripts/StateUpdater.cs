using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class StateUpdater
{
    public static bool isConnectingKinect = false;  //키넥트가 연결되어 있는지 확인
    public static bool isPlaying = false;
    public static bool isRecording = false;
    public static bool isPlayDone = false;
    public static bool isVoiceLoad = false;
    public static bool kinectControl = true;
    public static bool MainCanvas = false;
    public static bool SongCanvas = false;
    public static bool PlaySong2 = false;
    public static bool PlaySong3 = false;
}
