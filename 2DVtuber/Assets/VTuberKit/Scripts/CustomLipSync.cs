using UnityEngine;
using System.Collections;
using Live2D.Cubism.Core;

namespace VTuberKit
{
    [RequireComponent(typeof(AudioSource))]
    public class CustomLipSync : MonoBehaviour
    {
        AudioSource audioSource = null;
        [SerializeField]
        float gain = 10f;
        [SerializeField, Range(0f, 1f)]
        float minVolume = 0.1f;
        [SerializeField]
        float smoothTime = 0.05f;

        private OVRLipSyncContextBase lipsyncContext = null;

        [SerializeField]
        CubismModel _model = null;

        float velocity = 0.0f;
        float currentVolume = 0.0f;

        private void LateUpdate()
        {
            // 形態素の取得
            // if (lipsyncContext != null)
            // {
            //     OVRLipSync.Frame frame = lipsyncContext.GetCurrentPhonemeFrame();
            //     if (frame != null)
            //     {
            //         for (int i = 0; i < frame.Visemes.Length; i++)
            //         {
            //             Debug.LogFormat("{0}: {1}", (OVRLipSync.Viseme)i, frame.Visemes[i]);
            //         }
            //     }
            // }

            float targetVolume = GetAveragedVolume() * gain;
            targetVolume = targetVolume < minVolume ? 0 : targetVolume;

            currentVolume = Mathf.SmoothDamp(currentVolume, targetVolume, ref velocity, smoothTime);

            var parameter = _model.Parameters[13];
            parameter.Value = Mathf.Clamp01(currentVolume);
        }
        void Start()
        {
            audioSource = GetComponent<AudioSource>();

            lipsyncContext = GetComponent<OVRLipSyncContextBase>();
            if (lipsyncContext == null)
            {
                Debug.Log("LipSyncContextTextureFlip.Start WARNING: No lip sync context component set to object");
            }
        }

        float GetAveragedVolume()
        {
            float[] data = new float[256];
            float a = 0;
            audioSource.GetOutputData(data, 0);
            foreach (float s in data)
            {
                a += Mathf.Abs(s);
            }
            return a / 255.0f;
        }
    }
}