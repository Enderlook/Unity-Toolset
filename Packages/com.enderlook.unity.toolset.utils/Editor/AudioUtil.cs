using System;
using System.Reflection;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Toolset.Utils
{
    /// <summary>
    /// A wrapper to a non-exposed AudioUtil api from Unity Editor.
    /// </summary>
    public static class AudioUtil
    {
        // TODO: Add documentation to public members
#pragma warning disable CS1591
        // https://forum.unity.com/threads/way-to-play-audio-in-editor-using-an-editor-script.132042/
        // https://github.com/MattRix/UnityDecompiled/blob/cc432a3de42b53920d5d5dae85968ff993f4ec0e/UnityEditor/UnityEditor/AudioUtil.cs
        // https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Audio/Bindings/AudioUtil.bindings.cs

        private static readonly Func<bool> resetAllAudioClipPlayCountsOnPlayGetter;
        private static readonly Action<bool> resetAllAudioClipPlayCountsOnPlaySetter;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity called it in this way.")]
        public static bool resetAllAudioClipPlayCountsOnPlay {
            get => resetAllAudioClipPlayCountsOnPlayGetter();
            set => resetAllAudioClipPlayCountsOnPlaySetter(value);
        }

        private static readonly Func<bool> canUseSpatializerEffectGetter;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Unity called it in this way.")]
        public static bool canUseSpatializerEffect => canUseSpatializerEffectGetter();

#if !UNITY_2019_1_OR_NEWER
        private static readonly Action<AudioClip> PlayClip1;
        private static readonly Action<AudioClip, int> PlayClip2;
#endif
        private static readonly Action<AudioClip, int, bool> PlayClip3;

        public static readonly Action<AudioClip> StopClip;
        public static readonly Action<AudioClip> PauseClip;
        public static readonly Action<AudioClip> ResumeClip;
        public static readonly Action<AudioClip, bool> LoopClip;
        public static readonly Func<AudioClip, bool> IsClipPlaying;
        public static readonly Action StopAllClips;
        public static readonly Func<AudioClip, float> GetClipPosition;
        public static readonly Func<AudioClip, int> GetClipSamplePosition;
        public static readonly Action<AudioClip, int> SetClipSamplePosition;
        public static readonly Func<AudioClip, int> GetSampleCount;
        public static readonly Func<AudioClip, int> GetChannelCount;
        public static readonly Func<AudioClip, int> GetBitRate;
        public static readonly Func<AudioClip, int> GetBitsPerSample;
        public static readonly Func<AudioClip, int> GetFrequency;
        public static readonly Func<AudioClip, int> GetSoundSize;
        public static readonly Func<AudioClip, AudioCompressionFormat> GetSoundCompressionFormat;
        public static readonly Func<AudioClip, AudioCompressionFormat> GetTargetPlatformSoundCompressionFormat;
        public static readonly Func<string[]> GetAmbisonicDecoderPluginNames;
        public static readonly Func<AudioClip, bool> HasPreview;
        public static readonly Func<AudioClip, AudioImporter> GetImporterFromClip;
        public static readonly Func<AudioImporter, float[]> GetMinMaxData;
        public static readonly Func<AudioClip, double> GetDuration;
        public static readonly Func<int> GetFMODMemoryAllocated;
        public static readonly Func<float> GetFMODCPUUsage;
        public static readonly Func<AudioClip, bool> IsMovieAudio;
        public static readonly Func<AudioClip, bool> IsTrackerFile;
        public static readonly Func<AudioClip, int> GetMusicChannelCount;
        public static readonly Func<AudioLowPassFilter, AnimationCurve> GetLowpassCurve;
        public static readonly Func<Vector3> GetListenerPos;
        public static readonly Action UpdateAudio;
        public static readonly Action<Transform> SetListenerTransform;
        public static readonly Func<MonoBehaviour, bool> HasAudioCallback;
        public static readonly Func<MonoBehaviour, int> GetCustomFilterChannelCount;
        public static readonly Func<MonoBehaviour, int> GetCustomFilterProcessTime;
        public static readonly Func<MonoBehaviour, int, float> GetCustomFilterMaxIn;
        public static readonly Func<MonoBehaviour, int, float> GetCustomFilterMaxOut;
#pragma warning disable CS1591

        static AudioUtil()
        {
            Type audioUtilClass = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");

            const BindingFlags bindingFlags = BindingFlags.Static | BindingFlags.Public;

            T CreateDelegate<T>(MethodInfo methodInfo) where T : Delegate => (T)methodInfo.CreateDelegate(typeof(T));

            T GetDelegate<T>(string name, params Type[] parameters) where T : Delegate => CreateDelegate<T>(audioUtilClass.GetMethod(name, bindingFlags, null, parameters, null));

            PropertyInfo resetAllAudioClipPlayCountsOnPlayProperty = audioUtilClass.GetProperty("resetAllAudioClipPlayCountsOnPlay", bindingFlags);
            resetAllAudioClipPlayCountsOnPlayGetter = CreateDelegate<Func<bool>>(resetAllAudioClipPlayCountsOnPlayProperty.GetGetMethod());
            resetAllAudioClipPlayCountsOnPlaySetter = CreateDelegate<Action<bool>>(resetAllAudioClipPlayCountsOnPlayProperty.GetSetMethod());

            canUseSpatializerEffectGetter = CreateDelegate<Func<bool>>(audioUtilClass.GetProperty("canUseSpatializerEffect", bindingFlags).GetGetMethod());

#if !UNITY_2019_1_OR_NEWER
            PlayClip1 = GetDelegate<Action<AudioClip>>("PlayClip", typeof(AudioClip));
            PlayClip2 = GetDelegate<Action<AudioClip, int>>("PlayClip", typeof(AudioClip), typeof(int));
#endif
            PlayClip3 = GetDelegate<Action<AudioClip, int, bool>>("PlayClip", typeof(AudioClip), typeof(int), typeof(bool));

            StopClip = GetDelegate<Action<AudioClip>>("StopClip", typeof(AudioClip));
            PauseClip = GetDelegate<Action<AudioClip>>("PauseClip", typeof(AudioClip));
            ResumeClip = GetDelegate<Action<AudioClip>>("ResumeClip", typeof(AudioClip));
            LoopClip = GetDelegate<Action<AudioClip, bool>>("LoopClip", typeof(AudioClip), typeof(bool));
            IsClipPlaying = GetDelegate<Func<AudioClip, bool>>("IsClipPlaying", typeof(AudioClip));
            StopAllClips = GetDelegate<Action>("StopAllClips", Type.EmptyTypes);
            GetClipPosition = GetDelegate<Func<AudioClip, float>>("GetClipPosition", typeof(AudioClip));
            GetClipSamplePosition = GetDelegate<Func<AudioClip, int>>("GetClipSamplePosition", typeof(AudioClip));
            SetClipSamplePosition = GetDelegate<Action<AudioClip, int>>("SetClipSamplePosition", typeof(AudioClip), typeof(int));
            GetSampleCount = GetDelegate<Func<AudioClip, int>>("GetSampleCount", typeof(AudioClip));
            GetChannelCount = GetDelegate<Func<AudioClip, int>>("GetChannelCount", typeof(AudioClip));
            GetBitRate = GetDelegate<Func<AudioClip, int>>("GetBitRate", typeof(AudioClip));
            GetBitsPerSample = GetDelegate<Func<AudioClip, int>>("GetBitsPerSample", typeof(AudioClip));
            GetFrequency = GetDelegate<Func<AudioClip, int>>("GetFrequency", typeof(AudioClip));
            GetSoundSize = GetDelegate<Func<AudioClip, int>>("GetSoundSize", typeof(AudioClip));
            GetSoundCompressionFormat = GetDelegate<Func<AudioClip, AudioCompressionFormat>>("GetSoundCompressionFormat", typeof(AudioClip));
            GetTargetPlatformSoundCompressionFormat = GetDelegate<Func<AudioClip, AudioCompressionFormat>>("GetTargetPlatformSoundCompressionFormat", typeof(AudioClip));
            GetAmbisonicDecoderPluginNames = GetDelegate<Func<string[]>>("GetAmbisonicDecoderPluginNames", Type.EmptyTypes);
            HasPreview = GetDelegate<Func<AudioClip, bool>>("HasPreview", typeof(AudioClip));
            GetImporterFromClip = GetDelegate<Func<AudioClip, AudioImporter>>("GetImporterFromClip", typeof(AudioClip));
            GetMinMaxData = GetDelegate<Func<AudioImporter, float[]>>("GetMinMaxData", typeof(AudioImporter));
            GetDuration = GetDelegate<Func<AudioClip, double>>("GetDuration", typeof(AudioClip));
            GetFMODMemoryAllocated = GetDelegate<Func<int>>("GetFMODMemoryAllocated", Type.EmptyTypes);
            GetFMODCPUUsage = GetDelegate<Func<float>>("GetFMODCPUUsage", Type.EmptyTypes);
            IsMovieAudio = GetDelegate<Func<AudioClip, bool>>("IsMovieAudio", typeof(AudioClip));
            IsTrackerFile = GetDelegate<Func<AudioClip, bool>>("IsTrackerFile", typeof(AudioClip));
            GetMusicChannelCount = GetDelegate<Func<AudioClip, int>>("GetMusicChannelCount", typeof(AudioClip));
            GetLowpassCurve = GetDelegate<Func<AudioLowPassFilter, AnimationCurve>>("GetLowpassCurve", typeof(AudioLowPassFilter));
            UpdateAudio = GetDelegate<Action>("UpdateAudio", Type.EmptyTypes);
            GetListenerPos = GetDelegate<Func<Vector3>>("GetListenerPos", Type.EmptyTypes);
            SetListenerTransform = GetDelegate<Action<Transform>>("SetListenerTransform", typeof(Transform));
            HasAudioCallback = GetDelegate<Func<MonoBehaviour, bool>>("HasAudioCallback", typeof(MonoBehaviour));
            GetCustomFilterChannelCount = GetDelegate<Func<MonoBehaviour, int>>("GetCustomFilterChannelCount", typeof(MonoBehaviour));
            GetCustomFilterProcessTime = GetDelegate<Func<MonoBehaviour, int>>("GetCustomFilterProcessTime", typeof(MonoBehaviour));
            GetCustomFilterMaxIn = GetDelegate<Func<MonoBehaviour, int, float>>("GetCustomFilterMaxIn", typeof(MonoBehaviour), typeof(int));
            GetCustomFilterMaxOut = GetDelegate<Func<MonoBehaviour, int, float>>("GetCustomFilterMaxOut", typeof(MonoBehaviour), typeof(int));
        }

        public static void PlayClip(AudioClip audioClip) =>
#if !UNITY_2019_1_OR_NEWER
            PlayClip1(audioClip);
#else
            PlayClip3(audioClip, 0, false);
#endif

        public static void PlayClip(AudioClip audioClip, int startSample = 0) =>
#if !UNITY_2019_1_OR_NEWER
            PlayClip2(audioClip, startSample);
#else
            PlayClip3(audioClip, startSample, false);
#endif

        public static void PlayClip(AudioClip audioClip, int startSample = 0, bool loop = false) => PlayClip3(audioClip, startSample, loop);
    }
}