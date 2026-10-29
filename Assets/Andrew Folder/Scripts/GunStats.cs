using UnityEngine;

// The numbers the gun stats popup shows.
// The gun/upgrade code fills one of these in and passes it to GunStatsPopup.Show(...)
[System.Serializable]
public class GunStats
{
    public string gunName = "Prototype Gun";
    public float damage = 25f;
    public float fireRate = 3.3f; // shots per second (1 / shootDelay)
    public int currentAmmo = 12;
    public int maxAmmo = 12;
    public int upgradeLevel = 1;

    public GunStats Copy()
    {
        return (GunStats)MemberwiseClone();
    }
}
