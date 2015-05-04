using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;

using ICities;
using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.Math;
using ColossalFramework.UI;
using UnityEngine;

namespace RemoveStuckVehicles
{
    internal sealed class Helper
    {
        private Helper()
        {
            GameLoaded = false;
            ManualRemovalRequests = new HashSet<ushort>();
        }

        private static readonly Helper _Instance = new Helper();
        public static Helper Instance { get { return _Instance; } }

        internal bool GameLoaded;
        internal HashSet<ushort> ManualRemovalRequests;

        public void Log(string message)
        {
            Debug.Log(String.Format("{0}: {1}", Settings.Instance.Tag, message));
        }
    }
}