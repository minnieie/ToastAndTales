using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public FirebaseAuth auth;
    public FirebaseUser user;
    private DatabaseReference dbRef;

    public System.Action<int, int> OnProgressUpdated;
    public System.Action OnUserLoggedIn;
    public System.Action OnUserLoggedOut;

    private bool isFirebaseReady = false;
    public int CurrentProgress { get; private set; } = 0;
    private const int TotalDishes = 3;
    private Dictionary<string, bool> completedScenes = new Dictionary<string, bool>();

    private void Awake()
        {
            // STRICT CHECK: If a manager already exists, kill THIS new one immediately.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Kill the imposter
                return; // Stop running any more code
            }

            // If I am the first one, I am the King.
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    private async void Start()
    {
        await InitializeFirebase();

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

    /// <summary>
    /// Signs up a new user with email and password.
    /// </summary>
    public async Task<string> SignUpAsync(string email, string password)
    {   
        // 1. Pre-check: Is Firebase ready?
        if (!isFirebaseReady) return "Firebase is not ready.";

        // 2. Manual Check: This gives you the specific "Password too short" feedback immediately
        if (password.Length < 6)
        {
            return "Password is too short! It must be at least 6 characters.";
        }

        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = result.User;

            var userDict = new Dictionary<string, object>
            {
                { "email", user.Email },
                { "progress", 0 },
                { "createdAt", System.DateTime.Now.ToString() },
                { "lastLogin", System.DateTime.Now.ToString() }
            };

            await dbRef.Child("users").Child(user.UserId).UpdateChildrenAsync(userDict);

            CurrentProgress = 0;
            
            // 3. Return an empty string to indicate SUCCESS
            return ""; 
        }
        catch (FirebaseException e)
        {
            // 4. If Firebase fails (e.g., "Email already in use"), return that specific message
            Debug.LogError($"Signup failed: {e.Message}");
            return e.Message; 
        }
    }
    // --- UPDATED: Keeps 'lastLogin' update ---
    public async Task<bool> LoginAsync(string email, string password)
    {
        if (!isFirebaseReady) return false;

        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            user = result.User;

            // Update Last Login timestamp on successful login
            if (user != null)
            {
                await dbRef.Child("users").Child(user.UserId).Child("lastLogin").SetValueAsync(System.DateTime.Now.ToString());
            }

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

    public async void MarkDishComplete(string sceneName, float timeTaken = 0f)
    {
        if (user == null) return;
        if (IsSceneCompleted(sceneName)) return;

        completedScenes[sceneName] = true;

        await SaveProgressToFirebase(sceneName, timeTaken);
        OnProgressUpdated?.Invoke(CurrentProgress, TotalDishes);

        Debug.Log($"✓ {sceneName} completed! Progress: {CurrentProgress}/{TotalDishes}");

        if (CurrentProgress >= TotalDishes)
            Debug.Log("ALL DISHES COMPLETED!");
    }

    private async Task SaveProgressToFirebase(string sceneName, float timeTaken)
    {
        if (user == null) return;

        string uid = user.UserId;
        try
        {
            // 1. Save overall progress count
            await dbRef.Child("users").Child(uid).Child("progress").SetValueAsync(CurrentProgress);

            // 2. Create detailed data object
            var dishData = new Dictionary<string, object>
            {
                { "completed", true },
                { "timeTaken", timeTaken },
                { "dateCompleted", System.DateTime.Now.ToString() }
            };

            // 3. Save it directly to the key (e.g., users/uid/Kopi)
            // This replaces the old "true" value with this new object
            await dbRef.Child("users").Child(uid).Child(sceneName).SetValueAsync(dishData);
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
                    
                    // Ignore metadata keys
                    if (key != "progress" && key != "email" && key != "createdAt" && key != "lastLogin")
                    {
                        // CASE 1: Old Format ("Kopi": true)
                        if (childSnapshot.Value is bool sceneBool)
                        {
                            completedScenes[key] = sceneBool;
                        }
                        // CASE 2: New Format ("Kopi": { "completed": true, ... })
                        else if (childSnapshot.HasChild("completed"))
                        {
                            bool isComplete = (bool)childSnapshot.Child("completed").Value;
                            completedScenes[key] = isComplete;
                        }
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

    public bool IsUserLoggedIn()
    {
        return user != null;
    }

    public string GetUserEmail()
    {
        return user?.Email ?? "Not logged in";
    }

    public async void UpdateUserProgress(int newProgress)
    {
        CurrentProgress = newProgress;
        
        OnProgressUpdated?.Invoke(CurrentProgress, TotalDishes);

        if (user != null && dbRef != null)
        {
            try
            {
                await dbRef.Child("users").Child(user.UserId).Child("progress").SetValueAsync(CurrentProgress);
                Debug.Log($"Progress updated to {CurrentProgress}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to update progress: {e.Message}");
            }
        }
    }
}