using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MicManager : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioSource lipsyncSource;
    public InputField audiofilename;
    private int micFrequency = 48000;
    float speed = 1.0f;
    AudioMixerGroup VoiceMixer;
    AudioMixerGroup BackGroundMixer;
    AudioMixerGroup LipsyncMixer;
    AudioMixerGroup SongMixer;

    public float MicFrequency
    {
        get { return micFrequency; }
        set { micFrequency = (int)Mathf.Clamp((float)value, 0, 96000); }
    }
    // Start is called before the first frame update
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        VoiceMixer = Resources.Load<AudioMixerGroup>("VoiceMixer");
        BackGroundMixer = Resources.Load<AudioMixerGroup>("BackGroundMixer");
        LipsyncMixer = Resources.Load<AudioMixerGroup>("LipsyncMixer");
        SongMixer = Resources.Load<AudioMixerGroup>("SongMixer");
    }

    public void ClickedVoiceStart()
    {
        audioSource.loop = true;
        lipsyncSource.loop = true;
        if(StateUpdater.SongCanvas)
            audioSource.clip = Microphone.Start(Microphone.devices[0], true, 4, 44100);
        if(StateUpdater.MainCanvas)
            audioSource.clip = Microphone.Start(Microphone.devices[0], true, 10, 44100);
        lipsyncSource.clip = audioSource.clip;
        lipsyncSource.Play();
    }
    public void VoiceStop()
    {
        Microphone.End(Microphone.devices[0]);
    }

    public void ClickedVoicePlay()
    {
        Microphone.End(Microphone.devices[2]);
        //audioSource.pitch = 1.5f;
        //lipsyncSource.pitch = 1.5f;
        lipsyncSource.clip = audioSource.clip;
        lipsyncSource.loop = false;
        audioSource.loop = false;
        audioSource.Play();
        lipsyncSource.Play();
        StateUpdater.isVoiceLoad = false;
    }

    public void ClickedVoiceLoad()
    {
        StateUpdater.isVoiceLoad = true;
        ClickedVoicePlay();
    }
    

    public static bool Save(string filename, AudioClip clip)
    {
        if (!filename.ToLower().EndsWith(".wav"))
        {
            filename += ".wav";
        }

        var filepath = Path.Combine("Assets/Resources/", filename);

        Debug.Log(filepath);

        // Make sure directory exists if user is saving to sub dir.
        Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        using (var fileStream = CreateEmpty(filepath))
        {

            ConvertAndWrite(fileStream, clip);

            WriteHeader(fileStream, clip);
        }

        return true; // TODO: return false if there's a failure saving the file

    }

    static FileStream CreateEmpty(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < 44; i++) //preparing the header
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }

    static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    {

        var samples = new float[clip.samples];

        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

        Byte[] bytesData = new Byte[samples.Length * 2];
        //bytesData array is twice the size of
        //dataSource array because a float converted in Int16 is 2 bytes.

        int rescaleFactor = 32767; //to convert float to Int16

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            Byte[] byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    static void WriteHeader(FileStream fileStream, AudioClip clip)
    {

        var hz = clip.frequency;
        var channels = clip.channels;
        var samples = clip.samples;

        fileStream.Seek(0, SeekOrigin.Begin);

        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);

        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        Byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        UInt16 two = 2;
        UInt16 one = 1;

        Byte[] audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        Byte[] numChannels = BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);

        Byte[] sampleRate = BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);

        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
        fileStream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort)(channels * 2);
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        fileStream.Write(bitsPerSample, 0, 2);

        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(datastring, 0, 4);

        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        fileStream.Write(subChunk2, 0, 4);

        //      fileStream.Close();
    }

    public void VoiceController(float speedvalue)
    {

        if (speedvalue == 1.0f)
            speed = 1.4f;
        else if (speedvalue == 0.0f)
            speed = 1.0f;
        else
            speed = 0.7f;
        ChangeSpeed(speed);
    }

    void ChangeSpeed(float speed)
    {
        VoiceMixer.audioMixer.SetFloat("VoiceMixer_Pitch", speed);
        VoiceMixer.audioMixer.SetFloat("VoiceMixer_PitchShifter", 1 / speed);
        BackGroundMixer.audioMixer.SetFloat("BackGroundMixer_Pitch", speed);
        BackGroundMixer.audioMixer.SetFloat("BackGroundMixer_PitchShifter", speed);
        LipsyncMixer.audioMixer.SetFloat("Lipsync_pitch", speed);
        LipsyncMixer.audioMixer.SetFloat("Lipsync_pitchshifter", 1 / speed);
        SongMixer.audioMixer.SetFloat("SongMixer_Pitch", speed);
        SongMixer.audioMixer.SetFloat("SongMixer_PitchShifter", speed);
    }

}