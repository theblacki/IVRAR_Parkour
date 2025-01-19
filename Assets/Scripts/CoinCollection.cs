using UnityEngine;

public class CoinCollection : MonoBehaviour
{
    public ParkourCounter parkourCounter;
    private void OnTriggerEnter(Collider other)
    {
        parkourCounter.coinCount += 1;
        GetComponent<AudioSource>().Play();
        other.gameObject.SetActive(false);
    }
}
