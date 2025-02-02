﻿using Modding;
using SFCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using UObject = UnityEngine.Object;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using UnityEngine.Video;
using System.Linq;
using System.Text;
using SFCore.Utils;

namespace ZaliantsSurprise;

public class ZaliantsSurprise : Mod
{
    public static ZaliantsSurprise Instance;

    public LanguageStrings LangStrings { get; private set; }

    private AssetBundle _abTitleScreenRick = null;

    public override string GetVersion() => Util.GetVersion(Assembly.GetExecutingAssembly());

    public override List<ValueTuple<string, string>> GetPreloadNames()
    {
        return new List<(string, string)>()
        {
            ("Room_shop", "_SceneManager")
        };
    }

    public ZaliantsSurprise() : base("Zaliants Surprise")
    {
        LangStrings = new LanguageStrings(Assembly.GetExecutingAssembly(), "ZaliantsSurprise.Resources.Language.json", Encoding.UTF8);

        InitCallbacks();
    }

    public override void Initialize()
    {
        Log("Initializing");
        Instance = this;

        var tmpStyle = MenuStyles.Instance.styles.First(x => x.styleObject.name.Contains("Rick Style"));
        MenuStyles.Instance.SetStyle(MenuStyles.Instance.styles.ToList().IndexOf(tmpStyle), false);

        Log("Initialized");
    }

    private void InitCallbacks()
    {
        MenuStyleHelper.AddMenuStyleHook += AddMenuStyleRick;

        ModHooks.LanguageGetHook += OnLanguageGetHook;
    }

    private (string languageString, GameObject styleGo, int titleIndex, string unlockKey, string[] achievementKeys, MenuStyles.MenuStyle.CameraCurves cameraCurves, AudioMixerSnapshot musicSnapshot) AddMenuStyleRick(MenuStyles self)
    {
        GameObject pcStyleGo = new GameObject("Rick Style");
        pcStyleGo.SetActive(false);

        pcStyleGo.transform.SetParent(self.gameObject.transform);
        pcStyleGo.transform.localPosition = new Vector3(0, -1.2f, 0);

        #region Loading assetbundle

        if (_abTitleScreenRick == null)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            using (Stream s = asm.GetManifestResourceStream("ZaliantsSurprise.Resources.rick"))
            {
                if (s != null)
                {
                    _abTitleScreenRick = AssetBundle.LoadFromStream(s);
                }
            }
        }

        #endregion

        var aSource = pcStyleGo.AddComponent<AudioSource>();
        aSource.clip = null;
        aSource.outputAudioMixerGroup = self.styles[0].styleObject.Find("Audio Player Actor 2D (1)")
            .GetComponent<AudioSource>().outputAudioMixerGroup;
        aSource.mute = false;
        aSource.bypassEffects = false;
        aSource.bypassListenerEffects = false;
        aSource.bypassReverbZones = false;
        aSource.playOnAwake = true;
        aSource.loop = true;
        aSource.priority = 128;
        aSource.volume = 1;
        aSource.pitch = 1;
        aSource.panStereo = 0;
        aSource.spatialBlend = 0;
        aSource.reverbZoneMix = 1;
        aSource.dopplerLevel = 0;
        aSource.spread = 0;
        aSource.rolloffMode = AudioRolloffMode.Custom;
        aSource.maxDistance = 58.79711f;
        aSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, new AnimationCurve(new []
        {
            new Keyframe(45.86174f, 1),
            new Keyframe(55.33846f, 0)
        }));
        var vp = pcStyleGo.AddComponent<VideoPlayer>();
        //vp.playOnAwake = false;
        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        vp.renderMode = VideoRenderMode.CameraFarPlane;
        vp.isLooping = true;
        vp.targetCamera = GameCameras.instance.mainCamera;
        vp.source = VideoSource.VideoClip;
        vp.clip = _abTitleScreenRick.LoadAsset<VideoClip>("RickRoll");
        vp.SetTargetAudioSource(0, aSource);
        UObject.DontDestroyOnLoad(vp.clip);

        var cameraCurves = new MenuStyles.MenuStyle.CameraCurves();
        cameraCurves.saturation = 1f;
        cameraCurves.redChannel = new AnimationCurve();
        cameraCurves.redChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.redChannel.AddKey(new Keyframe(1f, 1f));
        cameraCurves.greenChannel = new AnimationCurve();
        cameraCurves.greenChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.greenChannel.AddKey(new Keyframe(1f, 1f));
        cameraCurves.blueChannel = new AnimationCurve();
        cameraCurves.blueChannel.AddKey(new Keyframe(0f, 0f));
        cameraCurves.blueChannel.AddKey(new Keyframe(1f, 1f));
        UObject.DontDestroyOnLoad(pcStyleGo);
        pcStyleGo.SetActive(true);
        //PrintDebug(pcStyleGo);
        return ("UI_MENU_STYLE_RICK", pcStyleGo, -1, "", null, cameraCurves, Resources.FindObjectsOfTypeAll<AudioMixer>().First(x => x.name == "Music").FindSnapshot("Silent"));
    }

    private string OnLanguageGetHook(string key, string sheet, string orig)
    {
        //Log($"Sheet: {sheet}; Key: {key}");
        if (LangStrings.ContainsKey(key, sheet))
        {
            return LangStrings.Get(key, sheet);
        }
        return orig;
    }

    private static void SetInactive(GameObject go)
    {
        if (go == null) return;

        UnityEngine.Object.DontDestroyOnLoad(go);
        go.SetActive(false);
    }

    private static void SetInactive(UnityEngine.Object go)
    {
        if (go != null)
        {
            UnityEngine.Object.DontDestroyOnLoad(go);
        }
    }
}