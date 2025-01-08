using Oculus.Interaction;
using System.Collections.Generic;
using UnityEngine;

public class TRBScript : MonoBehaviour
{
    public float throwingSpeed;
    public float maxSpeed;
    public float gravityMultiplier;
    public float maxRange;
    public int nrOldPositions;
    public Transform leftHandTransform;

    public GameObject Player;

    private static readonly string playerTag = "Player";
    private Rigidbody rb;
    private readonly Queue<Vector3> oldPositions = new();
    private bool isCurrentlyGrabbed;
    
    private bool IsGrabbing => (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) > .1f
                            || OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > .1f)
                            && true;//object is in hand - via collider?
    private Transform Transform => GetComponent<Transform>();

    void Start()
    {
        Player = GameObject.FindGameObjectWithTag(playerTag);
        rb = GetComponent<Rigidbody>();
        rb.maxLinearVelocity = maxSpeed;
    }

    void Update()
    {
        //calculate control inputs - grab, teleport, return
        bool letGo = isCurrentlyGrabbed && !IsGrabbing;
        isCurrentlyGrabbed = IsGrabbing;
        bool doTeleport = OVRInput.GetDown(OVRInput.Button.One);
        bool doReturn = OVRInput.Get(OVRInput.Button.Three);



        //resolve let go
        if (letGo)
        {
            Vector3 movement = (transform.position - oldPositions.Peek()) * throwingSpeed;
            if (movement.magnitude > maxSpeed) movement = movement.normalized * maxSpeed;
            Debug.LogWarning("Throw with movement:" + movement.ToString());
            rb.AddForce(movement, ForceMode.VelocityChange);
            //rb.angularVelocity = rotationfromtransformaswithmovement
        }

        //resolve return to hand
        if (doReturn)
        {
            transform.position = leftHandTransform.position + leftHandTransform.right * .1f;
        }

        //resolve teleport player
        if (doTeleport)
        {
            Player.transform.position = transform.position + new Vector3(0, -.04f, 0);
            //maybe put the board into the right hand
        }
    }

    private void FixedUpdate()
    {
        if (!IsGrabbing)
        {
            rb.AddForce(Vector3.down * gravityMultiplier, ForceMode.Force);
        }
        //update position queue (don't care about delta)
        oldPositions.Enqueue(Transform.position);
        if (oldPositions.Count > nrOldPositions) oldPositions.Dequeue();
    }
}
