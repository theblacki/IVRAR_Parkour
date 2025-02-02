using UnityEngine;

public class BoardAsPlayer : MonoBehaviour
{
    public OriginalGameMechanics ogm;

    private void OnTriggerEnter(Collider other)
    {
        bool playCollectCoinSound = true;
        if (other.CompareTag("coin"))
        {
            playCollectCoinSound = false;
            GetComponent<AudioSource>().Play();
        }
        ogm.HandleCollision(other, playCollectCoinSound);
    }
}
