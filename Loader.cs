using System;
using System.Collections.Generic;
using System.Threading;

using ICities;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;
using UnityEngine;

namespace RemoveStuckVehicles
{
    public class Loader : LoadingExtensionBase
    {
        private Settings _settings;
        private Helper _helper;

        private GameObject _gameObject;
        private RemoveButton _removeButton;

        public override void OnCreated(ILoading loading)
        {
            _settings = Settings.Instance;
            _helper = Helper.Instance;

            _helper.GameLoaded = loading.loadingComplete;
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame) return;

            _helper.GameLoaded = true;

            _gameObject = new GameObject(_settings.Tag);
            _removeButton = _gameObject.AddComponent<RemoveButton>();
        }

        public override void OnLevelUnloading()
        {
            _helper.GameLoaded = false;

            if (_removeButton != null) UnityEngine.Object.Destroy(_removeButton);
            if (_gameObject   != null) UnityEngine.Object.Destroy(_gameObject);
        }
    }
}