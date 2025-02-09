using UnityEngine;

public class HandGameEvents : MonoBehaviour
{
    public SelectionTaskMeasure selectionTaskMeasure;
    public OriginalGameMechanics ogm;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("selectionTaskStart") && !selectionTaskMeasure.isCountdown)
        {
            selectionTaskMeasure.isTaskStart = true;
            selectionTaskMeasure.StartOneTask();
        }
        else if (other.gameObject.CompareTag("done"))
        {
            selectionTaskMeasure.isTaskStart = false;
            selectionTaskMeasure.EndOneTask();
        }
        else
        {
            ogm.HandleCollision(other);
        }
    }
}