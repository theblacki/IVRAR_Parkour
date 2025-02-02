using UnityEngine;

public class OriginalGameMechanics : MonoBehaviour
{
    public GameObject hmd;
    public ParkourCounter parkourCounter;
    public string stage;
    public SelectionTaskMeasure selectionTaskMeasure;
    public TRBScript trbs;

    void OnTriggerEnter(Collider other)
    {
        HandleCollision(other);
    }

    public void HandleCollision(Collider other, bool playCollectCoinSound = true)
    {
        if (other.CompareTag("banner"))
        {
            stage = other.gameObject.name;
            parkourCounter.isStageChange = true;
            Debug.LogWarning("Now entering stage " + stage);
        }
        else if (other.CompareTag("objectInteractionTask"))
        {
            selectionTaskMeasure.scoreText.text = "";
            selectionTaskMeasure.partSumErr = 0f;
            selectionTaskMeasure.partSumTime = 0f;
            float tempValueY = other.transform.position.y > 0 ? 12 : 0;
            Vector3 tmpTarget = new(hmd.transform.position.x, tempValueY, hmd.transform.position.z);
            selectionTaskMeasure.taskUI.transform.LookAt(tmpTarget);
            selectionTaskMeasure.taskUI.transform.Rotate(new Vector3(0, 180f, 0));
            selectionTaskMeasure.taskStartPanel.SetActive(true);
            trbs.TeleportPlayer(selectionTaskMeasure.taskUI.transform.position);
            trbs.gameObject.transform.parent.gameObject.SetActive(false);
            Debug.LogWarning("Now starting selection task");
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("coin"))
        {
            parkourCounter.coinCount += 1;
            if (playCollectCoinSound)
                GetComponent<AudioSource>().Play();
            other.gameObject.SetActive(false);
        }
    }
}