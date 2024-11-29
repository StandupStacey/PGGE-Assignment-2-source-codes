using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PGGE;

public class PlayerMovement : MonoBehaviour
{
    [HideInInspector]
    public CharacterController mCharacterController;
    public Animator mAnimator;
    public Animator mAnimator1;

    public float mWalkSpeed = 1.5f;
    public float mRotationSpeed = 50.0f;
    public bool mFollowCameraForward = false;
    public float mTurnRate = 10.0f;

#if UNITY_ANDROID
    public FixedJoystick mJoystick;
#endif

    private float hInput;
    private float vInput;
    private float speed;
    private bool jump = false;
    private bool crouch = false;
    public float mGravity = -30.0f;
    public float mJumpHeight = 1.0f;

    public AudioSource footstepAudioSource; // AudioSource For footstep sounds
    public AudioClip[] footstepSounds; // Array of footstep sounds
    public float stepRate = 0.8f; // Time between steps
    private float nextStepTime = 0.0f; // Next time to play a step sound
    private bool footstepWalk = false; // Flag to check if the player is walking

    private Vector3 mVelocity = new Vector3(0.0f, 0.0f, 0.0f);

    void Start()
    {
        mCharacterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        //HandleInputs();
        //Move();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    public void HandleInputs()
    {
        // We shall handle our inputs here.
    #if UNITY_STANDALONE
        hInput = Input.GetAxis("Horizontal");
        vInput = Input.GetAxis("Vertical");
    #endif

    #if UNITY_ANDROID
        hInput = 2.0f * mJoystick.Horizontal;
        vInput = 2.0f * mJoystick.Vertical;
    #endif

        speed = mWalkSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = mWalkSpeed * 2.0f;
            stepRate = 0.4f;
            mAnimator1.SetBool("Run", true);
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            stepRate = 0.8f;
            mAnimator1.SetBool("Run", false);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jump = true;
            mAnimator.SetTrigger("Jump");
            mAnimator1.SetTrigger("Jump");
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            jump = false;
            mAnimator1.SetBool("Jump", false);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Concrete"))
        {
            if (footstepWalk && Time.time >= nextStepTime)
            {
                PlayFootstepSoundConcrete();
                nextStepTime = Time.time + stepRate;
            }
        }
        else if (other.gameObject.CompareTag("Grass"))
        {
            if (footstepWalk && Time.time >= nextStepTime)
            {
                PlayFootstepSoundGrass();
                nextStepTime = Time.time + stepRate;
            }
        }
        speed = speed * -1;
    }

    public void Move()
    {
        if (crouch) return;

        // We shall apply movement to the game object here.
        if (mAnimator == null) return;
        if (mFollowCameraForward)
        {
            // rotate Player towards the camera forward.
            Vector3 eu = Camera.main.transform.rotation.eulerAngles;
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                Quaternion.Euler(0.0f, eu.y, 0.0f),
                mTurnRate * Time.deltaTime);
        }
        else
        {
            transform.Rotate(0.0f, hInput * mRotationSpeed * Time.deltaTime, 0.0f);
        }

        Vector3 forward = transform.TransformDirection(Vector3.forward).normalized;
        forward.y = 0.0f;

        mCharacterController.Move(forward * vInput * speed * Time.deltaTime);
        mAnimator.SetFloat("PosX", 0);
        mAnimator.SetFloat("PosZ", vInput * speed / (2.0f * mWalkSpeed));

        if(jump)
        {
            Jump();
            jump = false;
        }

        if (Input.GetKey(KeyCode.W))
        {
            footstepWalk = true;
            mAnimator1.SetFloat("Speed", 1);
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            footstepWalk = false;
            mAnimator1.SetFloat("Speed", 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            footstepWalk = true;
            mAnimator1.SetFloat("Speed", -1);
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            footstepWalk = false;
            mAnimator1.SetFloat("Speed", 0);
        }
        if (Input.GetKey(KeyCode.C))
        {
            mAnimator1.SetBool("Slide", true);
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            mAnimator1.SetBool("Slide", false);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            jump = true;
            mAnimator1.SetBool("Hop", true);
        }
        if (Input.GetKeyUp(KeyCode.Q))
        {
            jump = false;
            mAnimator1.SetBool("Hop", false);
        }
        if (Input.GetKey(KeyCode.E))
        {
            mAnimator1.SetBool("Win", true);
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            mAnimator1.SetBool("Win", false);
        }
        if (Input.GetKey(KeyCode.F))
        {
            mAnimator1.SetBool("Lose", true);
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            mAnimator1.SetBool("Lose", false);
        }
    }

    void Jump()
    {
        mVelocity.y += Mathf.Sqrt(mJumpHeight * -2f * mGravity);
    }

    private Vector3 HalfHeight;
    private Vector3 tempHeight;
    void Crouch()
    {
        mAnimator.SetBool("Crouch", crouch);
        if(crouch)
        {
            tempHeight = CameraConstants.CameraPositionOffset;
            HalfHeight = tempHeight;
            HalfHeight.y *= 0.5f;
            CameraConstants.CameraPositionOffset = HalfHeight;
        }
        else
        {
            CameraConstants.CameraPositionOffset = tempHeight;
        }
    }

    void ApplyGravity()
    {
        // apply gravity.
        mVelocity.y += mGravity * Time.deltaTime;
        if (mCharacterController.isGrounded && mVelocity.y < 0)
            mVelocity.y = 0f;
    }

    private void PlayFootstepSoundConcrete()
    {
        // choose a random footstep sound from the array
        AudioClip clip = footstepSounds[Random.RandomRange(0, 4)];
        footstepAudioSource.PlayOneShot(clip); // Play the selected sound
    }
    private void PlayFootstepSoundGrass()
    {
        // choose a random footstep sound from the array
        AudioClip clip = footstepSounds[Random.RandomRange(4, 8)];
        footstepAudioSource.PlayOneShot(clip); // Play the selected sound
    }
}
