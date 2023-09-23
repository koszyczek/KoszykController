using FlaxEngine;
using System;

public class HeadBob : Script
{
    private Vector3 _originalPosition;
    private float _timer = 0;
    private int _lastPlayedSoundIndex = -1; // To prevent playing the same sound twice in a row
    private System.Random _random = new System.Random(); // For generating random numbers

    // Adjust these values to fit your needs
    public float BobbingSpeed = 0.18f;
    public float SprintBobbingSpeed = 0.25f; // New variable for sprinting bobbing speed
    public float BobbingAmount = 0.2f;
    public AudioClip[] BobSounds; 
    public AudioClip JumpSound; // The AudioClip to play when jumping
    public AudioClip LandingSound; // The AudioClip to play when landing
    public AudioSource audio;// The array of AudioClips to play on each bob
    public AudioSource LandingAudio; // The AudioSource to play the landing sound

    

    private AudioSource _audioSource; // The AudioSource component
    public CharacterController _characterController; // Reference to the CharacterController
    private bool _wasInAir = false; // To track if the player was in the air in the last frame

    private Quaternion _originalRotation; // The original rotation of the camera

    /// <inheritdoc />
    public override void OnStart()
    {
        // Store the original position of the camera (or object) this script is attached to
        _originalPosition = Actor.LocalPosition;

        // Get or add the AudioSource component
        _audioSource = audio;

        // Get the CharacterController component

        // Store the original rotation of the camera
        _originalRotation = Actor.Orientation;
    }

    /// <inheritdoc />
    public override void OnUpdate()
    {
        float waveslice = 0.0f;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 localPosition = Actor.LocalPosition;

        if (Mathf.Abs(horizontal) == 0 && Mathf.Abs(vertical) == 0)
        {
            _timer = 0.0f;
        }
        else
        {
            waveslice = Mathf.Sin(_timer);
            _timer = _timer + (IsSprinting() ? SprintBobbingSpeed : BobbingSpeed) * Time.DeltaTime; // Use SprintBobbingSpeed if sprinting
            if (_timer > Mathf.Pi * 2)
            {
                _timer = _timer - (Mathf.Pi * 2);

                // Only play the bobbing sound if the player is grounded
                if (_characterController.IsGrounded && BobSounds.Length > 0)
                {
                    int randomIndex;
                    do
                    {
                        randomIndex = _random.Next(BobSounds.Length);
                    } while (randomIndex == _lastPlayedSoundIndex); // Ensure we don't play the same sound twice
                    _lastPlayedSoundIndex = randomIndex;

                    // Set the selected sound as the source
                    _audioSource.Clip = BobSounds[randomIndex];

                    // Play the sound
                    _audioSource.Play();
                }
            }
        }

        if (waveslice != 0)
        {
            float translateChange = waveslice * BobbingAmount;
            float totalAxes = Mathf.Abs(horizontal) + Mathf.Abs(vertical);
            totalAxes = Mathf.Clamp(totalAxes, 0.0f, 1.0f);
            translateChange = totalAxes * translateChange;

            // Apply the bobbing effect
            localPosition.Y = _originalPosition.Y + translateChange;
        }
        else
        {
            localPosition.Y = _originalPosition.Y;
        }

        Actor.LocalPosition = localPosition;

        // Play the landing sound if the player just landed
        if (_wasInAir && _characterController.IsGrounded)
        {
            LandingAudio.Clip = LandingSound;
            LandingAudio.Play();
        }

        // Play the jumping sound if the player is jumping
        if (IsJumping() && _characterController.IsGrounded)
        {
            _audioSource.Clip = JumpSound;
            _audioSource.Play();
        }

        // Remember if the player is in the air for the next frame
        _wasInAir = !_characterController.IsGrounded;

        // Calculate the lean angle based on the player's movement direction
        
    }

    private bool IsSprinting()
    {
        return Input.GetAction("Sprint");
    }

    private bool IsJumping()
    {
        return Input.GetAction("Jump");
    }
}
