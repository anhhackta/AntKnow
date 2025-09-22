using System;
using UnityEngine;

namespace AntKnow.Auth
{
    [Serializable]
    public class UserProfile
    {
        public string uid;
        public string username;
        public string email;
        public bool rankEligible = true;
        public int elo = 1000;
        public int level = 1;
        public int exp = 0;
        public int powerScore = 0;
        public DateTime createdAt;
        public DateTime lastLoginAt;

        public UserProfile()
        {
            createdAt = DateTime.UtcNow;
            lastLoginAt = DateTime.UtcNow;
        }

        public UserProfile(string uid, string username, string email)
        {
            this.uid = uid;
            this.username = username;
            this.email = email;
            createdAt = DateTime.UtcNow;
            lastLoginAt = DateTime.UtcNow;
        }
    }
}
