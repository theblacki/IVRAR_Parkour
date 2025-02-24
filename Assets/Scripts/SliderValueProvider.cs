using UnityEngine;

public class SliderValueProvider : MonoBehaviour
{
    public float sliderValue;

    private Transform slider;
    private readonly float radius = 0.32f;
    void Start()
    {
        slider = gameObject.transform.GetChild(1);
    }

    void Update()
    {
        sliderValue = (slider.localPosition.x + radius) / (2 * radius);
    }
}
