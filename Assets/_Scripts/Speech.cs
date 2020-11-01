using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.CognitiveServices.Speech;


public class Speech : MonoBehaviour
{
    public RecordManager recordManager;
    public CanvasController canvasController;
    public OVRLipSyncContext ovrLipSyncContext;

    public Text OuputText;
    //public Button startRecoButton;
    

    private object sttThreadLocker = new object();
    private bool waitingForReco;
    private bool toggle;
    private string message;
    private string togglemessage;
    private bool micPermissionGranted = false;

    public InputField inputField;
    public Button speakButton;
    public AudioSource audioSource;

    private object ttsThreadLocker = new object();
    private bool waitingForSpeak;
    private AudioSource lipsyncAudio;
    private int micFrequency = 48000;
    public float MicFrequency
    {
        get { return micFrequency; }
        set { micFrequency = (int)Mathf.Clamp((float)value, 0, 96000); }
    }

    void Start()
    {
        if (OuputText == null)
        {
            UnityEngine.Debug.LogError("outputText property is null! Assign a UI Text element to it.");
        }
        //else if (startRecoButton == null)
        //{
        //    message = "startRecoButton property is null! Assign a UI Button to it.";
        //    UnityEngine.Debug.LogError(message);
        //}
        else
        {
            micPermissionGranted = true;
            //startRecoButton.onClick.AddListener(SttButtonClick);
        }

        if (inputField == null)
        {
            message = "inputField property is null! Assign a UI InputField element to it.";
            UnityEngine.Debug.LogError(message);
        }
        else if (speakButton == null)
        {
            message = "speakButton property is null! Assign a UI Button to it.";
            UnityEngine.Debug.LogError(message);
        }
        else
        {
            speakButton.onClick.AddListener(TtsButtonClick);
        }


        canvasController = GameObject.Find("CanvasController").GetComponent<CanvasController>();
    }

    void Update()
    {
        lock (sttThreadLocker)
        {
            //if (startRecoButton != null)
            //{
            //    startRecoButton.interactable = !waitingForReco && micPermissionGranted;
            //}
            if (OuputText != null)
            {
                OuputText.text = message;
            }
        }

        lock (ttsThreadLocker)
        {
            if (speakButton != null)
            {
                speakButton.interactable = !waitingForSpeak;
            }


        }


    }

    public void StopRecord()
    {
        waitingForReco = false;
    }

    public async void SttButtonClick()
    {
        var config = SpeechConfig.FromSubscription("34b6d5dc95c2429e90f93e94462a19d8", "westus");
        config.SpeechRecognitionLanguage = "ko-KR";

        using (var recognizer = new SpeechRecognizer(config))
        {

            lock (sttThreadLocker)
            {
                waitingForReco = true;
                toggle = true;
            }

            var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(waitingForReco);
            string newMessage = string.Empty;



            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                newMessage = result.Text;
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                newMessage = "NOMATCH: Speech could not be recognized.";
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
                newMessage = $"CANCELED: Reason={cancellation.Reason} ErrorDetails={cancellation.ErrorDetails}";
            }

            lock (sttThreadLocker)
            {
                message = newMessage;
                waitingForReco = false;
            }

            // await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
        }

    }

    public void TtsButtonClick()
    {
        var config = SpeechConfig.FromSubscription("34b6d5dc95c2429e90f93e94462a19d8", "westus");
        config.SpeechSynthesisLanguage = "ko-KR";
        lipsyncAudio = GameObject.Find("InputType_Microphone").GetComponent<AudioSource>();
        lipsyncAudio.loop = false;
        using (var synthsizer = new SpeechSynthesizer(config, null))
        {
            lock (ttsThreadLocker)
            {
                waitingForSpeak = true;
            }

            if (toggle)
            {
                togglemessage = message;

            }
            if (!toggle)
            {
                
                togglemessage = inputField.text;
            }

            var result = synthsizer.SpeakTextAsync(togglemessage).Result;





            string newMessage = string.Empty;
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                var sampleCount = result.AudioData.Length / 2;
                var audioData = new float[sampleCount];
                for (var i = 0; i < sampleCount; ++i)
                {
                    audioData[i] = (short)(result.AudioData[i * 2 + 1] << 8 | result.AudioData[i * 2]) / 32768.0F;
                }

                var audioClip = AudioClip.Create("SynthesizedAudio", sampleCount, 1, 16000, false);
                audioClip.SetData(audioData, 0);
                audioSource.clip = audioClip;
                audioSource.Play();

                
                    lipsyncAudio.clip = audioClip;
                    lipsyncAudio.Play();
                   
                

            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                newMessage = $"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?";
            }

            lock (sttThreadLocker)
            {
                message = newMessage;
                waitingForSpeak = false;
                toggle = false;
               lipsyncAudio.loop = false;
            }
        }
    }
    //public void SetMicroPhone()
    //{
    //    if (StateUpdater.isPlayDone)
    //    {
    //        lipsyncAudio.loop = true;
    //        ovrLipSyncContext.audioLoopback = false;
    //        //ovrLipSyncContext.ToggleAudioLoopback();
    //        string selectedDevice = Microphone.devices[0].ToString();
    //        lipsyncAudio.clip = Microphone.Start(selectedDevice, true, 1, micFrequency);
    //        lipsyncAudio.Play();

    //    }

    //}


}