﻿using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Control code for the the player's game object.
/// Very approximate simulation of flight dynamics.
/// </summary>
public class PlayerControl : MonoBehaviour {
    /// <summary>
    /// Coefficient of draft for head winds
    /// </summary>
    [Header("Aerodynamic coefficients")]
    public float ForwardDragCoefficient = 0.01f;
    /// <summary>
    /// Drag coefficient for winds blowing up/down across wings
    /// </summary>
    public float VerticalDragCoefficient = 0.5f;
    /// <summary>
    /// Lift generated by the wings
    /// </summary>
    public float LiftCoefficient = 0.01f;

    /// <summary>
    /// How far the plane can tilt around the X axis
    /// </summary>
    [Header("Movement Speeds")]
    public float PitchRange = 45f;
    /// <summary>
    /// How far the plane can rotate about the Z axis
    /// </summary>
    public float RollRange = 45f;
    /// <summary>
    /// Amount to lerp on each fixed update between desired roll and current roll
    /// </summary>
    public float LerpWeight = 0.05f;
    /// <summary>
    /// How fast the plane yaws for a given degree of roll.
    /// </summary>
    public float RotationalSpeed = 0.02f;
    /// <summary>
    /// Thrust generated when the throttle is pulled back all the way.
    /// </summary>
    public float MaximumThrust = 20f;

    /// <summary>
    /// Text element for displaying status information
    /// </summary>
    [Header("HUD")]
    public Text StatusDisplay;
    /// <summary>
    /// Text element for displaying game-over text
    /// </summary>
    public Text GameOverText;

    /// <summary>
    /// Cached copy of the player's RigidBody component
    /// </summary>
    private Rigidbody playerRB;




    /// <summary>
    /// Magic layer mask code for the updraft(s)
    /// </summary>
    const int UpdraftLayerMask = 1 << 8;

    #region Internal flight state
    /// <summary>
    /// Current yaw (rotation about the Y axis)
    /// </summary>
    private float yaw;
    /// <summary>
    /// Current pitch (rotation about the X axis)
    /// </summary>
    private float pitch;
    /// <summary>
    /// Current roll (rotation about the Z axis)
    /// </summary>
    private float roll;
    /// <summary>
    /// Current thrust (forward force provided by engines
    /// </summary>
    private float thrust;
    #endregion

    /// <summary>
    /// Since physics is calculated before OnCollisionEnter, we need to store the previous velocity!
    /// </summary>
    private Vector3 lastVelocity;

    /// <summary>
    /// Initialize component
    /// </summary>
    internal void Start() {
        playerRB = GetComponent<Rigidbody>();
        playerRB.velocity = transform.forward*3;
    }

    /// <summary>
    /// Show game-over display
    /// </summary>
    /// <param name="safe">True if we won, false if we crashed</param>
    private void OnGameOver(bool safe) {
        playerRB.velocity = Vector3.zero;
        playerRB.useGravity = false;
        playerRB.constraints = RigidbodyConstraints.FreezeAll;
        if (safe) {
            GameOverText.text = "You Win!";
        } else {
            GameOverText.text = "OOPS";
        }
    }

    /// <summary>
    /// Display status information
    /// </summary>
    internal void OnGUI()
    {
        StatusDisplay.text = string.Format("Speed: {0:00.00}    altitude: {1:00.00}    Thrust {2:0.0}",
            playerRB.velocity.magnitude,
            transform.position.y,
            thrust);
    }

    void FixedUpdate()
    {
        // Get current pitch and roll inputs
        var joystickRoll = Input.GetAxis("Horizontal") * RollRange;
        var joystickPitch = Input.GetAxis("Vertical") * PitchRange;

        // Lerp between current and desired inputs with LerpWeight on each update
        roll = Mathf.Lerp(roll, joystickRoll, LerpWeight);
        pitch = Mathf.Lerp(pitch, joystickPitch, LerpWeight);
        yaw -= roll * RotationalSpeed;

        // Apply current orientation
        playerRB.MoveRotation(Quaternion.Euler(pitch, yaw, roll));

        // Get the thrust input, ignore negative values
        thrust = Input.GetAxis("Thrust") * MaximumThrust;
        if (thrust < 0f)
        {
            thrust = 0f;
        }

        // Add thrust force
        playerRB.AddForce(transform.forward * thrust);

        // forward component of relative velocity
        var vf = Vector3.Dot(playerRB.velocity, transform.forward);

        // vertical component of wind velocity
        var vup = Vector3.Dot(playerRB.velocity, transform.up);

        // Add lift
        playerRB.AddForce(LiftCoefficient * vf * vf * transform.up);

        // If you are in an updraft...
        Collider[] updrafts = Physics.OverlapSphere(transform.position, transform.localScale.x, UpdraftLayerMask);
        if (updrafts.Length > 0)
        {
            // update the vertical component of wind with draft's velocity
            Vector3 draft = updrafts[0].GetComponent<Updraft>().WindVelocity;
            vup -= draft.y;
        }

        // Add forward and vertical drag
        playerRB.AddForce(-Mathf.Sign(vf) * ForwardDragCoefficient * vf * vf * transform.forward);
        playerRB.AddForce(-Mathf.Sign(vup) * VerticalDragCoefficient * vup * vup * transform.up);

        // update last velocity so we know the velocity before collisions
        lastVelocity = playerRB.velocity;
    }

    void OnCollisionEnter(Collision col)
    {
        // Ignore collisions with targets and landing platforms
        if (col.gameObject.GetComponent<Target>() == null && col.gameObject.GetComponent<LandingPlatform>() == null)
        {
            // This means we hit a scene object that is not the landing pad -- game over
            OnGameOver(false);
        }
        
        // If we collide with landing platform, 
        if (col.gameObject.GetComponent<LandingPlatform>() != null)
        {
            var maxLandingSpeed = col.gameObject.GetComponent<LandingPlatform>().MaxLandingSpeed;

            // determine if we should win/lose depending on speed we hit the platform with
            if (lastVelocity.magnitude > maxLandingSpeed)
            {
                OnGameOver(false);
            }
            else
            {
                OnGameOver(true);
            }
        }
    }
}
