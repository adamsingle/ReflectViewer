using UnityEngine;

/// <summary>
/// Simple script for notifying that a marker has been found
/// </summary>
public class ImageFound : MonoBehaviour
{
    private void OnEnable()
    {
        ImageTracker.Instance.ImageFound();
    }
}
