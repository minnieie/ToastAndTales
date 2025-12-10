using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FirebaseManager : MonoBehaviour
{
    /// <summary>
    /// Manages user authentication using Firebase Authentication.
    /// Provides UI for login and signup, and handles authentication logic.
    /// </summary>
    public static FirebaseManager Instance { get; private set; }

    public FirebaseAuth auth;
    public FirebaseUser user;
    private DatabaseReference dbRef;

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

    private bool isFirebaseReady = false;
    public int CurrentProgress { get; private set; } = 0;
    private const int TotalDishes = 3;
    private Dictionary<string, bool> completedScenes = new Dictionary<string, bool>();

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindButtons();
    }

    private async void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InitializeFirebase();

        // Auto-login if user exists
        if (auth != null && auth.CurrentUser != null)
        {
            user = auth.CurrentUser;
            await FetchUserProgress();
            ShowPanel(startPanel);
        }
        else
        {
            ShowPanel(loginPanel);
        }
    }

    private void RebindButtons()
    {
        loginButton?.onClick.RemoveAllListeners();
        loginButton?.onClick.AddListener(Login);

        signupButton?.onClick.RemoveAllListeners();
        signupButton?.onClick.AddListener(SignUp);

        gotoSignupButton?.onClick.RemoveAllListeners();
        gotoSignupButton?.onClick.AddListener(() => ShowPanel(signupPanel));

        gotoLoginButton?.onClick.RemoveAllListeners();
        gotoLoginButton?.onClick.AddListener(() => ShowPanel(loginPanel));

        logoutButton?.onClick.RemoveAllListeners();
        logoutButton?.onClick.AddListener(Logout);

        storyButton?.onClick.RemoveAllListeners();
        storyButton?.onClick.AddListener(() => ShowPanel(historyPanel));

        startJourneyButton?.onClick.RemoveAllListeners();
        startJourneyButton?.onClick.AddListener(() => menuPanel?.SetActive(true));

        historyBackButton?.onClick.RemoveAllListeners();
        historyBackButton?.onClick.AddListener(() => ShowPanel(startPanel));

        menuBackButton?.onClick.RemoveAllListeners();
        menuBackButton?.onClick.AddListener(() => menuPanel?.SetActive(false));

        if (arSceneButtons != null)
        {
            foreach (var mapping in arSceneButtons)
            {
                if (mapping.button != null && !string.IsNullOrEmpty(mapping.sceneName))
                {
                    mapping.button.onClick.RemoveAllListeners();
                    mapping.button.onClick.AddListener(() => LoadSceneByName(mapping.sceneName));
                }
            }
        }
    }

    private async Task InitializeFirebase()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            dbRef = FirebaseDatabase.DefaultInstance.RootReference;
            isFirebaseReady = true;
            Debug.Log("✓ Firebase initialized");
        }
        else
        {
            Debug.LogError($"✗ Firebase error: {status}");
        }
    }

    private void ShowPanel(GameObject panel)
    {
        loginPanel?.SetActive(false);
        signupPanel?.SetActive(false);
        startPanel?.SetActive(false);
        historyPanel?.SetActive(false);
        menuPanel?.SetActive(false);

        panel?.SetActive(true);
    }

    public async void SignUp()
    {
        if (!isFirebaseReady)
        {
            UpdateStatus(signupStatusText, "Firebase not ready", Color.red);
            return;
        }

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

        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = result.User;

            UpdateStatus(signupStatusText, "Account created! Please log in.", Color.green);

            var userDict = new Dictionary<string, object>
            {
                { "email", user.Email },
                { "progress", 0 },
                { "createdAt", System.DateTime.Now.ToString() }
            };

            await dbRef.Child("users").Child(user.UserId).UpdateChildrenAsync(userDict);

            CurrentProgress = 0;
            ShowPanel(loginPanel);
        }
        catch (FirebaseException e)
        {
            if ((AuthError)e.ErrorCode == AuthError.EmailAlreadyInUse)
                UpdateStatus(signupStatusText, "Email already registered", Color.red);
            else
                UpdateStatus(signupStatusText, $"Signup failed: {e.Message}", Color.red);
        }
    }

    public async void Login()
    {
        if (!isFirebaseReady)
        {
            UpdateStatus(loginStatusText, "Firebase not ready", Color.red);
            return;
        }

        string email = loginEmail.text;
        string password = loginPassword.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            UpdateStatus(loginStatusText, "Enter email and password", Color.red);
            return;
        }

        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            user = result.User;

            UpdateStatus(loginStatusText, "Login successful!", Color.green);

            await SaveEmailToFirebase();
            await FetchUserProgress();
            ShowPanel(startPanel);
        }
        catch
        {
            UpdateStatus(loginStatusText, "Login failed", Color.red);
        }
    }

    public void Logout()
    {
        auth?.SignOut();
        user = null;
        CurrentProgress = 0;
        completedScenes.Clear();

        ShowPanel(loginPanel);
        loginEmail.text = "";
        loginPassword.text = "";
    }

    private void UpdateStatus(TextMeshProUGUI textElement, string message, Color color)
    {
        if (textElement != null)
        {
            textElement.text = message;
            textElement.color = color;
        }
    }

    private async Task SaveEmailToFirebase()
    {
        if (user == null) return;

        try
        {
            await dbRef.Child("users").Child(user.UserId).Child("email").SetValueAsync(user.Email);
            Debug.Log($"✓ Email saved to Firebase: {user.Email}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving email: {e}");
        }
    }

    private void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public async void MarkDishComplete(string sceneName)
    {
        if (user == null) return;
        if (IsSceneCompleted(sceneName)) return;

        completedScenes[sceneName] = true;
        CurrentProgress = Mathf.Clamp(CurrentProgress + 1, 0, TotalDishes);

        await SaveProgressToFirebase(sceneName);

        Debug.Log($"✓ {sceneName} completed! Progress: {CurrentProgress}/{TotalDishes}");

        if (CurrentProgress >= TotalDishes)
            Debug.Log("🎉 ALL DISHES COMPLETED!");
    }

    private async Task SaveProgressToFirebase(string sceneName)
    {
        if (user == null) return;

        string uid = user.UserId;
        try
        {
            await dbRef.Child("users").Child(uid).Child("progress").SetValueAsync(CurrentProgress);
            await dbRef.Child("users").Child(uid).Child(sceneName).SetValueAsync(true);

            Debug.Log($"Saved to Firebase: {sceneName} = complete, progress = {CurrentProgress}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error saving progress: {e}");
        }
    }

    public async Task FetchUserProgress()
    {
        if (user == null) return;

        string uid = user.UserId;
        try
        {
            var snapshot = await dbRef.Child("users").Child(uid).GetValueAsync();
            if (snapshot.Exists)
            {
                if (snapshot.Child("progress").Value != null)
                    CurrentProgress = int.Parse(snapshot.Child("progress").Value.ToString());

                completedScenes.Clear();
                foreach (var childSnapshot in snapshot.Children)
                {
                    string key = childSnapshot.Key;
                    if (key != "progress" && key != "createdAt")
                    {
                        if (childSnapshot.Value is bool sceneBool)
                            completedScenes[key] = sceneBool;
                    }
                }

                Debug.Log($"✓ Loaded user progress: {CurrentProgress}/{TotalDishes} dishes completed");
            }
            else
            {
                CurrentProgress = 0;
                completedScenes.Clear();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error fetching progress: {e}");
        }
    }

    public bool IsSceneCompleted(string sceneName)
    {
        return completedScenes.ContainsKey(sceneName) && completedScenes[sceneName];
    }

    public string GetProgressString()
    {
        return $"{CurrentProgress}/{TotalDishes}";
    }
}
