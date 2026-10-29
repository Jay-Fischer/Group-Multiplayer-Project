using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

// Shows a small health bar above every other player's head.
// Put this on the HUD canvas (Screen Space - Overlay) and give it a disabled bar to copy.
public class OverheadHealthBars : MonoBehaviour
{
    [Header("Bar")]
    [SerializeField] private RectTransform barTemplate; // disabled bar under the HUD canvas, with a Filled image inside it
    [SerializeField] private float maxHealth = 100f; // keep this the same as Max Health on the Player's NetworkHealth
    [SerializeField] private Gradient healthColor;

    [Header("Placement")]
    [SerializeField] private float heightAboveHead = 2.2f;
    [SerializeField] private float maxDistance = 50f;

    [Header("Who Gets Bars")]
    [SerializeField] private bool showOnEnemies = false;

    private class Bar
    {
        public NetworkHealth target;
        public RectTransform root;
        public Image fill;
    }

    // Keyed by NetworkObjectId so destroyed players can still be looked up and cleaned away
    private readonly Dictionary<ulong, Bar> bars = new();
    private readonly List<ulong> toRemove = new();
    private float nextScanTime;

    private void Awake()
    {
        if (barTemplate != null)
        {
            barTemplate.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        NetworkManager manager = NetworkManager.Singleton;

        if (barTemplate == null || manager == null || !manager.IsClient)
        {
            ClearBars();
            return;
        }

        // Looking for new players is slower, so only do it a few times a second
        if (Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + 0.5f;
            FindNewTargets();
        }

        Camera cam = Camera.main;

        foreach (KeyValuePair<ulong, Bar> pair in bars)
        {
            Bar bar = pair.Value;

            if (bar.target == null || !bar.target.IsSpawned)
            {
                toRemove.Add(pair.Key);
                continue;
            }

            UpdateBar(bar, cam);
        }

        foreach (ulong id in toRemove)
        {
            if (bars[id].root != null)
            {
                Destroy(bars[id].root.gameObject);
            }
            bars.Remove(id);
        }
        toRemove.Clear();
    }

    private void FindNewTargets()
    {
        foreach (NetworkHealth health in FindObjectsByType<NetworkHealth>(FindObjectsSortMode.None))
        {
            if (!health.IsSpawned || bars.ContainsKey(health.NetworkObjectId))
            {
                continue;
            }

            NetworkObject netObj = health.NetworkObject;

            // Your own health is already shown by PlayerHUD
            if (netObj.IsLocalPlayer)
            {
                continue;
            }

            if (!netObj.IsPlayerObject && !showOnEnemies)
            {
                continue;
            }

            RectTransform root = Instantiate(barTemplate, barTemplate.parent);
            root.name = $"OverheadBar ({health.name})";
            root.gameObject.SetActive(true);

            bars.Add(health.NetworkObjectId, new Bar { target = health, root = root, fill = FindFillImage(root) });
        }
    }

    private void UpdateBar(Bar bar, Camera cam)
    {
        NetworkHealth target = bar.target;

        if (cam == null)
        {
            bar.root.gameObject.SetActive(false);
            return;
        }

        Vector3 worldPos = target.transform.position + Vector3.up * heightAboveHead;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // Hide bars behind the camera or too far away
        bool visible = screenPos.z > 0f && screenPos.z <= maxDistance;
        bar.root.gameObject.SetActive(visible);

        if (!visible)
        {
            return;
        }

        bar.root.position = new Vector3(screenPos.x, screenPos.y, 0f);

        if (bar.fill != null)
        {
            float percent = Mathf.Clamp01(target.CurrentHealth.Value / maxHealth);
            bar.fill.fillAmount = percent;
            bar.fill.color = healthColor.Evaluate(percent);
        }
    }

    private static Image FindFillImage(RectTransform root)
    {
        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            if (image.type == Image.Type.Filled)
            {
                return image;
            }
        }
        return null;
    }

    private void ClearBars()
    {
        if (bars.Count == 0) return;

        foreach (Bar bar in bars.Values)
        {
            if (bar.root != null)
            {
                Destroy(bar.root.gameObject);
            }
        }
        bars.Clear();
    }
}
