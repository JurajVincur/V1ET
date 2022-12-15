using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnePointCalibration : MonoBehaviour
{
    public EyeManager manager;
    public GameObject calibSphere;
    public Vector3[] targets;

    private Stack<Vector3> remainingTargets = null;
    private Quaternion leftOpticalToVisual = Quaternion.identity;
    private Quaternion rightOpticalToVisual = Quaternion.identity;

    void Awake()
    {
        calibSphere.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F2) == true)
        {
            if (remainingTargets == null)
            {
                //start calibration
                remainingTargets = new Stack<Vector3>(targets);
                calibSphere.transform.position = remainingTargets.Pop();
                calibSphere.SetActive(true);
            }
            else
            {
                //record sample
                leftOpticalToVisual *= Quaternion.Slerp(Quaternion.identity, OpticalToVisual(calibSphere.transform.position - manager.LeftEyeRay.origin, manager.LeftEyeRay.direction), 1.0f / targets.Length);
                rightOpticalToVisual *= Quaternion.Slerp(Quaternion.identity, OpticalToVisual(calibSphere.transform.position - manager.RightEyeRay.origin, manager.RightEyeRay.direction), 1.0f / targets.Length);

                //finish?
                if (remainingTargets.Count == 0)
                {
                    manager.LeftOpticalToVisual = leftOpticalToVisual;
                    manager.RightOpticalToVisual = rightOpticalToVisual;
                    leftOpticalToVisual = Quaternion.identity;
                    rightOpticalToVisual = Quaternion.identity;
                    remainingTargets = null;
                    calibSphere.SetActive(false);
                    Debug.Log($"Visual to optical angles for left eye {manager.LeftOpticalToVisual.eulerAngles}");
                    Debug.Log($"Visual to optical angles for right eye {manager.RightOpticalToVisual.eulerAngles}");
                }
                else
                {
                    calibSphere.transform.position = remainingTargets.Pop();
                }
            }
        }
    }

    private Quaternion OpticalToVisual(Vector3 calibDir, Vector3 opticalDir)
    {
        return Quaternion.FromToRotation(opticalDir, calibDir);
    }
}
