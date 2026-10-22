using UnityEngine;

public class KillScript : MonoBehaviour
{
    public float killTime = 2.0f;
    private void Start()
    {
        Destroy(gameObject, killTime);
    }
}
