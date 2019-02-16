using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class FaceLive2D : MonoBehaviour {
    public int SelectDeviceNumber;

    [DllImport("FaceTracking_dll")]
    private static extern void Init(int dev);
    [DllImport("FaceTracking_dll")]
    private static extern int GetExpression(int num);
    //x,y,size
    [DllImport("FaceTracking_dll")]
    private static extern int GetDetection(int num);
    //BROW_RAISER_LEFT = 0,
    //BROW_RAISER_RIGHT = 1,
    //BROW_LOWERER_LEFT = 2,
    //BROW_LOWERER_RIGHT = 3,
    //SMILE = 4,
    //KISS = 5,
    //MOUTH_OPEN = 6,
    //EYES_CLOSED_LEFT = 7,
    //EYES_CLOSED_RIGHT = 8,
    //0-100
    [DllImport("FaceTracking_dll")]
    private static extern float GetRotation(int num);
    //yaw,pitch,roll
    [DllImport("FaceTracking_dll")]
    private static extern void Stop();

    struct Memory
    {
        private float[] val;
        private float pre;
        private float lim;
        public void Init(int count, float limit)
        {
            val = new float[count];
            lim = limit;
            pre = 0;
        }
        public float SetMemory(float v)
        {
            float buf = Mathf.Clamp(v, pre - lim, pre + lim);
            for (int i = val.Length - 2; i >= 0; i--) val[i + 1] = val[i];
            val[0] = buf;
            buf = 0;
            for (int i = 0; i < val.Length; i++) buf += val[i] / val.Length;
            pre = buf;
            return buf;
        }
    }
    //private Memory posX, posY, posZ;
    private Memory rotY, rotR, rotP;
    private Memory morphEyeL, morphEyeR, morphMouth;

    //rotation
    private float initpitch = 0, inityaw = 0, initroll = 0;

    //morph
    private float num_eye_L = 0, num_eye_R = 0;
    private float num_mouth = 0;

    private float time;

    // Use this for initialization
    void Start () {
        Init(SelectDeviceNumber);
        rotY.Init(10, 1.0f);
        rotR.Init(10, 1.0f);
        rotP.Init(10, 1.0f);
        morphEyeL.Init(5, 10);
        morphEyeR.Init(5, 10);
        morphMouth.Init(5, 10);

        time = 0;
    }
	
	// Update is called once per frame
	void Update () {
        time += Time.deltaTime;
        if (time > 0.01f)
        {
            //rotation
            inityaw = rotY.SetMemory(GetRotation(0) / 10);
            initpitch = rotP.SetMemory(GetRotation(1) / 10)-0.5f;
            initroll = rotR.SetMemory(GetRotation(2) / 10);
            //morph
            num_eye_L = morphEyeL.SetMemory(((float)GetExpression(7)) / 100);
            num_eye_R = morphEyeR.SetMemory(((float)GetExpression(8)) / 100);
            num_mouth = morphMouth.SetMemory(((float)GetExpression(6)) / 100);

            time = 0;
        }
    }

    public float GetRot(int num)
    {
        if (num == 0) return inityaw;
        else if (num == 1) return initpitch;
        else if (num == 2) return initroll;
        else return 0;
    }

    public float GetMorph(int num)
    {
        if (num == 0) return num_eye_L;
        else if (num == 1) return num_eye_R;
        else if (num == 2) return num_mouth;
        else return 0;
    }

    public void Reset()
    {
        Stop();
        Invoke("Restart", 1.0f);
    }

    private void Restart()
    {
        Init(SelectDeviceNumber);
    }

    void OnApplicationQuit()
    {
        Stop();
    }
}
