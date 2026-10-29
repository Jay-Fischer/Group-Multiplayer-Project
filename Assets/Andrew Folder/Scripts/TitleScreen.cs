using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

// Title screen shown before joining a game. Host starts a game, Join connects to one, Quit closes the game.
// Hides itself once you are in a game and comes back if you disconnect.
public class TitleScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup titleScreen;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_InputField addressInput; // optional: the host's IP address, leave empty for this PC
    [SerializeField] private TMP_Text statusText; // optional: shows "Connecting..." etc.

    [Header("Connection")]
    [SerializeField] private string defaultAddress = "127.0.0.1";

    private bool isConnecting;

    private void Start()
    {
        if (hostButton != null) hostButton.onClick.AddListener(Host);
        if (joinButton != null) joinButton.onClick.AddListener(Join);
        if (quitButton != null) quitButton.onClick.AddListener(Quit);

        NetworkManager manager = NetworkManager.Singleton;
        if (manager != null)
        {
            manager.OnClientConnectedCallback += OnClientConnected;
            manager.OnClientStopped += OnClientStopped;
        }

        SetStatus("");
        SetVisible(manager == null || !manager.IsListening);
    }

    private void OnDestroy()
    {
        NetworkManager manager = NetworkManager.Singleton;
        if (manager != null)
        {
            manager.OnClientConnectedCallback -= OnClientConnected;
            manager.OnClientStopped -= OnClientStopped;
        }
    }

    public void Host()
    {
        NetworkManager manager = NetworkManager.Singleton;
        if (manager == null || manager.IsListening) return;

        if (manager.StartHost())
        {
            SetVisible(false);
        }
        else
        {
            SetStatus("Could not start a game.");
        }
    }

    public void Join()
    {
        NetworkManager manager = NetworkManager.Singleton;
        if (manager == null || manager.IsListening) return;

        if (manager.TryGetComponent(out UnityTransport transport))
        {
            string address = addressInput != null ? addressInput.text.Trim() : "";
            transport.ConnectionData.Address = string.IsNullOrEmpty(address) ? defaultAddress : address;
        }

        if (manager.StartClient())
        {
            isConnecting = true;
            SetButtonsInteractable(false);
            SetStatus("Connecting...");
        }
        else
        {
            SetStatus("Could not connect.");
        }
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnClientConnected(ulong clientId)
    {
        NetworkManager manager = NetworkManager.Singleton;

        // Only react to ourselves joining, not other players
        if (manager != null && clientId == manager.LocalClientId)
        {
            isConnecting = false;
            SetStatus("");
            SetVisible(false);
        }
    }

    private void OnClientStopped(bool wasHost)
    {
        // Either the connection failed or we left the game, so go back to the title screen
        SetStatus(isConnecting ? "Could not connect." : "Disconnected.");
        isConnecting = false;
        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        if (titleScreen != null)
        {
            titleScreen.alpha = visible ? 1f : 0f;
            titleScreen.interactable = visible;
            titleScreen.blocksRaycasts = visible;
        }

        SetButtonsInteractable(visible);

        // The player's camera locks the mouse in game, so free it on the title screen
        if (visible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (hostButton != null) hostButton.interactable = interactable;
        if (joinButton != null) joinButton.interactable = interactable;
        if (quitButton != null) quitButton.interactable = true;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
