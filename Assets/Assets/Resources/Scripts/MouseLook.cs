using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Camera-Control/Mouse Look")]
public class MouseLook : MonoBehaviour
{
    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;

    public float sensitivityX = 2.0f;
    public float sensitivityY = 2.0f;

    public float minimumY = -89.0f;
    public float maximumY = 89.0f;

    // Based off mouse axis, not rotational axis
    public float rotationX = 0.0f;
    public GameObject cmra = null;

    public float rotationY = 0.0f;

    // Variables to track and average mouse speed
    private List<float> rotArrayX = new List<float>();
    float rotAverageX = 0.0f;

    private List<float> rotArrayY = new List<float>();
    float rotAverageY = 0.0f;

    public float frameCounter = 5;

    Quaternion originalRotation;

    void Start()
    {
        //cmra = GameObject.FindWithTag("Camera");

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        if (axes == RotationAxes.MouseXAndY)
        {
            // Read the mouse input axis
            rotationX += (Input.GetAxis("Mouse X") * sensitivityX / 30 * cmra.GetComponent<Camera>().fieldOfView);
            rotationY += (Input.GetAxis("Mouse Y") * sensitivityY / 30 * cmra.GetComponent<Camera>().fieldOfView);

            // Clamp Y axis rotation
            rotationY = ClampAngle(rotationY, minimumY, maximumY);

            // Average out mouse speed to smooth rotation for less jitter
            rotAverageY = 0.0f;
            rotAverageX = 0.0f;

            rotArrayY.Add(rotationY);
            rotArrayX.Add(rotationX);

            if (rotArrayY.Count >= frameCounter)
            {
                rotArrayY.RemoveAt(0);
            }
            if (rotArrayX.Count >= frameCounter)
            {
                rotArrayX.RemoveAt(0);
            }

            for (int j = 0; j < rotArrayY.Count; j++)
            {
                rotAverageY += rotArrayY[j];
            }
            for (int i = 0; i < rotArrayX.Count; i++)
            {
                rotAverageX += rotArrayX[i];
            }

            rotAverageY /= rotArrayY.Count;
            rotAverageX /= rotArrayX.Count;

            // Apply rotation
            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);

            transform.localRotation = originalRotation * xQuaternion * yQuaternion;
        }
        else if (axes == RotationAxes.MouseX)
        {
            // Read the mouse input axis
            rotationX += (Input.GetAxis("Mouse X") * sensitivityX / 60 * cmra.GetComponent<Camera>().fieldOfView);

            // Average out mouse speed to smooth rotation for less jitter
            rotAverageX = 0.0f;
            rotArrayX.Add(rotationX);

            if (rotArrayX.Count >= frameCounter)
            {
                rotArrayX.RemoveAt(0);
            }

            for (int i = 0; i < rotArrayX.Count; i++)
            {
                rotAverageX += rotArrayX[i];
            }
            rotAverageX /= rotArrayX.Count;

            // Apply rotation
            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            transform.localRotation = originalRotation * xQuaternion;
        }
        else
        {
            // Read the mouse input axis
            rotationY += (Input.GetAxis("Mouse Y") * sensitivityY / 60 * cmra.GetComponent<Camera>().fieldOfView);

            // Clamp Y axis rotation
            rotationY = ClampAngle(rotationY, minimumY, maximumY);

            // Average out mouse speed to smooth rotation for less jitter
            rotAverageY = 0.0f;
            rotArrayY.Add(rotationY);

            if (rotArrayY.Count >= frameCounter)
            {
                rotArrayY.RemoveAt(0);
            }

            for (int j = 0; j < rotArrayY.Count; j++)
            {
                rotAverageY += rotArrayY[j];
            }
            rotAverageY /= rotArrayY.Count;

            // Apply rotation
            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);
            transform.localRotation = originalRotation * yQuaternion;
        }

    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
