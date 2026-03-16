using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SpeedLineManager : MonoBehaviour
{
    [SerializeField] private float speedLineTreshold = 10;
    [SerializeField] private float MaximumSpeedLineTreshold = 40;
    [SerializeField] private FullScreenPassRendererFeature fullscreenFeature;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Material SpeedLineMat;

    private Material cacheMat;

    private void Start()
    {
        cacheMat = new Material(SpeedLineMat);
        fullscreenFeature.passMaterial = cacheMat;
    }

    private void OnDestroy()
    {
        Destroy(cacheMat);
    }

    private void Update()
    {
        if(rb.linearVelocity.magnitude > speedLineTreshold)
        {
            float currentSpeed = rb.linearVelocity.magnitude;
            float t = Mathf.Clamp01(currentSpeed / MaximumSpeedLineTreshold);
            float maskSize = Mathf.Lerp(1.0f, 0.6f, t);
            float mastContrast = Mathf.Lerp(1.0f, 0.6f, t);

            cacheMat.SetFloat("_Mask_size", maskSize);
            cacheMat.SetFloat("_Mask_Contrast", mastContrast);
        }
        else
        {
            cacheMat.SetFloat("_Mask_size", 1);
            cacheMat.SetFloat("_Mask_Contrast", 1);
        }
    }

}
