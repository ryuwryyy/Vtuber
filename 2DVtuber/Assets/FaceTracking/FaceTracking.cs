using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System;
using UnityEngine.UI;

public class FaceTracking : MonoBehaviour {
    public GameObject target;
    public GameObject headtarget;
    public Text text;
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
    private Memory posX, posY, posZ;
    private Memory rotY, rotR, rotP;
    private Memory morphEye, morphSmile, morphMouth;

    //position
    private float initx = 0, inity = 0, initz = 0;

    //rotation
    private float initpitch = 0, inityaw = 0, initroll = 0;

    //morph
    private int num_eye = 0;
    private int num_smile = 0;
    private int num_mouth = 0;

    private float time;

    // Use this for initialization
    void Start()
    {
        Init(SelectDeviceNumber);
        //
        posX.Init(10, 0.1f);
        posY.Init(10, 0.1f);
        posZ.Init(10, 0.1f);
        rotY.Init(10, 1.0f);
        rotR.Init(10, 1.0f);
        rotP.Init(10, 1.0f);
        morphEye.Init(5, 10);
        morphSmile.Init(5, 10);
        morphMouth.Init(5, 10);
        //position
        initx = target.transform.position.x;
        inity = target.transform.position.y;
        initz = target.transform.position.z;
        //rotation
        initpitch = headtarget.transform.localPosition.x;
        inityaw = 0;// head.transform.position.y;
        initroll =0;

        time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        if (time > 0.01f)
        {
            //position
            target.transform.position = new Vector3(posX.SetMemory(((float)GetDetection(0)) / 1000) + initx, posY.SetMemory(((float)GetDetection(1)) / 1000) + inity, posZ.SetMemory(((float)200 - GetDetection(2)) / 1000) + initz);
            //rotation
            headtarget.transform.position = new Vector3(rotY.SetMemory(GetRotation(0) / 10) + inityaw, rotP.SetMemory(GetRotation(1) / 10) + initpitch, headtarget.transform.position.z);
            target.transform.LookAt(headtarget.transform);
            //morph
            float buf=morphEye.SetMemory(((float)GetExpression(7)) / 100);
            text.text = "EYES_CLOSED : " +buf;

            time = 0;
        }
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
