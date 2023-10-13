using UnityEngine;

/// <summary>
/// Control the player on screen
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    /// <summary>
    /// Prefab for the orbs we will shoot
    /// </summary>
    public GameObject OrbPrefab;

    /// <summary>
    /// How fast our engines can accelerate us
    /// </summary>
    public float EnginePower = 1;
    
    /// <summary>
    /// How fast we turn in place
    /// </summary>
    public float RotateSpeed = 1;

    /// <summary>
    /// How fast we should shoot our orbs
    /// </summary>
    public float OrbVelocity = 10;

    /// <summary>
    /// Rigidbody2D component that we apply physics to
    /// </summary>
    public Rigidbody2D rigidBody;

    /// <summary>
    /// The number of orbs to shoot out every physics update
    /// </summary>
    public int fireRate = 1;

    /// <summary>
    /// Store the RigidBody2D component of the player game object
    /// </summary>
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Handle moving and firing.
    /// Called by Uniity every 1/50th of a second, regardless of the graphics card's frame rate
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    void FixedUpdate()
    {
        Manoeuvre();
        MaybeFire();
    }

    /// <summary>
    /// Fire if the player is pushing the button for the Fire axis
    /// Unlike the Enemies, the player has no cooldown, so they shoot a whole blob of orbs
    /// </summary>
    void MaybeFire()
    {
        if (Input.GetAxis("Fire") > 0)
        {
            for (int i = 0; i < fireRate; i++)
            {
                FireOrb();
            }
        }
    }

    /// <summary>
    /// Fire one orb.  The orb should be placed one unit "in front" of the player.
    /// transform.right will give us a vector in the direction the player is facing.
    /// It should move in the same direction (transform.right), but at speed OrbVelocity.
    /// </summary>
    private void FireOrb()
    {
        var orb = Instantiate(OrbPrefab, transform.position + transform.right, Quaternion.identity);
        var orbRB = orb.GetComponent<Rigidbody2D>();
        orbRB.velocity = OrbVelocity * transform.right;
    }

    /// <summary>
    /// Accelerate and rotate as directed by the player
    /// Apply a force in the direction (Horizontal, Vertical) with magnitude EnginePower
    /// Note that this is in *world* coordinates, so the direction of our thrust doesn't change as we rotate
    /// Set our angularVelocity to the Rotate axis time RotateSpeed
    /// </summary>
    void Manoeuvre()
    {
        float X = Input.GetAxis("Horizontal");
        float Y = Input.GetAxis("Vertical");
        rigidBody.AddForce(new Vector2(X, Y) * EnginePower);

        float rot = Input.GetAxis("Rotate");
        rigidBody.angularVelocity = rot * RotateSpeed;
    }

    /// <summary>
    /// If this is called, we got knocked off screen.  Deduct a point!
    /// </summary>
    // ReSharper disable once UnusedMember.Local
    void OnBecameInvisible()
    {
        ScoreKeeper.ScorePoints(-1);
    }
}
