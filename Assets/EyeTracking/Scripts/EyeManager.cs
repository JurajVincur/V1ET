using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PupilLabs;
using Leap.Unity.AR;

public class EyeManager : MonoBehaviour
{
    public OpticalCalibrationManager manager;
    public ARRaytracer leftEyeARRaytracer;
    public ARRaytracer rightEyeARRaytracer;

    public GameObject constrainingQuad;
    public bool quadConstraint;
    public float ipd = 0.064f;

    public int rightEyeId = 0; //right should be zero!!!

    public SubscriptionsController subsCtrl;
    public TimeSync timeSync;

    public Transform leftCam;
    public Transform rightCam;

    public Transform leftRef;
    public Transform rightRef;

    public Transform leftEye;
    public Transform rightEye;

    private PupilListener listener;

    void OnEnable()
    {
        if (listener == null)
        {
            listener = new PupilListener(subsCtrl);
        }

        listener.Enable();
        listener.OnReceivePupilData += ReceivePupilData;
    }

    void OnDisable()
    {
        listener.Disable();
        listener.OnReceivePupilData -= ReceivePupilData;
    }

    private Vector3 constrainToQuad(Vector3 camPos, Vector3 eyePos)
    {
        var plane = new Plane(constrainingQuad.transform.forward, constrainingQuad.transform.position);
        float enter = 0f;
        var ray = new Ray(camPos, eyePos - camPos);
        Debug.DrawRay(ray.origin, ray.direction, Color.blue);
        plane.Raycast(ray, out enter);
        return ray.GetPoint(enter);
    }

    void ReceivePupilData(PupilData pupilData)
    {
        double unityTime = timeSync.ConvertToUnityTime(pupilData.PupilTimestamp);
        //Debug.Log($"Receive Pupil Data with method {pupilData.Method} and confidence {pupilData.Confidence} at {unityTime}");
        Debug.Log(pupilData.Sphere.Center);
        //Debug.Log(pupilData.Circle.Normal);
        if (pupilData.Confidence > 0.75)
        {
            var localPos = Vector3.Scale(pupilData.Sphere.Center, new Vector3(0.001f, -0.001f, 0.001f));
            var localDir = Vector3.Scale(pupilData.Circle.Normal, new Vector3(1f, -1f, 1f));
            Ray globalRay = new Ray();
            if (pupilData.EyeIdx == rightEyeId)//right
            {
                rightEye.position = rightCam.TransformPoint(localPos);
                rightRef.localPosition = localPos;

                if (quadConstraint && constrainingQuad != null)
                {
                    rightEye.position = constrainToQuad(rightCam.position, rightEye.position);
                }

                globalRay.origin = rightEye.position;
                globalRay.direction = rightCam.TransformDirection(localDir);
                Debug.DrawRay(globalRay.origin, globalRay.direction, Color.red);
            }
            else //left
            {
                leftEye.position = leftCam.TransformPoint(localPos);
                leftRef.localPosition = localPos;

                if (quadConstraint && constrainingQuad != null)
                {
                    leftEye.position = constrainToQuad(leftCam.position, leftEye.position);
                }

                globalRay.origin = leftEye.position;
                globalRay.direction = leftCam.TransformDirection(localDir);
                Debug.DrawRay(globalRay.origin, globalRay.direction, Color.green);
            }

            var dist = Vector3.Distance(leftEye.position, rightEye.position);
            if (dist > ipd)
            {
                constrainingQuad.transform.localPosition += new Vector3(0f, 0f, 0.0001f);
            }
            else
            {
                constrainingQuad.transform.localPosition -= new Vector3(0f, 0f, 0.0001f);
            }

            /*
            RaycastHit hit;
            if (Physics.SphereCast(globalRay, 0.005f, out hit, 1, LayerMask.GetMask(new string[] { "Gaze" })))
            {
            }
            */
        }
    }

    public void LateUpdate()
    {
        if (manager != null)
        {
            manager.currentCalibration.leftEye.eyePosition = leftEye.localPosition;
            manager.currentCalibration.rightEye.eyePosition = rightEye.localPosition;
            manager.UpdateCalibrationFromObjects();
        }

        leftEyeARRaytracer.ScheduleCreateDistortionMesh();
        rightEyeARRaytracer.ScheduleCreateDistortionMesh();
    }

    private void Awake()
    {

    }
}