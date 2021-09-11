using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public struct TipsMessage
    {
        public string Text;
        public UnityAction Callback;
    }
    
    public class UIBoxManager
    {
        public enum DefUIStatus
        {
            None,
            Tips
        }

        public struct UIBox
        {
            [NotNull] public object Status;
            public GameObject ShowInst;
            public UnityAction CallBack;
        }

        private readonly List<UIBox> _uiBoxes;

        private GameObject _currentUIBox;

        [NotNull] private object _currentUIStatus = DefUIStatus.None;
        [NotNull] private object _updateUIStatus = DefUIStatus.None;

        private readonly Tips _tips;

        private readonly Queue<UnityAction> _unityActionQueue = new Queue<UnityAction>();

        public UIBoxManager()
        {
            var transform = GameObject.Find("Canvas").transform;
            var tipsPrefab = Resources.Load<GameObject>("Prefabs/Tips");
            var tipsGameObj = Object.Instantiate(tipsPrefab, transform);
            tipsGameObj.SetActive(false);
            _tips = new Tips(tipsGameObj);
            
            _uiBoxes = new List<UIBox>();
            RegistBox(new UIBox {Status = DefUIStatus.None});
            RegistBox(new UIBox
            {
                Status = DefUIStatus.Tips,
                ShowInst = tipsGameObj
            });
        }

        public void RegistBox(UIBox uiBox) => _uiBoxes.Add(uiBox);

        public void OnUpdate()
        {
            while (_unityActionQueue.Count > 0)
            {
                var action = _unityActionQueue.Dequeue();
                action.Invoke();
            }

            if (_currentUIStatus.Equals(_updateUIStatus))
                return;

            if (_currentUIBox)
                _currentUIBox.SetActive(false);

            var uiBox = _uiBoxes.Find(box => box.Status.Equals(_updateUIStatus));
            _currentUIBox = uiBox.ShowInst;
            if (_currentUIBox)
            {
                _currentUIBox.SetActive(true);
                uiBox.CallBack?.Invoke();
            }

            _currentUIStatus = _updateUIStatus;
        }

        public bool IsStatus(object status) => _currentUIStatus.Equals(status);

        public void UpdateStatus(object status) => _updateUIStatus = status;

        public void ShowTips(TipsMessage tipsMessage)
        {
            _unityActionQueue.Enqueue(_tips.ShowTips(tipsMessage));
            UpdateStatus(DefUIStatus.Tips);
        }

        public void RunUnityAction(UnityAction action) => _unityActionQueue.Enqueue(action);

        private class Tips
        {
            private readonly Text _tipsText;
            private readonly Button _tipsButton;

            internal Tips(GameObject tipsUIBox)
            {
                _tipsText = tipsUIBox.transform.Find("Text").GetComponent<Text>();
                _tipsButton = tipsUIBox.transform.Find("Button").GetComponent<Button>();
            }

            internal UnityAction ShowTips(TipsMessage tipsMessage)
            {
                return () =>
                {
                    _tipsText.text = tipsMessage.Text;
                    _tipsButton.onClick.RemoveAllListeners();
                    if (tipsMessage.Callback != null)
                    {
                        _tipsButton.onClick.AddListener(tipsMessage.Callback);
                    }
                };
            }
        }
    }
}