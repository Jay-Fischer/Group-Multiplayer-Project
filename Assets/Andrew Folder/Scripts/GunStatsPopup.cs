using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Pop-up panel that shows the player's gun stats.
// Shows while Tab is held, and pops up for a few seconds when Show(...) is called (e.g. after an upgrade).
//
// For teammates: from the gun/upgrade code, call
//     GunStatsPopup.Show(stats);      // update the numbers AND pop the panel up
//     GunStatsPopup.SetStats(stats);  // update the numbers quietly (e.g. ammo changed)
public class GunStatsPopup : MonoBehaviour
{
    public static GunStatsPopup Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CanvasGroup panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statsText;

    [Header("Timing")]
    [SerializeField] private float popupDuration = 3f;
    [SerializeField] private float fadeSpeed = 4f;

    [Header("Stats")]
    [SerializeField] private GunStats stats = new GunStats(); // test values until the gun sends real ones

    [Header("Testing")]
    [SerializeField] private bool enableTestKeys = true;

    private float hideTime;

    private void Awake()
    {
        Instance = this;

        if (panel != null)
        {
            panel.alpha = 0f;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }

        RefreshText();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (enableTestKeys && Keyboard.current != null && Keyboard.current.uKey.wasPressedThisFrame)
        {
            TestUpgrade();
        }

        bool holdingKey = Keyboard.current != null && Keyboard.current.tabKey.isPressed;
        bool popupActive = Time.time < hideTime;

        float targetAlpha = (holdingKey || popupActive) ? 1f : 0f;

        if (panel != null)
        {
            panel.alpha = Mathf.MoveTowards(panel.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
        }
    }

    public static void Show(GunStats newStats)
    {
        if (Instance == null) return;

        Instance.ApplyStats(newStats);
        Instance.hideTime = Time.time + Instance.popupDuration;
    }

    public static void SetStats(GunStats newStats)
    {
        if (Instance == null) return;

        Instance.ApplyStats(newStats);
    }

    private void ApplyStats(GunStats newStats)
    {
        if (newStats == null) return;

        stats = newStats.Copy();
        RefreshText();
    }

    private void RefreshText()
    {
        if (titleText != null)
        {
            titleText.text = stats.gunName;
        }

        if (statsText != null)
        {
            statsText.text =
                $"Damage: {stats.damage:0.#}\n" +
                $"Fire Rate: {stats.fireRate:0.#}/s\n" +
                $"Ammo: {stats.currentAmmo}/{stats.maxAmmo}\n" +
                $"Level: {stats.upgradeLevel}";
        }
    }

    // Pretend an upgrade happened so the popup can be tested without any gun code
    private void TestUpgrade()
    {
        GunStats upgraded = stats.Copy();
        upgraded.upgradeLevel++;
        upgraded.damage *= 1.2f;
        upgraded.fireRate *= 1.1f;

        Show(upgraded);
    }
}
