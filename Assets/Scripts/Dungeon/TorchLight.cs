using UnityEngine;

public class TorchLight : MonoBehaviour
{
    public Light torchLight;
    public float minIntensity = 0.8f;
    public float maxIntensity = 1.2f;
    public float flickerSpeed = 10.0f;

    private float baseIntensity;
    private float randomOffset;

    void Start()
    {
        if (torchLight == null) torchLight = GetComponent<Light>();
        baseIntensity = torchLight.intensity;
        randomOffset = UnityEngine.Random.Range(0f, 100f);
    }

    void Update()
    {
        if (torchLight != null)
        {
            float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, randomOffset);
            torchLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
        }
    }
}

