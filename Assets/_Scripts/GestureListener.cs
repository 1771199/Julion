using UnityEngine;
using System.Collections;
using System;

public class GestureListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    // GUI Text to display the gesture messages.
    public GUIText GestureInfo;

    private bool tpose;
    private bool swipeLeft;
    private bool swipeRight;
    private bool psi;
    private bool jump;
    


    public bool IsTPose()
    {
        //if (StateUpdater.isRecording || StateUpdater.kinectControl)
        // tpose = false;
        if (tpose)
        {

            tpose = false;
            return true;
        }

        return false;
    }

    public bool IsSwipeRight()
    {
        
        if (swipeRight)
        {
            swipeRight = false;
            return true;
        }

        return false;
    }

    public bool IsSwipeLeft()
    {
        
        if (swipeLeft)
        {
            swipeLeft = false;
            return true;
        }

        return false;
    }

    public bool IsPsi()
    {

        if (psi)
        {
            psi = false;
            return true;
        }

        return false;
    }

    public bool IsJump()
    {

        if (jump)
        {
            jump = false;
            return true;
        }

        return false;
    }

    public void UserDetected(uint userId, int userIndex)
    {
        // detect these user specific gestures
        KinectManager manager = KinectManager.Instance;

        manager.DetectGesture(userId, KinectGestures.Gestures.Tpose);
        manager.DetectGesture(userId, KinectGestures.Gestures.Psi);
        manager.DetectGesture(userId, KinectGestures.Gestures.SwipeRight);
        manager.DetectGesture(userId, KinectGestures.Gestures.SwipeLeft);
        manager.DetectGesture(userId, KinectGestures.Gestures.Jump);

        if (GestureInfo != null)
        {
            GestureInfo.GetComponent<GUIText>().text = "Swipe left or right to change the slides.";
        }
    }

    public void UserLost(uint userId, int userIndex)
    {
        if (GestureInfo != null)
        {
            GestureInfo.GetComponent<GUIText>().text = string.Empty;
        }
    }

    public void GestureInProgress(uint userId, int userIndex, KinectGestures.Gestures gesture,
                                  float progress, KinectWrapper.NuiSkeletonPositionIndex joint, Vector3 screenPos)
    {
        // don't do anything here
    }

    public bool GestureCompleted(uint userId, int userIndex, KinectGestures.Gestures gesture,
                                  KinectWrapper.NuiSkeletonPositionIndex joint, Vector3 screenPos)
    {
        string sGestureText = gesture + " detected";
        if (GestureInfo != null)
        {
            GestureInfo.GetComponent<GUIText>().text = sGestureText;
            GestureInfo.GetComponent<GUIText>().text = null;
        }
        
        if (gesture == KinectGestures.Gestures.Tpose)
        {
            if ((StateUpdater.isPlaying || !StateUpdater.kinectControl) && StateUpdater.SongCanvas)
                tpose = false;
            else tpose = true;
            
        }

        else if (gesture == KinectGestures.Gestures.Psi)
        {
            if (StateUpdater.isPlaying || !StateUpdater.kinectControl)
                psi = false;
            else psi = true;
        }

        else if (gesture == KinectGestures.Gestures.SwipeRight)
        {
            if (StateUpdater.isPlaying || !StateUpdater.kinectControl)
                swipeRight = false;
            else swipeRight = true;
        }

        else if (gesture == KinectGestures.Gestures.SwipeLeft)
        {
            if (StateUpdater.isPlaying || !StateUpdater.kinectControl)
                swipeLeft = false;
            else swipeLeft = true;
        }
        else if (gesture == KinectGestures.Gestures.Jump)
        {
            if (StateUpdater.isPlaying || !StateUpdater.kinectControl)
                jump = false;
            else jump = true;
        }
        
        return true;
    }

    public bool GestureCancelled(uint userId, int userIndex, KinectGestures.Gestures gesture,
                                  KinectWrapper.NuiSkeletonPositionIndex joint)
    {
        // don't do anything here, just reset the gesture state
        return true;
    }

}