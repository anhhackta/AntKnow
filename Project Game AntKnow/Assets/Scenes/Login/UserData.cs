using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;

namespace AntKnow.Auth
{
    [Serializable]
    public class UserData
    {
        [Header("Basic Info")]
        public string uid;
        public string username;
        public string email;
        public bool emailVerified;
        public string ingameName;
        public string gender;
        
        [Header("Timestamps")]
        public DateTime createdAt;
        public DateTime lastLoginAt;
        
        [Header("Game Progress")]
        public int level = 1;
        public int xp = 0;
        
        [Header("Currencies")]
        public Currencies currencies;
        
        [Header("Stats")]
        public UserStats stats;
        
        [Header("Status")]
        public string status = "active";

        public UserData()
        {
            createdAt = DateTime.UtcNow;
            lastLoginAt = DateTime.UtcNow;
            currencies = new Currencies();
            stats = new UserStats();
            emailVerified = false;
        }

        public UserData(string uid, string username, string email)
        {
            this.uid = uid;
            this.username = username;
            this.email = email;
            createdAt = DateTime.UtcNow;
            lastLoginAt = DateTime.UtcNow;
            currencies = new Currencies();
            stats = new UserStats();
            emailVerified = false;
        }

        /// <summary>
        /// Convert UserData to Dictionary for Firestore
        /// </summary>
        public Dictionary<string, object> ToFirestoreData()
        {
            return new Dictionary<string, object>
            {
                { "username", username },
                { "email", email },
                { "emailVerified", emailVerified },
                { "ingameName", ingameName ?? null },
                { "gender", gender ?? null },
                { "createdAt", Timestamp.GetCurrentTimestamp() },
                { "lastLoginAt", Timestamp.GetCurrentTimestamp() },
                { "level", level },
                { "xp", xp },
                { "currencies", new Dictionary<string, object>
                    {
                        { "antCoin", currencies.antCoin },
                        { "dCoin", currencies.dCoin }
                    }
                },
                { "stats", new Dictionary<string, object>
                    {
                        { "matchesPlayed", stats.matchesPlayed },
                        { "wins", stats.wins }
                    }
                },
                { "status", status }
            };
        }

        /// <summary>
        /// Create UserData from Firestore document
        /// </summary>
        public static UserData FromFirestoreData(string uid, Dictionary<string, object> data)
        {
            var userData = new UserData();
            userData.uid = uid;
            
            if (data.ContainsKey("username"))
                userData.username = data["username"].ToString();
            if (data.ContainsKey("email"))
                userData.email = data["email"].ToString();
            if (data.ContainsKey("emailVerified"))
                userData.emailVerified = (bool)data["emailVerified"];
            if (data.ContainsKey("ingameName"))
                userData.ingameName = data["ingameName"]?.ToString();
            if (data.ContainsKey("gender"))
                userData.gender = data["gender"]?.ToString();
            
            if (data.ContainsKey("level"))
                userData.level = Convert.ToInt32(data["level"]);
            if (data.ContainsKey("xp"))
                userData.xp = Convert.ToInt32(data["xp"]);
            
            if (data.ContainsKey("status"))
                userData.status = data["status"].ToString();
            
            // Parse currencies
            if (data.ContainsKey("currencies") && data["currencies"] is Dictionary<string, object> currenciesData)
            {
                userData.currencies = new Currencies();
                if (currenciesData.ContainsKey("antCoin"))
                    userData.currencies.antCoin = Convert.ToInt32(currenciesData["antCoin"]);
                if (currenciesData.ContainsKey("dCoin"))
                    userData.currencies.dCoin = Convert.ToInt32(currenciesData["dCoin"]);
            }
            
            // Parse stats
            if (data.ContainsKey("stats") && data["stats"] is Dictionary<string, object> statsData)
            {
                userData.stats = new UserStats();
                if (statsData.ContainsKey("matchesPlayed"))
                    userData.stats.matchesPlayed = Convert.ToInt32(statsData["matchesPlayed"]);
                if (statsData.ContainsKey("wins"))
                    userData.stats.wins = Convert.ToInt32(statsData["wins"]);
            }
            
            return userData;
        }
    }

    [Serializable]
    public class Currencies
    {
        public int antCoin = 0;
        public int dCoin = 0;

        public Currencies()
        {
            antCoin = 0;
            dCoin = 0;
        }

        public Currencies(int antCoin, int dCoin)
        {
            this.antCoin = antCoin;
            this.dCoin = dCoin;
        }
    }

    [Serializable]
    public class UserStats
    {
        public int matchesPlayed = 0;
        public int wins = 0;

        public UserStats()
        {
            matchesPlayed = 0;
            wins = 0;
        }

        public UserStats(int matchesPlayed, int wins)
        {
            this.matchesPlayed = matchesPlayed;
            this.wins = wins;
        }

        public float WinRate => matchesPlayed > 0 ? (float)wins / matchesPlayed : 0f;
    }
}
