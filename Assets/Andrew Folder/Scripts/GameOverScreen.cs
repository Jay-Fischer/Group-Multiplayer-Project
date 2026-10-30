using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

// Game Over screen shown to everyone when every player's health reaches 0.
// Restart (host only) clears the enemies, resets the waves, refills everyone's health and puts players back at the start.
// Quit closes the game.
// Needs a NetworkObject on the same GameObject so the host can tell every player to restart.
public class GameOverScreen : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup gameOverScreen;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_Text statusText; // optional: shows "Waiting for the host to restart..." for non-hosts

    [Header("Restart")]
    [SerializeField] private float playerMaxHealth = 100f; // keep this the same as Max Health on the Player's NetworkHealth
    [SerializeField] private Transform spawnPoint; // optional: where players go on restart, leave empty to use the Player prefab's position
    [SerializeField] private float spaceBetweenPlayers = 2f;

    private bool isShowing;
    private bool someoneWasAlive; // stops the screen showing before anyone has spawned in
    private float nextCheckTime;

    private void Awake()
    {
        SetVisible(false);
    }

    private void Start()
    {
        if (restartButton != null) restartButton.onClick.AddListener(Restart);
        if (quitButton != null) quitButton.onClick.AddListener(Quit);
    }

    private void Update()
    {
        NetworkManager manager = NetworkManager.Singleton;

        // Left the game, so the title screen takes over
        if (manager == null || !manager.IsClient)
        {
            someoneWasAlive = false;
            if (isShowing) SetVisible(false);
            return;
        }

        // Looking for players is slower, so only do it a few times a second
        if (Time.time < nextCheckTime) return;
        nextCheckTime = Time.time + 0.25f;

        bool allDead = AreAllPlayersDead(out bool anyPlayers);

        if (!anyPlayers) return;

        if (!allDead)
        {
            someoneWasAlive = true;
            if (isShowing) SetVisible(false);
        }
        else if (someoneWasAlive && !isShowing)
        {
            SetVisible(true);
        }
    }

    private static bool AreAllPlayersDead(out bool anyPlayers)
    {
        anyPlayers = false;

        foreach (NetworkHealth health in FindObjectsByType<NetworkHealth>(FindObjectsSortMode.None))
        {
            if (!health.IsSpawned || !health.NetworkObject.IsPlayerObject) continue;

            anyPlayers = true;

            if (health.CurrentHealth.Value > 0f) return false;
        }

        return anyPlayers;
    }

    public void Restart()
    {
        // Only the host can reset the game for everyone
        if (!IsServer || !IsSpawned) return;

        NetworkManager manager = NetworkManager.Singleton;

        // Remove enemies, bullets and anything else spawned during the game (players and objects placed in the scene stay)
        List<NetworkObject> toDespawn = new();
        foreach (NetworkObject netObj in manager.SpawnManager.SpawnedObjectsList)
        {
            if (netObj.IsPlayerObject || netObj.IsSceneObject == true) continue;
            toDespawn.Add(netObj);
        }
        foreach (NetworkObject netObj in toDespawn)
        {
            if (netObj != null && netObj.IsSpawned) netObj.Despawn(true);
        }

        // Respawning the wave system makes it start again from wave 1
        foreach (EnemyHandler waves in FindObjectsByType<EnemyHandler>(FindObjectsSortMode.None))
        {
            if (!waves.IsSpawned) continue;
            waves.NetworkObject.Despawn(false);
            waves.NetworkObject.Spawn();
        }

        // Refill everyone's health (NetworkHealth turns their movement and shooting back on)
        foreach (NetworkHealth health in FindObjectsByType<NetworkHealth>(FindObjectsSortMode.None))
        {
            if (health.IsSpawned && health.NetworkObject.IsPlayerObject)
            {
                health.CurrentHealth.Value = playerMaxHealth;
            }
        }

        RestartPlayersRpc();
    }

    // Each player controls their own position, so every player moves themselves back to the start
    [Rpc(SendTo.ClientsAndHost)]
    private void RestartPlayersRpc()
    {
        NetworkManager manager = NetworkManager.Singleton;
        NetworkObject localPlayer = manager.LocalClient?.PlayerObject;

        if (localPlayer != null)
        {
            // Spread players out so they don't all land on the same spot (the host is client 0)
            int index = (int)(manager.LocalClientId % 8);

            Vector3 start = spawnPoint != null
                ? spawnPoint.position
                : manager.NetworkConfig.PlayerPrefab.transform.position;

            // A CharacterController undoes position changes while it's on, so turn it off for the move
            CharacterController controller = localPlayer.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;
            localPlayer.transform.position = start + Vector3.right * spaceBetweenPlayers * index;
            if (controller != null) controller.enabled = true;
        }

        someoneWasAlive = true;
        SetVisible(false);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetVisible(bool visible)
    {
        bool wasShowing = isShowing;
        isShowing = visible;

        if (gameOverScreen != null)
        {
            gameOverScreen.alpha = visible ? 1f : 0f;
            gameOverScreen.interactable = visible;
            gameOverScreen.blocksRaycasts = visible;
        }

        bool isHost = NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;

        if (restartButton != null) restartButton.interactable = visible && isHost;

        if (statusText != null)
        {
            statusText.text = visible && !isHost ? "Waiting for the host to restart..." : "";
        }

        if (visible)
        {
            // Free the mouse so the buttons can be clicked
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (wasShowing)
        {
            // Back in the game, so lock the mouse again for looking around
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
