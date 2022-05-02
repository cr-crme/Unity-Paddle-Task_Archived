﻿using UnityEngine;
using Unity.Labs.SuperScience;

public class Paddle : MonoBehaviour
{
    public enum PaddleIdentifier {LEFT, RIGHT};

    // Is this the left paddle or the right paddle?
    private PaddleIdentifier paddleIdentifier;

    // The materials used to display this paddle. If it is transparent,
    // disable the collider.
    [SerializeField]
    private Material opaquePaddleMat;
    [SerializeField]
    private Material opaqueBacksideMat;
    [SerializeField]
    private Material transparentPaddleMat;
    [SerializeField]
    private Material transparentBacksideMat;

    // The collider for this paddle
    [SerializeField]
    private GameObject paddleCollider;

    // The mesh renderers for this paddle. Modify the material that they use.
    [SerializeField]
    private GameObject paddleModel;
    [SerializeField]
    private GameObject backsideModel;

    private PhysicsTracker m_MotionData = new PhysicsTracker();

    // Enable this paddle. Make it visible and turn on collider
    public void EnablePaddle()
    {
        paddleCollider.SetActive(true);

        paddleModel.GetComponent<MeshRenderer>().material = opaquePaddleMat;
        backsideModel.GetComponent<MeshRenderer>().material = opaqueBacksideMat;

        // Use UnityLabs PhysicsTracker
        m_MotionData.Reset(transform.position, transform.rotation, Vector3.zero, Vector3.zero);
    }
    static public Paddle GetPaddleFromCollider(Collision c)
    {
        return c.gameObject.transform.parent.transform.parent.GetComponent<Paddle>();
    }

    private void Start()
    {
        opaquePaddleMat = GetComponent<Material>();
        opaqueBacksideMat = GetComponent<Material>();
        transparentPaddleMat = GetComponent<Material>();
        transparentBacksideMat = GetComponent<Material>();
        paddleCollider = paddleCollider.gameObject;
        paddleModel = paddleModel.gameObject;
        backsideModel = backsideModel.gameObject;   
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

        paddleModel.GetComponent<MeshRenderer>().material = transparentPaddleMat;
        backsideModel.GetComponent<MeshRenderer>().material = transparentBacksideMat;
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

