using UnityEngine;

public class ImageTracker
{
    private static ImageTracker _instance;

    public static ImageTracker Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ImageTracker();
            }
            return _instance;
        }
    }

    public delegate void ImageFoundHandler();

    public event ImageFoundHandler ImageFoundEvent;

    /// <summary>
    /// The offset of the marker object found in the Reflect model
    /// </summary>
    public GameObject MarkerOffset;

    /// <summary>
    /// The image marker in the scene
    /// </summary>
    public GameObject ImageMarker;

    public void ImageFound()
    {
        ImageFoundEvent?.Invoke();
    }
}
