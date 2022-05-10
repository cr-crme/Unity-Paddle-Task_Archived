using UnityEngine;
using Unity.Labs.SuperScience;

public class Paddle : MonoBehaviour
{
    public enum PaddleIdentifier {LEFT, RIGHT};

    // Is this the left paddle or the right paddle?
    private PaddleIdentifier paddleIdentifier;

    // The collider for this paddle
    [SerializeField]
    private GameObject paddleCollider;
    private PhysicsTracker m_MotionData = new PhysicsTracker();

    // Enable this paddle. Make it visible and turn on collider
    public void EnablePaddle()
    {
        paddleCollider.SetActive(true);

        // Use UnityLabs PhysicsTracker
        m_MotionData.Reset(transform.position, transform.rotation, Vector3.zero, Vector3.zero);
    }
    static public Paddle GetPaddleFromCollider(Collision c)
    {
        return c.gameObject.transform.parent.transform.parent.GetComponent<Paddle>();
    }

    private void Start()
    {
        paddleCollider = paddleCollider.gameObject;
    }

    void Update()
    {
        // send updated information to physicstracker
        m_MotionData.mUpdate(transform.position, transform.rotation, Time.smoothDeltaTime);
    }

    // Disable this paddle. Make it transparent and turn off collider.
    public void DisablePaddle()
    {
        paddleCollider.SetActive(false);
    }

    public Vector3 Position { get { return transform.position; } }
    public Vector3 Velocity { get { return m_MotionData.Velocity; } }
    public Vector3 Acceleration { get { return m_MotionData.Acceleration; } }

    // Is the collider on this paddle active?
    public bool ColliderIsActive()
    {
        return paddleCollider.activeInHierarchy;
    }

    // Set up this paddle as the left or right paddle
    public void SetPaddleIdentifier(PaddleIdentifier paddleId)
    {
        paddleIdentifier = paddleId;
    }

    public PaddleIdentifier GetPaddleIdentifier()
    {
        return paddleIdentifier;
    }
}

