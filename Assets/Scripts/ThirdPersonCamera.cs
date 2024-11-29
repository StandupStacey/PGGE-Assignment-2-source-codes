using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PGGE;

public enum CameraType
{
    Track,// Tracks the player's movement
    Follow_Track_Pos,// Follows the player's position only
    Follow_Track_Pos_Rot,// Follows both position and rotation of the player
    Topdown, // Top-down view of the player
    Follow_Independent, // Independent camera movement with rotation
}

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform mPlayer;

    TPCBase mThirdPersonCamera;
    // Position and rotation offsets for the camera
    public Vector3 mPositionOffset = new Vector3(0.0f, 2.0f, -2.5f);
    public Vector3 mAngleOffset = new Vector3(0.0f, 0.0f, 0.0f);
    [Tooltip("The damping factor to smooth the changes in position and rotation of the camera.")]
    public float mDamping = 1.0f;

    // Controls the camera's pitch
    public float mMinPitch = -30.0f;
    public float mMaxPitch = 30.0f;

    public float mRotationSpeed = 50.0f;
    public FixedTouchField mTouchField;

    // Set camera type
    public CameraType mCameraType = CameraType.Follow_Track_Pos;
    Dictionary<CameraType, TPCBase> mThirdPersonCameraDict = new Dictionary<CameraType, TPCBase>();

    public Transform player;
    public LayerMask obstacleLayer;// To detect obstacles
    public float sphereRadius = 0.5f;
    public float verticalOffset = 2.0f;
    public float distanceBehindPlayer = 3.0f;

    void Start()
    {
        // Set to CameraConstants class so that other objects can use.
        CameraConstants.Damping = mDamping;
        CameraConstants.CameraPositionOffset = mPositionOffset;
        CameraConstants.CameraAngleOffset = mAngleOffset;
        CameraConstants.MinPitch = mMinPitch;
        CameraConstants.MaxPitch = mMaxPitch;
        CameraConstants.RotationSpeed = mRotationSpeed;

        mThirdPersonCameraDict.Add(CameraType.Track, new TPCTrack(transform, mPlayer));
        mThirdPersonCameraDict.Add(CameraType.Follow_Track_Pos, new TPCFollowTrackPosition(transform, mPlayer));
        mThirdPersonCameraDict.Add(CameraType.Follow_Track_Pos_Rot, new TPCFollowTrackPositionAndRotation(transform, mPlayer));
        mThirdPersonCameraDict.Add(CameraType.Topdown, new TPCTopDown(transform, mPlayer));


        // We instantiate and add the new third-person camera to the dictionary
#if UNITY_STANDALONE
        mThirdPersonCameraDict.Add(CameraType.Follow_Independent, new TPCFollowIndependentRotation(transform, mPlayer));
#endif
#if UNITY_ANDROID
        mThirdPersonCameraDict.Add(CameraType.Follow_Independent, new TPCFollowIndependentRotation(transform, mPlayer, mTouchField));
#endif

        mThirdPersonCamera = mThirdPersonCameraDict[mCameraType];

    }

    private void Update()
    {
        // Update the game constant parameters every frame 
        // so that changes applied on the editor can be reflected
        CameraConstants.Damping = mDamping;
        //CameraConstants.CameraPositionOffset = mPositionOffset;
        CameraConstants.CameraAngleOffset = mAngleOffset;
        CameraConstants.MinPitch = mMinPitch;
        CameraConstants.MaxPitch = mMaxPitch;
        CameraConstants.RotationSpeed = mRotationSpeed;

        mThirdPersonCamera = mThirdPersonCameraDict[mCameraType];
    }

    void LateUpdate()
    {
        mThirdPersonCamera.Update();
        RepositionCamera();// Adjust the camera's position to avoid obstacles
    }

    void RepositionCamera()
    {
        Vector3 desiredPosition = player.position - player.forward * distanceBehindPlayer + Vector3.up * verticalOffset;

        // Perform a sphere cast to detect obstacles
        RaycastHit hit;
        Vector3 directionToDesiredPosition = (desiredPosition - player.position).normalized;
        float distanceToDesiredPosition = Vector3.Distance(player.position, desiredPosition);

        
        if (Physics.SphereCast(player.position, sphereRadius, directionToDesiredPosition, out hit, distanceToDesiredPosition, obstacleLayer))
        {
            //Adjust the camera position to avoid clipping into the obstacle
            Vector3 hitPosition = hit.point + hit.normal * sphereRadius;
            desiredPosition = new Vector3(hitPosition.x, desiredPosition.y, hitPosition.z);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, mDamping * Time.deltaTime);
    }
}
