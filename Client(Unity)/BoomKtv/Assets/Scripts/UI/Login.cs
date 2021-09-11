using Entity;
using Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class Login : MonoBehaviour
    {
        private enum UIStatus
        {
            Setting,
            Connecting
        }
        
        private readonly Client _client = Client.Instance;

        private UIBoxManager _uiBoxManager;

        private GameObject _settingUIBox;
        private InputField _uidInput;
        private InputField _keyInput;
        private InputField _ipInput;
        private InputField _portInput;

        private GameObject _connectUIBox;

        #region RenderEvents

        private void OnEnable()
        {
            _client.OnConnect += OnConnect;
            _client.OnAuthResult += OnAuthResult;
        }

        private void OnDestroy()
        {
            _client.OnConnect -= OnConnect;
            _client.OnAuthResult -= OnAuthResult;
        }

        private void Start()
        {
            _settingUIBox = transform.Find("Settings").gameObject;
            _uidInput = _settingUIBox.transform.Find("Uid").GetComponentInChildren<InputField>();
            _keyInput = _settingUIBox.transform.Find("Key").GetComponentInChildren<InputField>();
            _ipInput = _settingUIBox.transform.Find("IP").GetComponentInChildren<InputField>();
            _portInput = _settingUIBox.transform.Find("Port").GetComponentInChildren<InputField>();
            
            _uiBoxManager = new UIBoxManager();
            _uiBoxManager.RegistBox(new UIBoxManager.UIBox
            {
                Status = UIStatus.Setting,
                ShowInst = _settingUIBox
            });
            _uiBoxManager.RegistBox(new UIBoxManager.UIBox
            {
                Status = UIStatus.Connecting,
                ShowInst = transform.Find("Connecting").gameObject,
                CallBack = () => _client.Connect(Settings.Ip, Settings.Port)
            });

            if (Settings.CheckAndInitValues()) return;

            _uiBoxManager.ShowTips(new TipsMessage
            {
                Text = "欢迎来到BoomKtv~初次使用，请先设置一下吧~",
                Callback = ShowSettings
            });
        }

        private void Update()
        {
            _uiBoxManager.OnUpdate();
            
            if (_uiBoxManager.IsStatus(UIBoxManager.DefUIStatus.None))
                _uiBoxManager.UpdateStatus(UIStatus.Connecting);
        }

        #endregion

        #region NetworkEvents

        private void OnConnect(bool result)
        {
            if (result)
            {
                _uiBoxManager.RunUnityAction(() =>
                {
                    _client.RequestAuth(Settings.Uid, Settings.Key);
                });
            }
            else if (_uiBoxManager.IsStatus(UIStatus.Connecting))
            {
                _uiBoxManager.ShowTips(new TipsMessage
                {
                    Text = "连接失败了qwq",
                    Callback = ShowSettings
                });
            }
        }

        private void OnAuthResult(AuthResult result)
        {
            string text = null;
            switch (result)
            {
                case AuthResult.Success:
                    _uiBoxManager.RunUnityAction(() => SceneManager.LoadScene("RoomScene"));
                    return;
                case AuthResult.KeyErr:
                    text = "连接失败~key错了鸭";
                    break;
                case AuthResult.UidNotFound:
                    text = "连接失败~这个uid用户不存在鸭";
                    break;
                case AuthResult.UserIsOnline:
                    text = "连接失败~这个uid用户已经在线了鸭~";
                    break;
            }

            if (text != null)
            {
                _uiBoxManager.ShowTips(new TipsMessage
                {
                    Callback = ShowSettings,
                    Text = text
                });
            }
        }

        #endregion

        #region Actions

        public void ShowSettings()
        {
            _uidInput.text = Settings.Uid;
            _keyInput.text = Settings.Key;
            _ipInput.text = Settings.Ip;
            _portInput.text = Settings.Port.ToString();
            _uiBoxManager.UpdateStatus(UIStatus.Setting);
        }

        public void HideSettings() => _uiBoxManager.UpdateStatus(UIBoxManager.DefUIStatus.None);

        public void HideAndSaveSettings()
        {
            Settings.Uid = _uidInput.text;
            Settings.Key = _keyInput.text;
            Settings.Ip = _ipInput.text;
            Settings.Port = int.Parse(_portInput.text);
            HideSettings();
        }

        #endregion
    }
}