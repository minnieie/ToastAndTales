using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FirebaseManager : MonoBehaviour
{
    /// <summary>
    /// Manages Firebase authentication and database operations
    /// NO UI references - UI is handled by scene-specific managers
    /// </summary>
    public static FirebaseManager Instance { get; private set; }

    public FirebaseAuth auth;
    public FirebaseUser user;
    private DatabaseReference dbRef;

    // Events for UI communication
    public System.Action<int, int> OnProgressUpdated;
    public System.Action OnUserLoggedIn;
    public System.Action OnUserLoggedOut;

    private bool isFirebaseReady = false;
    public int CurrentProgress { get; private set; } = 0;
    private const int TotalDishes = 3;
    private Dictionary<string, bool> completedScenes = new Dictionary<string, bool>();

    private void Awake()
    {
        // Handle singleton properly
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning($"Destroying duplicate FirebaseManager on {gameObject.name}");
            DestroyImmediate(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        await InitializeFirebase();

        // Auto-login if user exists
        if (auth != null && auth.CurrentUser != null)
        {
            user = auth.CurrentUser;
            await FetchUserProgress();
            OnUserLoggedIn?.Invoke();
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

    // ============ AUTHENTICATION METHODS ============

    public async Task<bool> SignUpAsync(string email, string password)
    {
        if (!isFirebaseReady) return false;

        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = result.User;

            var userDict = new Dictionary<string, object>
            {
                { "email", user.Email },
                { "progress", 0 },
                { "createdAt", System.DateTime.Now.ToString() }
            };

            await dbRef.Child("users").Child(user.UserId).UpdateChildrenAsync(userDict);

            CurrentProgress = 0;
            return true;
        }
        catch (FirebaseException e)
        {
            Debug.LogError($"Signup failed: {e.Message}");
            return false;
        }
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        if (!isFirebaseReady) return false;

        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            user = result.User;

            await FetchUserProgress();
            OnUserLoggedIn?.Invoke();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Logout()
    {
        auth?.SignOut();
        user = null;
        CurrentProgress = 0;
        completedScenes.Clear();
        OnUserLoggedOut?.Invoke();
    }

    // ============ PROGRESS TRACKING METHODS ============

    public async void MarkDishComplete(string sceneName)
    {
        if (user == null) return;
        if (IsSceneCompleted(sceneName)) return;

        completedScenes[sceneName] = true;
        CurrentProgress = Mathf.Clamp(CurrentProgress + 1, 0, TotalDishes);

        await SaveProgressToFirebase(sceneName);
        OnProgressUpdated?.Invoke(CurrentProgress, TotalDishes);

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
                    if (key != "progress" && key != "email" && key != "createdAt")
                    {
                        if (childSnapshot.Value is bool sceneBool)
                            completedScenes[key] = sceneBool;
                    }
                }

                OnProgressUpdated?.Invoke(CurrentProgress, TotalDishes);
                Debug.Log($"✓ Loaded user progress: {CurrentProgress}/{TotalDishes}");
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

    // ============ HELPER METHODS ============

    public bool IsUserLoggedIn()
    {
        return user != null;
    }

    public string GetUserEmail()
    {
        return user?.Email ?? "Not logged in";
    }
}