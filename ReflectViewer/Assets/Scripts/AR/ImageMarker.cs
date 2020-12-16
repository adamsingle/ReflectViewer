using UnityEngine;

public class ImageMarker : MonoBehaviour
{
    private void OnEnable()
    {
        ImageTracker.Instance.ImageMarker = this.gameObject;
    }
}
