using UnityEngine;

public class AccessPoint : MonoBehaviour
{
    public TimeManager timeManager = null;
    public bool isGamePaused = false;

    // Start is called before the first frame update
    void Start()
    {
        timeManager = FindObjectOfType<TimeManager>();
    }

}
