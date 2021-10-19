using System;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
using UnityEngine.Scripting;
using UnityEngine.SpeechRecognition;

public class GameVoiceControl : MonoBehaviour, IGetCrashMessages, IGetLogMessages, IGetResult, IGetInitResult
{
    public enum language
    {
        en_US,
        ru_RU,
        fr_FR,
        de_DE,
        es_ES,
        it_IT,
        nl_NL,
        hi_IN,
        pt_PT
    }
    [Header("Debug mode")]
    public bool Enable = true;
    [Header("Select language")]
    public language lang;
    [Header("Keyword (OK GOOGLE)")]
    public string keyword;
    [Header("Keyword (OK GOOGLE) threshold [1e-50..1e-1]")]
    public double keywordThreshold = 1e-10f;
    [Header("Voice activity detection threshold [1..5]")]
    public double vadThreshold = 4.0;
    [Header("Set microphone index")]
    public byte microphoneIndex = 0;
    [Header("Time to read microphone buffer (ms)")]
    public float timeoutInterval = 100.0f;
    [Header("Grammars")]
    public GrammarFileStruct[] grammarStructs;
    [Header("Made-up words")]
    public PairG2P[] pairsOfGraphemePhonemes;
    [System.Serializable]
    public class ResultEvent : UnityEvent<String> { }
    [System.Serializable]
    public class InitEvent : UnityEvent<bool> { }

    public ResultEvent RecognitionResult = new ResultEvent();
    public InitEvent InitResult = new InitEvent();

    [Header("Log")]
    public Log log = null;
    public void getLogMessages(string message)
    {
        if (Enable)
        {
            if (log == null) return;
            log.add("log:" + message);
        }
    }
    public void getResult(string result)
    {
        if (log != null)
        {
            log.clear();
            log.add("<color=green>result:" + result + "</color>");
        }
        if (result == keyword)
        {
            _speechRecognizer.switchGrammar(grammarStructs[0].name);
        }
        else
        {
            if (RecognitionResult != null)
            {
                RecognitionResult.Invoke(result);
                _speechRecognizer.searchKeyword();
            }
        }
    }
    public void onStartListening()
    {
        if (!_start)
        {
            if (log != null)
                log.clear();
            _speechRecognizer.startListening();
            _start = true;

            VoiceCommands.instance.SetAudioActive();
        }
    }
    public void onStopListening()
    {
        if (_start)
        {
            _speechRecognizer.stopListening();
            _start = false;
        }
    }
    public void getCrashMessages(string message)
    {
        if (log != null)
        {
            log.add("<color=red>crash:" + message + "</color>");
        }
    }
    public void initComplete(bool result)
    {
        if (log != null)
        {
            log.add("<color=green>init complete:" + result + "</color>");
        }
        if (InitResult != null)
            InitResult.Invoke(result);
    }
    void initSpeechRecognizer()
    {
        Debug.Log("start initSpeechRecognizer");
        _speechRecognizer = new SpeechRecognizer(this);
        _speechRecognizer.setMessagesFromLogRecieverMethod(this);
        _speechRecognizer.setResultRecieverMethod(this);
        _speechRecognizer.setCrashMessagesRecieverMethod(this);
        _speechRecognizer.setInitResultRecieverMethod(this);
        Debug.Log("end initSpeechRecognizer");
        foreach (var pair in pairsOfGraphemePhonemes)
        {
            _speechRecognizer.addPairG2P(pair);
        }

        _speechRecognizer.init(lang.ToString(), grammarStructs, keyword, keywordThreshold, vadThreshold, timeoutInterval, microphoneIndex.ToString());
    }
    void Start()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#endif
    }

    private bool _init = false;

    void Update()
    {
        bool micAutorized = true;
#if UNITY_ANDROID
        micAutorized = Permission.HasUserAuthorizedPermission(Permission.Microphone);
#endif
        if (micAutorized)
        {
            if (!_init)
            {
                initSpeechRecognizer();
            }
            _init = true;
        }
    }

    void OnDestroy()
    {
        _speechRecognizer.Dispose();
        _speechRecognizer = null;
    }

    bool _start = false;
    SpeechRecognizer _speechRecognizer = null;
#if UNITY_ANDROID
    Microphone mic = new Microphone();
#endif
}
