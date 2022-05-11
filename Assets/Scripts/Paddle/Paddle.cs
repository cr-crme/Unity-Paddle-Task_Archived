using UnityEngine;
using Unity.Labs.SuperScience;

public class Paddle : MonoBehaviour
{
    // The collider for this paddle
    public bool isActive { get; private set; } = false;
    [SerializeField]
    private GameObject activeModel;
    [SerializeField]
    private GameObject inactiveModel;
    private PhysicsTracker m_MotionData = new PhysicsTracker();

    // Enable this paddle. Make it visible and turn on collider
    public void EnablePaddle()
    {
        isActive = true;
        activeModel.SetActive(true);
        inactiveModel.SetActive(false);

        // Use UnityLabs PhysicsTracker
        m_MotionData.Reset(transform.position, transform.rotation, Vector3.zero, Vector3.zero);
    }

    // Disable this paddle. Make it transparent and turn off collider.
    public void DisablePaddle()
    {
        isActive = false;
        activeModel.SetActive(false);
        inactiveModel.SetActive(true);
    }

    static public Paddle GetPaddleFromCollider(Collision c)
    {
        return c.gameObject.transform.parent.transform.parent.GetComponent<Paddle>();
    }

    void Update()
    {
        // send updated information to physicstracker
        m_MotionData.mUpdate(transform.position, transform.rotation, Time.smoothDeltaTime);
    }


    public Vector3 Position { get { return transform.position; } }
    public Vector3 Velocity { get { return m_MotionData.Velocity; } }
    public Vector3 Acceleration { get { return m_MotionData.Acceleration; } }
}

