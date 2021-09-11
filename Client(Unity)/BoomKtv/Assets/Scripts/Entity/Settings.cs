using UnityEngine;

namespace Entity
{
    public static class Settings
    {
        public static string Uid
        {
            get => PlayerPrefs.GetString(nameof(Uid));
            set
            {
                PlayerPrefs.SetString(nameof(Uid), value);
                PlayerPrefs.Save();
            }
        }

        public static string Key
        {
            get => PlayerPrefs.GetString(nameof(Key));
            set
            {
                PlayerPrefs.SetString(nameof(Key), value);
                PlayerPrefs.Save();
            }
        }

        public static string Ip
        {
            get => PlayerPrefs.GetString(nameof(Ip));
            set
            {
                PlayerPrefs.SetString(nameof(Ip), value);
                PlayerPrefs.Save();
            }
        }

        public static int Port
        {
            get => PlayerPrefs.GetInt(nameof(Port));
            set
            {
                PlayerPrefs.SetInt(nameof(Port), value);
                PlayerPrefs.Save();
            }
        }

        public static bool CheckAndInitValues()
        {
            if (IsInit) return true;

            Uid = "";
            Key = "hanhansidiudiu";
#if UNITY_EDITOR
            Ip = "127.0.0.1";
#else
            Ip = "8.129.170.99";
#endif
            Port = 9999;
            IsInit = true;
            return false;
        }

        private static bool IsInit
        {
            get => PlayerPrefs.HasKey(nameof(IsInit));
            set
            {
                if (value) 
                    PlayerPrefs.SetInt(nameof(IsInit), 1);
                else 
                    PlayerPrefs.DeleteKey(nameof(IsInit));
                PlayerPrefs.Save();
            }
        }
    }
}