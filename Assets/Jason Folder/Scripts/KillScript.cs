using Unity.Netcode;
using UnityEngine;

public class KillScript : NetworkBehaviour
{
    public float killTime = 2.0f;
    private void Start()
    {
        destroyRpc();
    }


    [Rpc(SendTo.Server)]
    public void destroyRpc()
    {
        Destroy(gameObject, killTime);
    }
    [Rpc(SendTo.Server)]
    public void instantDestroyRpc()
    {
        Destroy(gameObject);
    }
}
