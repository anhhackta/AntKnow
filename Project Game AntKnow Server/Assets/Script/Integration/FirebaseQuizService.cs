using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Define ANTKNOW_USE_FIREBASE once Firebase SDK is imported
#if ANTKNOW_USE_FIREBASE
using Firebase.Firestore;
#endif

public class FirebaseQuizService {
#if ANTKNOW_USE_FIREBASE
  readonly FirebaseFirestore _db;
  public FirebaseQuizService(FirebaseFirestore db){ _db=db; }

  [Serializable] public class QuizDoc {
    public string category; public string difficulty;
    public string question; public string[] choices; public int answerIndex;
  }

  public async Task<QuizDoc> GetRandomByCategory(string category){
    var snap = await _db.Collection("quizzes")
                        .WhereEqualTo("category", category)
                        .Limit(20)
                        .GetSnapshotAsync();
    if (snap.Count==0) return null;
    var docs = new List<QuizDoc>();
    foreach(var d in snap.Documents) docs.Add(d.ConvertTo<QuizDoc>());
    var r = new System.Random();
    return docs[r.Next(docs.Count)];
  }
#else
  // Stub API to avoid compile errors before Firebase SDK is imported
  [Serializable] public class QuizDoc { public string category; public string difficulty; public string question; public string[] choices; public int answerIndex; }
  public FirebaseQuizService(object _) {}
  public Task<QuizDoc> GetRandomByCategory(string category) { return Task.FromResult<QuizDoc>(null); }
#endif
}

