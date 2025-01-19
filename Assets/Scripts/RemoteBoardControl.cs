using System;
using System.Collections.Generic;
using UnityEngine;

public class RemoteBoardControl : MonoBehaviour
{
    public int nrOldPositions;
    public float remoteControlSpeed;

    public GameObject teleRollingBoard;
    private readonly Queue<Tuple<Vector3, Vector3>> oldPositions = new();

    private TRBScript TrbScript => teleRollingBoard.GetComponent<TRBScript>();

    void Update()
    {
        //only act if grabbing with left hand without contact
        if (!TrbScript.IsGrabbingRight && (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > .1f
                                        || OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > .1f))
        {
            Vector3 movement = (transform.position - oldPositions.Peek().Item1) * remoteControlSpeed;
            if (movement.magnitude > TrbScript.maxSpeed) movement = movement.normalized * TrbScript.maxSpeed;
            Rigidbody rb = teleRollingBoard.GetComponent<Rigidbody>();
            rb.AddForce(movement, ForceMode.VelocityChange);
            rb.angularVelocity = (transform.eulerAngles - oldPositions.Peek().Item2) / nrOldPositions;
        }
        else oldPositions.Clear();
    }

    private void FixedUpdate()
    {
        //update position queue (don't care about delta)
        oldPositions.Enqueue(Tuple.Create(transform.position, transform.eulerAngles));
        if (oldPositions.Count > nrOldPositions) oldPositions.Dequeue();
    }
}
