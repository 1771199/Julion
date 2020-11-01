using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class RecordManager : MonoBehaviour
{
    LionController lionController = new LionController();
    AvatarController avatarController = new AvatarController();
    public Slider motionspeed;
    public Slider mainSlider;
    public MicManager micManager;
    public AudioSource audioSource;
    public GestureListener gestureListener;
    public AudioSource dividedSoundManager;
    public AudioSource wholeSoundManager;
    public GameObject lion, lion2, lion3;
    public RuntimeAnimatorController dancing;
    static JointAngleArray jointAngleArray;
    public Image Main_RecordImage;
    public Image Main_PlayImage;
    public Image Song_RecordImage;
    public Image Song_PlayImage;
    public Text PsiShyText;
    public GameObject BigText;
    public GameObject SmallText;
    

    JointAngleArray smallJointAngleArray, bigJointAngleArray,  psiJointAngleArray;
    Transform neck;
    Transform[] bodyandNeck;
    Transform[] lionBone, lion2Bone, lion3Bone;
    float fps = 10f;
    float recordTime = 0f;
    bool TposeCommand, CustomedMotion = false;
    bool music0, music1_1, music1_2, music2_1, music2_2, music3, music4 = false;
    bool motionLionRunning = false;
    Vector3 beforeAngle, gapAngle, presentAngle, neckVector, beforeVector = Vector3.zero;
    int axis = 0;
    int speedvalue = 0;
    float motionRange = 1.0f;
    Color red = new Color(230, 0, 0);
    Color white = new Color(255, 255, 255);



    private void Start()
    {
        recordTime = 1 / fps;
        neck = AvatarController.GetNeck;
        bodyandNeck = AvatarController.GetBones;
        lionBone = LionController.GetLionBones;
        lion2Bone = LionController2.GetLion2Bones;
        lion3Bone = LionController3.GetLion3Bones;
        avatarController.Add(ref bodyandNeck, neck);
        

        lion.GetComponent<Animator>().enabled = false;
        lion2.GetComponent<Animator>().enabled = false;
        lion3.GetComponent<Animator>().enabled = false;
        lion2.SetActive(false);
        lion3.SetActive(false);

    }

    private void Update()
    {
        if (StateUpdater.MainCanvas)
        {
            if (gestureListener)
            {

                if (!StateUpdater.isRecording)
                {
                    if (gestureListener.IsTPose())
                    {
                        recordTime = 0.1f;
                        mainSlider.value = 0.0f;
                        micManager.VoiceController(mainSlider.value);
                        TposeCommand = true;
                        TposeRecordStart();
                        ControlImageColor();

                    }
                }
                else
                {
                    if (gestureListener.IsTPose())
                    {
                        StopRecord();
                        PlayLion();
                        ControlImageColor();
                    }
                }

                if (LionController.EditJointAngleArray != null)
                {
                    if (gestureListener.IsPsi())
                    {
                        psiJointAngleArray = new JointAngleArray();
                        recordTime = 0.1f;
                        motionRange = 1.7f;
                        psiJointAngleArray = EditMotion(motionRange);
                        StartCoroutine(MoveLion(psiJointAngleArray, bodyandNeck));
                        micManager.ClickedVoicePlay();
                        PsiShyText.text = "힘차게!";
                    }

                    if (gestureListener.IsSwipeRight())
                    {
                        speedvalue += 1;
                        if (speedvalue > 1)
                            speedvalue = -1;
                        mainSlider.value = speedvalue;
                        switch (mainSlider.value)
                        {
                            case 0:
                                recordTime = 0.1f;
                                break;
                            case 1:
                                recordTime = 0.06f;
                                break;
                            case -1:
                                recordTime = 0.13f;
                                break;
                        }
                        micManager.VoiceController(mainSlider.value);
                        micManager.ClickedVoicePlay();
                        StartCoroutine(MoveLion(jointAngleArray, bodyandNeck));

                    }

                    if (gestureListener.IsSwipeLeft())
                    {
                        speedvalue -= 1;
                        if (speedvalue < -1)
                        {
                            speedvalue = 1;
                        }
                        mainSlider.value = speedvalue;


                        switch (mainSlider.value)
                        {
                            case 0:
                                recordTime = 0.1f;
                                break;
                            case 1:
                                recordTime = 0.05f;
                                break;
                            case -1:
                                recordTime = 0.12f;
                                break;
                        }
                        micManager.VoiceController(mainSlider.value);
                        micManager.ClickedVoicePlay();
                        StartCoroutine(MoveLion(jointAngleArray, bodyandNeck));
                    }
                    if (gestureListener.IsJump())
                    {
                        PlayLion();
                        ControlImageColor();
                    }

                }
            
        }


            neckVector = neck.localRotation.eulerAngles;
            LimitAngle(ref neckVector);

            if (StateUpdater.kinectControl)
            {
                if (neckVector != beforeVector)
                {
                    if ((neckVector.x < -10.0f && neckVector.z < -20.0f) || (neckVector.x < -10.0f && neckVector.z > 20.0f))
                    {
                        beforeVector = neckVector;
                        if (!StateUpdater.isPlaying && StateUpdater.kinectControl && jointAngleArray != null)
                        {
                            neckVector = Vector3.zero;

                            JointAngleArray psiJointAngleArray = new JointAngleArray();
                            recordTime = 0.1f;
                            motionRange = 0.2f;
                            psiJointAngleArray = EditMotion(motionRange);
                            StartCoroutine(MoveLion(psiJointAngleArray, bodyandNeck));
                            micManager.VoiceController(motionspeed.value);
                            micManager.ClickedVoicePlay();
                            PsiShyText.text = "소심하게...";
                        }
                    }
                }

            }
        }
        //-------------------------------------------------------------------------------------------------
        if (StateUpdater.SongCanvas)
        {
            if (gestureListener)
            {
                if (!StateUpdater.isPlaying)
                {
                    if (gestureListener.IsTPose())
                    {
                        recordTime = 0.1f;
                        motionspeed.value = 0.0f;
                        micManager.VoiceController(motionspeed.value);
                        TposeCommand = true;
                        SoundandMotionControl();
                    }

                    if (LionController.EditJointAngleArray != null)
                    {

                        if (gestureListener.IsSwipeRight())
                        {
                            speedvalue += 1;
                            if (speedvalue > 1)
                                speedvalue = -1;
                            motionspeed.value = speedvalue;
                            switch (motionspeed.value)
                            {
                                case 0:
                                    recordTime = 0.1f;
                                    break;
                                case 1:
                                    recordTime = 0.06f;
                                    break;
                                case -1:
                                    recordTime = 0.13f;
                                    break;
                            }
                            micManager.VoiceController(motionspeed.value);
                            SoundandMotionControl();
                        }

                        if (gestureListener.IsSwipeLeft())
                        {
                            speedvalue -= 1;
                            if (speedvalue < -1)
                            {
                                speedvalue = 1;
                            }
                            motionspeed.value = speedvalue;


                            switch (motionspeed.value)
                            {
                                case 0:
                                    recordTime = 0.1f;
                                    break;
                                case 1:
                                    recordTime = 0.05f;
                                    break;
                                case -1:
                                    recordTime = 0.12f;
                                    break;
                            }
                            micManager.VoiceController(motionspeed.value);
                            SoundandMotionControl();
                        }

                    }
                }
            }
            neckVector = neck.localRotation.eulerAngles;
            LimitAngle(ref neckVector);

        }

    }

    public static JointAngleArray EditJointAngleArray
    {
        get { return jointAngleArray; }
        set { jointAngleArray = value; }
    }
    

    void TposeRecordStart()
    {
        LionController.NewJointAngleArray();
        StateUpdater.isRecording = true;
        micManager.ClickedVoiceStart();
    }

    void TposeRecordStop()
    {
        StateUpdater.isRecording = false;
    }

    void SoundandMotionControl()
    {
        music0 = true;
        StateUpdater.isPlaying = true;

        dividedSoundManager.clip = Resources.Load<AudioClip>("나처럼해봐요0");
        dividedSoundManager.Play();
        wholeSoundManager.clip = Resources.Load<AudioClip>("나처럼해봐요전체");
        wholeSoundManager.Play();

        StartCoroutine("BackGroundSound");
    }

    IEnumerator BackGroundSound()
    {
        do
        {
            if (!dividedSoundManager.isPlaying)
            {
                if (music0)
                {
                    PlayDoneIntro();
                    dividedSoundManager.clip = Resources.Load<AudioClip>("나처럼해봐요1-1");
                    dividedSoundManager.Play();
                    music0 = false;
                    music1_1 = true;
                    ControlImageColor();
                   

                }
                else if (music1_1)
                {
                    dividedSoundManager.clip = Resources.Load<AudioClip>("나처럼해봐요1-2");
                    dividedSoundManager.Play();
                    music1_1 = false;
                    music1_2 = true;
                    StopRecord();
                    smallJointAngleArray = new JointAngleArray();
                    smallJointAngleArray = EditMotion(0.2f);
                    bigJointAngleArray = new JointAngleArray();
                    bigJointAngleArray = EditMotion(1.8f);
                    micManager.ClickedVoicePlay();
                    StateUpdater.PlaySong2 = true;

                    

                }
                else if (music1_2)
                {
                    SmallText.SetActive(true);
                    lion2.SetActive(true);
                    dividedSoundManager.clip = Resources.Load<AudioClip>("나처럼해봐요2-11");
                    dividedSoundManager.Play();
                    music1_2 = false;
                    music2_1 = true;
                    PlayMotionFileForSong2();
                    ControlImageColor();
                }

                else if (music2_1)
                {
                    StateUpdater.PlaySong2 = false;
                    dividedSoundManager.clip = Resources.Load<AudioClip>("나처럼해봐요2-10");
                    dividedSoundManager.Play();
                    music2_1 = false;
                    music2_2 = true;
                    micManager.ClickedVoicePlay();
                    StateUpdater.PlaySong3 = true;
                }

                else if (music2_2)
                {
                    BigText.SetActive(true);
                    lion3.SetActive(true);
                    dividedSoundManager.clip = Resources.Load<AudioClip>("나처럼해봐요3");
                    dividedSoundManager.Play();
                    music2_2 = false;
                    music3 = true;
                    PlayMotionFileForSong3();

                }
                else if (music3)
                {

                    StateUpdater.PlaySong3 = false;
                    lion.GetComponent<Animator>().enabled = true;
                    lion2.GetComponent<Animator>().enabled = true;
                    lion3.GetComponent<Animator>().enabled = true;
                    lion.GetComponent<Animator>().runtimeAnimatorController = dancing;
                    lion2.GetComponent<Animator>().runtimeAnimatorController = dancing;
                    lion3.GetComponent<Animator>().runtimeAnimatorController = dancing;
                    dividedSoundManager.clip = Resources.Load<AudioClip>("나처럼해봐요4");
                    dividedSoundManager.Play();
                    music3 = false;
                    music4 = true;
                    //LionController.EditJointAngleArray.Clear();
                }

                else if (music4)
                {
                    LerpAfterAnim();
                    lion.GetComponent<Animator>().enabled = false;
                    lion2.GetComponent<Animator>().enabled = false;
                    lion3.GetComponent<Animator>().enabled = false;
                    lion.GetComponent<Animator>().runtimeAnimatorController = null;
                    lion2.GetComponent<Animator>().runtimeAnimatorController = null;
                    lion3.GetComponent<Animator>().runtimeAnimatorController = null;
                    lion2.SetActive(false);
                    lion3.SetActive(false);
                    BigText.SetActive(false);
                    SmallText.SetActive(false);
                    
                    
                    music4 = false;
                    StateUpdater.isPlaying = false;
                    TposeCommand = false;
                    CustomedMotion = false;
                    StateUpdater.kinectControl = true;
                    ControlImageColor();



                }
            }
            yield return null;
        } while (StateUpdater.isPlaying);


    }

    void PlayDoneIntro()
    {
        if (!TposeCommand)
        {
            CustomedMotion = true;
            StateUpdater.kinectControl = false;
            PlayMotionFileForLion();
            micManager.ClickedVoicePlay();

        }
        else
        {
            micManager.ClickedVoiceStart();
            ClickedRecordStart();
            StateUpdater.isRecording = true;
        }
    }

    void StopRecord()
    {
        if (TposeCommand)
        {
            
            StateUpdater.isRecording = false;
            AddZeroPosInJAA();
            if(StateUpdater.SongCanvas)
                audioSource.Stop();

        }
       

    }

    void PlayLion()
    {
        PlayMotionFileForLion();
        micManager.ClickedVoicePlay();
    }

    public void ResetbyCanvas()
    {
        LionController.NewJointAngleArray();
        recordTime = 0.1f;
    }



    public void ClickedRecordStart()
    {
        LionController.NewJointAngleArray();
        StateUpdater.isPlaying = true;

    }

    public void AddZeroPosInJAA()
    {
        if (StateUpdater.isPlaying)
        {
            lionController.SetZeroPose();
        }

    }

    public void PlayMotionFileForLion()
    {

        if (StateUpdater.isPlaying && motionLionRunning)
        {
            StopCoroutine("MoveLion");
            motionLionRunning = false;
        }
        jointAngleArray = LionController.EditJointAngleArray;

        StartCoroutine(MoveLion(jointAngleArray, bodyandNeck));
    }

    IEnumerator MoveLion(JointAngleArray jointAngleArray, Transform[] bodyandNeck)
    {

        if (StateUpdater.MainCanvas)
        {
            StateUpdater.kinectControl = false;
        }

        motionLionRunning = true;
        for (int i = 0; i < jointAngleArray.Length - 1; i++)
        {

            for (int j = 0; j < bodyandNeck.Length; j++)
            {

                StartCoroutine(SetQuatLerp(jointAngleArray[i][j], jointAngleArray[i + 1][j], bodyandNeck[j], recordTime));
            }


            yield return new WaitForSeconds(recordTime);

        }

        if (StateUpdater.MainCanvas)
        {
            Main_PlayImage.color = white;
            StateUpdater.kinectControl = true;
            PsiShyText.text = "";
        }
           
        motionLionRunning = false;
    }

    public IEnumerator SetQuatLerp(Quaternion startQuat, Quaternion targetQuat, Transform bodyandNeck, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime <= duration)
        {
            elapsedTime += Time.deltaTime;
            float normalTime = elapsedTime / duration;
            normalTime = float.IsInfinity(normalTime) ? 0f : normalTime;
            Quaternion currentQuat = Quaternion.Lerp(startQuat, targetQuat, elapsedTime / duration);
            bodyandNeck.localRotation = currentQuat;

            yield return null;
        }
    }

    void ControlImageColor()
    {
        
            if (StateUpdater.MainCanvas)
            {
                if (StateUpdater.isRecording)
                {
                    Main_RecordImage.color = red;
                    Main_PlayImage.color = white;
                }
                else
                {
                    Main_RecordImage.color = white;
                    Main_PlayImage.color = red;
                }
            }
        if (StateUpdater.isPlaying)
        {    
            if (StateUpdater.SongCanvas)
            {

                if (StateUpdater.isRecording)
                {
                    Song_RecordImage.color = red;
                    Song_PlayImage.color = white;
                }
                else
                {
                    Song_RecordImage.color = white;
                    Song_PlayImage.color = red;
                }

            }
        }

        else
        {
            
            if (StateUpdater.SongCanvas)
            {
                Song_RecordImage.color = white;
                Song_PlayImage.color = white;
            }
        }
    }

    void CreateMotionFile(string fileName, JointAngleArray jointAngleArray)
    {
        string motionToJson = JsonUtility.ToJson(jointAngleArray, true);
        File.WriteAllText(fileName, motionToJson);
        jointAngleArray = null;
        Debug.Log("create success");
    }

    Vector3 MinMaxAngle(Vector3 vector, int index)
    {
        switch (index)
        {
            case 1:
                Mathf.Clamp(vector.x, -90.0f, 40.0f);
                Mathf.Clamp(vector.y, 40.0f, 160.0f);
                Mathf.Clamp(vector.z, -90.0f, 90.0f);
                break;
            case 2:
                Mathf.Clamp(vector.x,-90.0f,0.0f);
                vector.y = 0.0f;
                vector.z = 0.0f;
                break;
            case 3:
                Mathf.Clamp(vector.x, -40.0f, 40.0f);
                Mathf.Clamp(vector.y, -60.0f, 60.0f);
                Mathf.Clamp(vector.y, -30.0f, 30.0f);
                break;
            case 4:
                Mathf.Clamp(vector.x, -90.0f, 40.0f);
                Mathf.Clamp(vector.y,-160.0f,-40.0f);
                Mathf.Clamp(vector.y, -90.0f, 90.0f);
                break;
            case 5:
                Mathf.Clamp(vector.x, -90.0f, 0.0f);
                vector.y = 0.0f;
                vector.z = 0.0f;
                break;
            case 6:
                Mathf.Clamp(vector.x, -40.0f, 40.0f);
                Mathf.Clamp(vector.y, -60.0f, 60.0f);
                Mathf.Clamp(vector.y, -30.0f, 30.0f);
                break;
        }
        return vector;
    }
    
    
    
    public JointAngleArray EditMotion(float motionRange)
    {
        JointAngleArray psiJointAngleArray = new JointAngleArray();
        jointAngleArray = LionController.EditJointAngleArray;

        for (int i = 0; i < jointAngleArray.Length; i++)
        {
            JointAngle jointAngle = new JointAngle();
            for (int j = 0; j < jointAngleArray[i].Length; j++)
            {

                if (i == 0)
                {
                    presentAngle = jointAngleArray[i][j].eulerAngles;
                    LimitAngle(ref presentAngle);
                    presentAngle = ChangeFirstMotion(presentAngle, j);
                    presentAngle = MinMaxAngle(presentAngle,j);
                    jointAngle.Add(Quaternion.Euler(presentAngle));
                }
                else
                {
                    Vector3 beforeAngle = jointAngleArray[i - 1][j].eulerAngles;

                    LimitAngle(ref beforeAngle);
                    Vector3 presentAngle = jointAngleArray[i][j].eulerAngles;
                    LimitAngle(ref presentAngle);
                    Vector3 gapAngle = presentAngle - beforeAngle;
                    int axis = CompareAngle(gapAngle);
                    switch (axis)
                    {
                        case 0:
                            presentAngle.x *= motionRange;
                            break;
                        case 1:
                            if (j == 1)
                            {
                                presentAngle.y -= 90.0f;
                                presentAngle.y *= motionRange;
                                presentAngle.y += 90.0f;

                            }
                            else if (j == 4)
                            {
                                presentAngle.y += 90.0f;
                                presentAngle.y *= motionRange;
                                presentAngle.y -= 90.0f;
                            }
                            else
                                presentAngle.y *= motionRange;
                            break;
                        case 2:
                            presentAngle.z *= motionRange;
                            break;

                    }
                    jointAngle.Add(Quaternion.Euler(presentAngle));
                }

            }
            psiJointAngleArray.Add(jointAngle);
        }
        return psiJointAngleArray;
    }


    private void LimitAngle(ref Vector3 eulerangle)
    {
        if (eulerangle.x > 180.0f)
            eulerangle.x -= 360.0f;
        else if (eulerangle.x < -180.0f)
            eulerangle.x += 360.0f;
        if (eulerangle.y > 180.0f)
            eulerangle.y -= 360.0f;
        else if (eulerangle.y < -180.0f)
            eulerangle.y += 360.0f;
        if (eulerangle.z > 180.0f)
            eulerangle.z -= 360.0f;
        else if (eulerangle.z < -180.0f)
            eulerangle.z += 360.0f;
    }

    private int CompareAngle(Vector3 gapAngle)
    {
        float absX = Mathf.Abs(gapAngle.x);
        float absY = Mathf.Abs(gapAngle.y);
        float absZ = Mathf.Abs(gapAngle.z);

        int axis = absX < absY ? (absY < absZ ? 2 : 1) : (absX < absZ ? 2 : 0);
        return axis;
    }

    private Vector3 ChangeFirstMotion(Vector3 presentAngle, int index)
    {
        switch (index)
        {
            case 1:
                beforeAngle = new Vector3(0.0f, 90.0f, 0.0f);
                gapAngle = presentAngle - beforeAngle;
                axis = CompareAngle(gapAngle);
                presentAngle.y -= 90.0f;
                ChangeCustomizeAngle(ref presentAngle, axis);
                presentAngle.y += 90.0f;
                break;
            case 0:
            case 2:
            case 3:
            case 6:
            case 5:
            case 7:
                beforeAngle = new Vector3(0.0f, 0.0f, 0.0f);
                gapAngle = presentAngle - beforeAngle;
                axis = CompareAngle(gapAngle);
                ChangeCustomizeAngle(ref presentAngle, axis);
                break;
            case 4:
                beforeAngle = new Vector3(0.0f, -90.0f, 0.0f);
                gapAngle = presentAngle - beforeAngle;
                axis = CompareAngle(gapAngle);
                presentAngle.y += 90.0f;
                ChangeCustomizeAngle(ref presentAngle, axis);
                presentAngle.y -= 90.0f;
                break;
        }
        return presentAngle;
    }



    private void ChangeCustomizeAngle(ref Vector3 presentAngle, int axis)
    {
        switch (axis)
        {
            case 0:
                presentAngle.x *= 1.5f;
                break;
            case 1:
                presentAngle.y *= 1.5f;
                break;
            case 2:
                presentAngle.z *= 1.5f;
                break;

        }
    }

    int LimitMotionSpeed(int motionSpeed)
    {
        return Mathf.Clamp(motionSpeed, -2, 2);
    }


    


    void PlayMotionFileForSong2()
    {
        
        jointAngleArray = LionController.EditJointAngleArray;
        StartCoroutine(MoveLion(jointAngleArray, lionBone));
        StartCoroutine(MoveLion(smallJointAngleArray, bodyandNeck));
    }

    void PlayMotionFileForSong3()
    {
        
        jointAngleArray = LionController.EditJointAngleArray;
        StartCoroutine(MoveLion(bigJointAngleArray, bodyandNeck));
        StartCoroutine(MoveLion(jointAngleArray, lionBone));
        StartCoroutine(MoveLion(smallJointAngleArray, lion2Bone));

    }

    private void LerpAfterAnim()
    {
        for (int i = 0; i < 8; i++)
        {
            Vector3 vector = new Vector3();
            switch (i)
            {
                case 0:
                case 2:
                case 3:
                case 5:
                case 6:
                case 7:
                    vector = new Vector3(0, 0, 0);
                    StartCoroutine(SetQuatLerp(lionBone[i].localRotation, Quaternion.Euler(vector), lionBone[i], 0.5f));
                    break;
                case 1:
                    vector = new Vector3(0, 90, 0);
                    StartCoroutine(SetQuatLerp(lionBone[i].localRotation, Quaternion.Euler(vector), lionBone[i], 0.5f));
                    break;
                case 4:
                    vector = new Vector3(0, -90, 0);
                    StartCoroutine(SetQuatLerp(lionBone[i].localRotation, Quaternion.Euler(vector), lionBone[i], 0.5f));
                    break;
            }

        }
    }



}