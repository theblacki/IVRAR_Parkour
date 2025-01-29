using UnityEngine;

public class CoinCollection : MonoBehaviour
{
    public ParkourCounter parkourCounter;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("coin"))
        {
            parkourCounter.coinCount += 1;
            GetComponent<AudioSource>().Play();
            other.gameObject.SetActive(false);
        }
    }
}
