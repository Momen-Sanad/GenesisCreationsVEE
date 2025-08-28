using UnityEngine;

public class TiltIndicator : MonoBehaviour
{
    public GameObject whiteLine;
    public GameObject greenLine;

    public void SetTiltInRange(bool inRange)
    {
        Debug.Log("Changing colors");
        whiteLine.SetActive(!inRange);
        greenLine.SetActive(inRange);
    }

    public void DisableAll()
    {
        whiteLine.SetActive(false);
        greenLine.SetActive(false);
    }
}
