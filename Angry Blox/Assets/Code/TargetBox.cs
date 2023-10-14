using UnityEngine;

public class TargetBox : MonoBehaviour
{
    /// <summary>
    /// Targets that move past this point score automatically.
    /// </summary>
    public static float OffScreen;

    private bool alreadyScored = false;

    internal void Start() {
        OffScreen = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width-100, 0, 0)).x;
    }

    internal void Update()
    {
        if (transform.position.x > OffScreen)
            Scored();
    }

    private void Scored()
    {
        if (!alreadyScored)
        {
            var boxSR = gameObject.GetComponent<SpriteRenderer>();
            boxSR.color = Color.green;

            var boxRB = gameObject.GetComponent<Rigidbody2D>();
            ScoreKeeper.AddToScore(boxRB.mass);

            alreadyScored = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            Scored();
        }
    }
}
