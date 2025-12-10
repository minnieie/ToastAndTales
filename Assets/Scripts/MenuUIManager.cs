using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class MenuUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject signupPanel;
    public GameObject startPanel;
    public GameObject historyPanel;
    public GameObject menuPanel;

    [Header("Login UI")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;
    public Button loginButton;
    public Button gotoSignupButton;
    public TextMeshProUGUI loginStatusText;

    [Header("Signup UI")]
    public TMP_InputField signupEmail;
    public TMP_InputField signupPassword;
    public TMP_InputField signupConfirmPassword;
    public Button signupButton;
    public Button gotoLoginButton;
    public TextMeshProUGUI signupStatusText;

    [Header("Start Panel UI")]
    public Button storyButton;
    public Button startJourneyButton;
    public Button logoutButton;
    public TextMeshProUGUI welcomeText;

    [Header("History Panel UI")]
    public Button historyBackButton;

    [Header("Menu Panel UI")]
    public Button menuBackButton;

    [System.Serializable]
    public class SceneButtonMapping
    {
        public Button button;
        public string sceneName;
    }

    [Header("AR Scene Buttons")]
    public SceneButtonMapping[] arSceneButtons;

    private void Start()
    {
        BindButtons();
        SetupEventListeners();

        // Check current auth state
        if (FirebaseManager.Instance?.user != null)
        {
            ShowPanel(startPanel);
            UpdateWelcomeText();
        }
        else
        {
            ShowPanel(loginPanel);
        }
    }

    private void BindButtons()
    {
        // Authentication buttons
        loginButton.onClick.AddListener(OnLoginClicked);
        signupButton.onClick.AddListener(OnSignupClicked);
        gotoSignupButton.onClick.AddListener(() => ShowPanel(signupPanel));
        gotoLoginButton.onClick.AddListener(() => ShowPanel(loginPanel));
        logoutButton.onClick.AddListener(OnLogoutClicked);
        
        // Navigation buttons
        storyButton.onClick.AddListener(() => ShowPanel(historyPanel));
        startJourneyButton.onClick.AddListener(() => menuPanel.SetActive(true));
        historyBackButton.onClick.AddListener(() => ShowPanel(startPanel));
        menuBackButton.onClick.AddListener(() => menuPanel.SetActive(false));

        // AR Scene buttons
        if (arSceneButtons != null)
        {
            foreach (var mapping in arSceneButtons)
            {
                if (mapping.button != null && !string.IsNullOrEmpty(mapping.sceneName))
                {
                    mapping.button.onClick.RemoveAllListeners();
                    mapping.button.onClick.AddListener(() => LoadARScene(mapping.sceneName));
                }
            }
        }
    }

    private void SetupEventListeners()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnUserLoggedIn += OnUserLoggedIn;
            FirebaseManager.Instance.OnUserLoggedOut += OnUserLoggedOut;
        }
    }

    private void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnUserLoggedIn -= OnUserLoggedIn;
            FirebaseManager.Instance.OnUserLoggedOut -= OnUserLoggedOut;
        }
    }

    private void ShowPanel(GameObject panel)
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        startPanel.SetActive(false);
        historyPanel.SetActive(false);
        menuPanel.SetActive(false);

        panel.SetActive(true);
    }

    private async void OnLoginClicked()
    {
        string email = loginEmail.text;
        string password = loginPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            UpdateStatus(loginStatusText, "Enter email and password", Color.red);
            return;
        }

        UpdateStatus(loginStatusText, "Logging in...", Color.yellow);

        bool success = await FirebaseManager.Instance.LoginAsync(email, password);
        
        if (success)
        {
            UpdateStatus(loginStatusText, "Login successful!", Color.green);
            UpdateWelcomeText();
        }
        else
        {
            UpdateStatus(loginStatusText, "Login failed", Color.red);
        }
    }

    private async void OnSignupClicked()
    {
        string email = signupEmail.text;
        string password = signupPassword.text;
        string confirm = signupConfirmPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            UpdateStatus(signupStatusText, "Enter email and password", Color.red);
            return;
        }

        if (password != confirm)
        {
            UpdateStatus(signupStatusText, "Passwords do not match", Color.red);
            return;
        }

        UpdateStatus(signupStatusText, "Creating account...", Color.yellow);

        bool success = await FirebaseManager.Instance.SignUpAsync(email, password);
        
        if (success)
        {
            UpdateStatus(signupStatusText, "Account created! Please log in.", Color.green);
            ShowPanel(loginPanel);
        }
        else
        {
            UpdateStatus(signupStatusText, "Signup failed", Color.red);
        }
    }

    private void OnLogoutClicked()
    {
        FirebaseManager.Instance.Logout();
    }

    private void OnUserLoggedIn()
    {
        ShowPanel(startPanel);
        UpdateWelcomeText();
    }

    private void OnUserLoggedOut()
    {
        ShowPanel(loginPanel);
        ClearInputFields();
    }

    private void UpdateWelcomeText()
    {
        if (welcomeText != null && FirebaseManager.Instance?.user != null)
        {
            welcomeText.text = $"Welcome, {FirebaseManager.Instance.user.Email}!";
        }
    }

    private void UpdateStatus(TextMeshProUGUI textElement, string message, Color color)
    {
        if (textElement != null)
        {
            textElement.text = message;
            textElement.color = color;
        }
    }

    private void ClearInputFields()
    {
        loginEmail.text = "";
        loginPassword.text = "";
        signupEmail.text = "";
        signupPassword.text = "";
        signupConfirmPassword.text = "";
    }

    // NEW METHOD: Load AR Scene
    private void LoadARScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            Debug.Log($"Loading AR scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Scene name is empty!");
        }
    }

    // Optional: Add progress display to menu
    public void UpdateProgressDisplay()
    {
        // You can add progress display here if needed
        if (FirebaseManager.Instance != null)
        {
            Debug.Log($"Current progress: {FirebaseManager.Instance.GetProgressString()}");
        }
    }
}