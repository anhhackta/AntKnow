using UnityEngine;
using System.Threading.Tasks;

// Define ANTKNOW_USE_FIREBASE in Player Settings -> Scripting Define Symbols once Firebase SDK is imported
#if ANTKNOW_USE_FIREBASE
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
#endif

public class FirebaseAuthController : MonoBehaviour {
#if ANTKNOW_USE_FIREBASE
  public static FirebaseAuth Auth; public static Firebase.Firestore.FirebaseFirestore DB;

  async void Awake(){
    await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
    Auth = FirebaseAuth.DefaultInstance;
    DB   = FirebaseFirestore.DefaultInstance;
  }

  public async Task<FirebaseUser> SignInEmail(string email, string password){
    var res = await Auth.SignInWithEmailAndPasswordAsync(email, password);
    return res.User;
  }

  public async Task<FirebaseUser> SignUpEmail(string email, string password, string displayName){
    var res = await Auth.CreateUserWithEmailAndPasswordAsync(email, password);
    await res.User.UpdateUserProfileAsync(new UserProfile{ DisplayName = displayName });
    await DB.Collection("profiles").Document(res.User.UserId).SetAsync(new {
      displayName, level = 1, xp = 0, coins = 0, diamonds = 0,
      Luck=0,Resistance=0,Intelligence=0,Health=0,Agility=0
    });
    return res.User;
  }
#else
  // Stub to avoid compile errors before Firebase SDK is imported
  void Awake() { Debug.Log("FirebaseAuthController stub active. Import Firebase SDK and define ANTKNOW_USE_FIREBASE to enable."); }
#endif
}

