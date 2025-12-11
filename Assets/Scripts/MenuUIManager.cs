using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.XR.Management;
using System.Collections;

/// <summary>
/// Manages the entire Main Menu UI flow, including Login, Signup, Navigation, and Level Selection.
/// Handles authentication states via FirebaseManager and enforces level locking based on user progress.
/// </summary>
public class MenuUIManager : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("Panel for user login.")]
    public GameObject loginPanel;
    [Tooltip("Panel for creating a new account.")]
    public GameObject signupPanel;
    [Tooltip("Main menu panel shown after logging in.")]
    public GameObject startPanel;
    [Tooltip("Panel showing user history or story.")]
    public GameObject historyPanel;
    [Tooltip("Panel containing the list of AR levels.")]
    public GameObject menuPanel;
    
    [Header("Feedback UI")] 
    [Tooltip("Pop-up panel used to warn the user (e.g., when a level is locked).")]
    public GameObject warningPanel;       
    [Tooltip("Text element inside the warning panel to display specific messages.")]
    public TextMeshProUGUI warningText;   

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

    /// <summary>
    /// Struct to map a Unity Button to a specific Scene Name and its Requirement.
    /// </summary>
    [System.Serializable]
    public class SceneButtonMapping
    {
        [Tooltip("The UI Button component.")]
        public Button button;
        
        [Tooltip("The exact name of the scene to load (must be in Build Settings).")]
        public string sceneName;
        
        [Tooltip("Minimum progress required to enter this scene. (0 for Kopi/Toast, 2 for Tray)")]
        public int requiredProgress = 0; 
    }

    [Header("AR Scene Buttons")]
    [Tooltip("List of buttons that load AR scenes. Configure progress requirements here.")]
    public SceneButtonMapping[] arSceneButtons;

    /// <summary>
    /// Initializes UI state, hides warnings, and checks if a user is already logged in.
    /// </summary>
    private void Start()
    {
        // Hide warning panel at start to be safe
        if (warningPanel != null) warningPanel.SetActive(false);

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

    /// <summary>
    /// Attaches click listeners to all buttons in the scene.
    /// Includes logic for Auth, Navigation, and dynamic AR Level buttons.
    /// </summary>
    private void BindButtons()
    {
        // Authentication buttons
        if (loginButton) loginButton.onClick.AddListener(OnLoginClicked);
        if (signupButton) signupButton.onClick.AddListener(OnSignupClicked);
        if (gotoSignupButton) gotoSignupButton.onClick.AddListener(() => ShowPanel(signupPanel));
        if (gotoLoginButton) gotoLoginButton.onClick.AddListener(() => ShowPanel(loginPanel));
        if (logoutButton) logoutButton.onClick.AddListener(OnLogoutClicked);
        
        // Navigation buttons
        if (storyButton) storyButton.onClick.AddListener(() => ShowPanel(historyPanel));
        if (startJourneyButton) startJourneyButton.onClick.AddListener(() => menuPanel.SetActive(true));
        if (historyBackButton) historyBackButton.onClick.AddListener(() => ShowPanel(startPanel));
        if (menuBackButton) menuBackButton.onClick.AddListener(() => menuPanel.SetActive(false));

        // AR Scene buttons with Logic Check
        if (arSceneButtons != null)
        {
            foreach (var mapping in arSceneButtons)
            {
                if (mapping.button != null && !string.IsNullOrEmpty(mapping.sceneName))
                {
                    mapping.button.onClick.RemoveAllListeners();
                    // Pass the required progress to the load function
                    mapping.button.onClick.AddListener(() => LoadARScene(mapping.sceneName, mapping.requiredProgress));
                }
            }
        }
    }

    /// <summary>
    /// Subscribes to Firebase authentication events to handle auto-navigation on login/logout.
    /// </summary>
    private void SetupEventListeners()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnUserLoggedIn += OnUserLoggedIn;
            FirebaseManager.Instance.OnUserLoggedOut += OnUserLoggedOut;
        }
    }

    /// <summary>
    /// Unsubscribes from events to prevent memory leaks when the object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnUserLoggedIn -= OnUserLoggedIn;
            FirebaseManager.Instance.OnUserLoggedOut -= OnUserLoggedOut;
        }
    }

    /// <summary>
    /// Helper method to show one specific panel and hide all others.
    /// Also ensures warning popups are cleared.
    /// </summary>
    /// <param name="panel">The GameObject of the panel to display.</param>
    private void ShowPanel(GameObject panel)
    {
        if (loginPanel) loginPanel.SetActive(false);
        if (signupPanel) signupPanel.SetActive(false);
        if (startPanel) startPanel.SetActive(false);
        if (historyPanel) historyPanel.SetActive(false);
        if (menuPanel) menuPanel.SetActive(false);
        
        // Also hide warning when switching panels so it doesn't get stuck
        if (warningPanel != null) warningPanel.SetActive(false);

        if (panel) panel.SetActive(true);
    }

    /// <summary>
    /// Validates user progress before loading an AR scene.
    /// If the user meets the 'minProgress' requirement, the scene loads.
    /// If not, a warning popup is shown.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    /// <param name="minProgress">The minimum progress count required (e.g. 2).</param>
    private void LoadARScene(string sceneName, int minProgress)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        // 1. Get Current Progress
        int currentProgress = 0;
        if (FirebaseManager.Instance != null)
        {
            currentProgress = FirebaseManager.Instance.CurrentProgress;
        }

        // 2. Check if player qualifies
        if (currentProgress < minProgress)
        {
            // STOP! Show warning message
            ShowWarning($"You need to complete kopi and toast first! ({minProgress} steps required)");
            Debug.Log($"Blocked entry to {sceneName}. Progress {currentProgress}/{minProgress}");
            return;
        }

        // 3. Allowed -> Load Scene
        Debug.Log($"Loading AR scene: {sceneName}");
        StartCoroutine(InitializeARAndLoad(sceneName));
    }

    /// <summary>
    /// Displays the warning panel with a custom message and auto-hides it after 2.5 seconds.
    /// </summary>
    /// <param name="message">The message to display to the user.</param>
    private void ShowWarning(string message)
    {
        if (warningPanel != null)
        {
            warningPanel.SetActive(true);
            
            // Update text if we have it
            if (warningText != null) warningText.text = message;
            
            // Auto hide after 2.5 seconds
            CancelInvoke(nameof(HideWarning));
            Invoke(nameof(HideWarning), 2.5f);
        }
    }

    /// <summary>
    /// Hides the warning panel. Called automatically by Invoke.
    /// </summary>
    private void HideWarning()
    {
        if (warningPanel != null) warningPanel.SetActive(false);
    }

    /// <summary>
    /// Handles the login button click. Validates inputs and calls Firebase Login.
    /// </summary>
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

    /// <summary>
    /// Handles the signup button click. Validates passwords match and calls Firebase Signup.
    /// </summary>
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

    /// <summary>
    /// Logs out the user via FirebaseManager.
    /// </summary>
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
        if (loginEmail) loginEmail.text = "";
        if (loginPassword) loginPassword.text = "";
        if (signupEmail) signupEmail.text = "";
        if (signupPassword) signupPassword.text = "";
        if (signupConfirmPassword) signupConfirmPassword.text = "";
    }

    /// <summary>
    /// Coroutine that initializes XR Subsystems before loading the AR scene.
    /// Prevents crashes by ensuring AR Foundation is ready.
    /// </summary>
    /// <param name="sceneName">The scene to load.</param>
    private IEnumerator InitializeARAndLoad(string sceneName)
    {
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
        
        if (XRGeneralSettings.Instance.Manager.activeLoader != null)
        {
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }

        SceneManager.LoadScene(sceneName);
    }
}