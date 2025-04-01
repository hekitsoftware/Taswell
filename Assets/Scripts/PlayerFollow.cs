using UnityEngine;

public class PlayerFollow : MonoBehaviour
{
    public Camera _cam;
    public GameObject _target;

    public Vector3 _camPosition;
    public Vector3 _targetPosition;

    public bool _allowedToFollow = false;
    public bool _followX = false;

    public float smoothSpeed = 0.125f; // Speed at which the camera follows the target, lower values = smoother follow

    private void Update()
    {
        _targetPosition = _target.transform.position;
        _camPosition = _cam.transform.position;

        // Call the methods to follow the target with smoothing

        if (Input.GetKeyDown(KeyCode.F1)) { _allowedToFollow = false; }
        if (Input.GetKeyDown(KeyCode.F2)) { _allowedToFollow = true; _followX = false; }
        if (Input.GetKeyDown(KeyCode.F3)) { _allowedToFollow = true; _followX = true; }

        Follow();
    }

    public void FollowX()
    {
        // Smoothly interpolate the camera's X position
        float smoothX = Mathf.Lerp(_camPosition.x, _targetPosition.x, smoothSpeed);
        _cam.transform.position = new Vector3(smoothX, _camPosition.y, _camPosition.z);
    }

    public void FollowY()
    {
        // Smoothly interpolate the camera's Y position
        float smoothY = Mathf.Lerp(_camPosition.y, _targetPosition.y, smoothSpeed);
        _cam.transform.position = new Vector3(_camPosition.x, smoothY, _camPosition.z);
    }

    public void Follow()
    {
        if (_allowedToFollow)
        {
            FollowY();

            if (_followX)
            {
                FollowY();
                FollowX();
            }
        }
    }
}
