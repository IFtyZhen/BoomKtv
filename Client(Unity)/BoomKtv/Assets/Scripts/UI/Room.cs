using System;
using System.Collections.Generic;
using System.Linq;
using Entity;
using Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UI
{
    struct Seat
    {
        public User User;
        public GameObject Inst;
    }

    public class Room : MonoBehaviour
    {
        private readonly Client _client = Client.Instance;

        private UIBoxManager _uiBoxManager;
        private AudioDevice _audioDevice;

        private List<Seat> _seats;
        private Image _micImage;

        private const float SyncTime = 0.5f;
        private float _syncTimeCount;

        #region RenderEvents

        private void OnEnable()
        {
            _client.OnClose += OnClose;
            _client.OnUserListResult += OnUserListResult;
            _client.OnGetRecord += OnGetRecord;
            _client.OnUserEnter += OnUserEnter;
            _client.OnUserExit += OnUserExit;
        }

        private void OnDestroy()
        {
            _client.OnClose -= OnClose;
            _client.OnUserListResult -= OnUserListResult;
            _client.OnGetRecord -= OnGetRecord;
            _client.OnUserEnter -= OnUserEnter;
            _client.OnUserExit -= OnUserExit;

            _audioDevice.StopRecord();
        }

        private void Start()
        {
            _seats = new List<Seat>();
            var childTransform = transform.Find("Seats").transform;
            var len = childTransform.childCount;
            for (var i = 0; i < len; i++)
            {
                _seats.Add(new Seat {Inst = childTransform.GetChild(i).gameObject});
            }

            _micImage = transform.Find("Options/Mic").GetComponent<Image>();

            _uiBoxManager = new UIBoxManager();
            _audioDevice = new AudioDevice();

            _client.RequestUserList();

            CloseMic();
        }

        private void Update()
        {
            _uiBoxManager.OnUpdate();

            if (!_audioDevice.IsRecording()) return;

            _syncTimeCount += Time.deltaTime;
            if (_syncTimeCount >= SyncTime)
            {
                var data = _audioDevice.GetRecordData(SyncTime);
                _client.SendRecord(data);
                OnGetRecord(data);

                _syncTimeCount = 0f;
            }
        }

        public void OnSwitchMic()
        {
            if (_audioDevice.IsRecording()) CloseMic();
            else OpenMic();
        }

        #endregion

        #region NetworkEvents

        private void OnClose()
        {
            _uiBoxManager.RunUnityAction(() => SceneManager.LoadScene("LoginScene"));
        }

        private void OnUserListResult(List<User> users)
        {
            _uiBoxManager.RunUnityAction(() =>
            {
                foreach (var user in users)
                {
                    IntoSeat(user);
                }
            });
        }

        private void OnUserEnter(User user) => _uiBoxManager.RunUnityAction(() => IntoSeat(user));

        private void OnUserExit(User user) => _uiBoxManager.RunUnityAction(() => OutSeat(user));

        private void OnGetRecord(byte[] bytes)
        {
            var audioClip = _audioDevice.BytesToAudio(bytes);
            _audioDevice.PlayAudio(audioClip);
        }

        #endregion

        #region Actions

        private void OpenMic()
        {
            if (!_audioDevice.HasRecordDevices())
            {
                _uiBoxManager.ShowTips(new TipsMessage
                {
                    Text = "麦都不给，唱个龟龟鸭~"
                });
                return;
            }

            _audioDevice.BeginRecord();
            _micImage.color = new Color(255, 255, 255, 255);
        }

        private void CloseMic()
        {
            _audioDevice.StopRecord();
            _micImage.color = new Color(0, 0, 0, 255);
        }

        private void IntoSeat(User user)
        {
            foreach (var s in _seats.Where(seat => seat.User == null))
            {
                var seat = s;
                seat.Inst.GetComponentInChildren<Text>().text = user.Name;
                seat.User = user;
                break;
            }
        }

        private void OutSeat(User user)
        {
            foreach (var s in _seats.Where(seat => seat.User.Uid == user.Uid))
            {
                var seat = s;
                seat.Inst.GetComponentInChildren<Text>().text = "空位";
                seat.User = null;
                break;
            }
        }

        #endregion
    }

    internal class AudioDevice
    {
        private int _freq;

        private AudioClip _recordClip;
        private readonly AudioSource _source;

        private readonly string _device;

        public AudioDevice()
        {
            if (HasRecordDevices())
            {
                _device = Microphone.devices[0];
            }

            _source = Object.FindObjectOfType<AudioSource>();
        }

        public bool HasRecordDevices() => Microphone.devices.Length > 0;

        public bool IsRecording() => Microphone.IsRecording(_device);

        public void BeginRecord()
        {
            if (!HasRecordDevices() || IsRecording())
                return;

            if (_freq == 0)
                Microphone.GetDeviceCaps(_device, out _freq, out _);

            _recordClip = Microphone.Start(_device, true, 3, _freq);
        }

        public void StopRecord()
        {
            if (!IsRecording()) return;

            Microphone.End(_device);
            _recordClip = null;
        }

        public void PlayAudio(AudioClip audioClip)
        {
            _source.clip = audioClip;
            _source.Play();
        }

        public byte[] GetRecordData(float timeLength)
        {
            if (!_recordClip) return null;

            var len = (int)(_freq * timeLength);
            var volumeData = new float[len];

            var offset = Microphone.GetPosition(_device) - len + 1;
            if (offset < 0) offset = 0;

            _recordClip.GetData(volumeData, offset);

            var bytes = new byte[len];
            Buffer.BlockCopy(volumeData, 0, bytes, 0, len);

            return bytes;
        }

        public AudioClip BytesToAudio(byte[] bytes)
        {
            var samples = new float[bytes.Length];
            Buffer.BlockCopy(bytes, 0, samples, 0, bytes.Length);

            var clip = AudioClip.Create("Audio", samples.Length, 1, _freq, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}