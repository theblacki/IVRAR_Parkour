using System;
using System.Collections.Generic;
using UnityEngine;

public class RemoteBoardControl : MonoBehaviour
{
    public int nrOldPositions;
    public float remoteControlSpeed;

    public GameObject teleRollingBoard;
    private readonly Queue<Vector3> oldRotations = new();
    private Vector3 initialPosition = new();
    private bool initialSet = false;

    private TRBScript TrbScript => teleRollingBoard.GetComponent<TRBScript>();

    void Update()
    {
        //only act if grabbing with left hand without contact
        if (!TrbScript.IsGrabbingLeft && (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) > .1f
                                        || OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > .1f))
        {
            if (!initialSet)
            {
                initialSet = true;
                initialPosition = transform.position;
            }
            Vector3 movement = (transform.position - initialPosition) * remoteControlSpeed;
            if (movement.magnitude > TrbScript.maxSpeed * TrbScript.speedSlider.sliderValue) movement = TrbScript.maxSpeed * TrbScript.speedSlider.sliderValue * movement.normalized;
            Rigidbody rb = teleRollingBoard.GetComponent<Rigidbody>();
            rb.AddForce(movement, ForceMode.VelocityChange);
            rb.angularVelocity = (transform.eulerAngles - oldRotations.Peek()) * remoteControlSpeed / nrOldPositions;
        }
        else
        {
            initialSet = false;
            oldRotations.Clear();
        }
    }

    private void FixedUpdate()
    {
        //update position queue (don't care about delta)
        oldRotations.Enqueue(transform.eulerAngles);
        if (oldRotations.Count > nrOldPositions) oldRotations.Dequeue();
    }
}
