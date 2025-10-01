using UnityEngine;

#if UNITY_EDITOR
using System.IO;
#endif

namespace AntKnow.Auth
{
    /// <summary>
    /// Helper để detect ParrelSync clone và auto login tài khoản khác
    /// </summary>
    public static class ParrelSyncHelper
    {
        /// <summary>
        /// Kiểm tra xem có phải ParrelSync clone không
        /// </summary>
        public static bool IsClone()
        {
#if UNITY_EDITOR
            // ParrelSync tạo folder với suffix "_clone_X"
            string projectPath = Application.dataPath;
            return projectPath.Contains("_clone_");
#else
            return false;
#endif
        }

        /// <summary>
        /// Lấy clone number (0 = Editor, 1 = Clone 1, 2 = Clone 2, ...)
        /// </summary>
        public static int GetCloneNumber()
        {
#if UNITY_EDITOR
            string projectPath = Application.dataPath;
            
            if (!projectPath.Contains("_clone_"))
                return 0; // Editor gốc
            
            // Extract number từ "_clone_X"
            int cloneIndex = projectPath.IndexOf("_clone_");
            if (cloneIndex >= 0)
            {
                string afterClone = projectPath.Substring(cloneIndex + 7); // "_clone_".Length = 7
                
                // Tìm số đầu tiên
                string numberStr = "";
                foreach (char c in afterClone)
                {
                    if (char.IsDigit(c))
                        numberStr += c;
                    else
                        break;
                }
                
                if (int.TryParse(numberStr, out int cloneNum))
                    return cloneNum;
            }
            
            return 1; // Default clone 1
#else
            return 0;
#endif
        }

        /// <summary>
        /// Lấy test account email dựa trên clone number
        /// </summary>
        public static string GetTestEmail()
        {
            int cloneNum = GetCloneNumber();
            
            if (cloneNum == 0)
                return "test1@gmail.com"; // Editor gốc
            else
                return $"test{cloneNum + 1}@gmail.com"; // Clone 1 → test2@gmail.com, Clone 2 → test3@gmail.com
        }

        /// <summary>
        /// Lấy test account password (giống nhau cho tất cả)
        /// </summary>
        public static string GetTestPassword()
        {
            return "123456";
        }

        /// <summary>
        /// Log thông tin clone
        /// </summary>
        public static void LogCloneInfo()
        {
            bool isClone = IsClone();
            int cloneNum = GetCloneNumber();
            string email = GetTestEmail();
            
            if (isClone)
            {
                Debug.Log($"[ParrelSync] Running on Clone {cloneNum}");
                Debug.Log($"[ParrelSync] Test Account: {email}");
            }
            else
            {
                Debug.Log($"[ParrelSync] Running on Main Editor");
                Debug.Log($"[ParrelSync] Test Account: {email}");
            }
        }

        /// <summary>
        /// Lấy player name dựa trên clone number
        /// </summary>
        public static string GetTestPlayerName()
        {
            int cloneNum = GetCloneNumber();
            
            if (cloneNum == 0)
                return "Player1"; // Editor gốc
            else
                return $"Player{cloneNum + 1}"; // Clone 1 → Player2, Clone 2 → Player3
        }
    }
}

