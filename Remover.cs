using System;
using System.Collections.Generic;
using System.Threading;

using ICities;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using UnityEngine;

namespace RemoveStuckVehicles
{
    public class Remover : ThreadingExtensionBase
    {
        private Settings _settings;
        private Helper _helper;

        private string _confused = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CONFUSED");
        private InstanceID _selected;
        private InstanceID _dummy;

        private bool _initialized;
        private bool _terminated;

        protected bool IsOverwatched()
        {
            #if DEBUG

            return true;

            #else

            foreach (var plugin in PluginManager.instance.GetPluginsInfo())
            {
                if (plugin.publishedFileID.AsUInt64 == 421028969)
                    return true;
            }

            return false;

            #endif
        }

        public override void OnCreated(IThreading threading)
        {
            _settings = Settings.Instance;
            _helper = Helper.Instance;

            _selected = default(InstanceID);
            _dummy = default(InstanceID);

            _initialized = false;
            _terminated = false;

            base.OnCreated(threading);
        }

        public override void OnBeforeSimulationTick()
        {
            if (_terminated) return;

            if (!_helper.GameLoaded)
            {
                _initialized = false;
                return;
            }

            base.OnBeforeSimulationTick();
        }

        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (_terminated) return;

            if (!_helper.GameLoaded) return;

            try
            {
                if (!_initialized)
                {
                    if (!IsOverwatched())
                    {
                        _helper.NotifyPlayer("Skylines Overwatch not found. Terminating...");
                        _terminated = true;

                        return;
                    }

                    SkylinesOverwatch.Settings.Instance.Enable.VehicleMonitor = true;

                    _initialized = true;

                    _helper.NotifyPlayer("Initialized");
                }
                else
                {
                    if (WorldInfoPanel.AnyWorldInfoPanelOpen())
                    {
                        _selected = WorldInfoPanel.GetCurrentInstanceID();

                        if (_selected.IsEmpty || _selected.Vehicle == 0)
                            _selected = default(InstanceID);
                    }
                    else
                        _selected = default(InstanceID);
                    
                    VehicleManager instance = Singleton<VehicleManager>.instance;
                    InstanceID instanceID = new InstanceID();
                    SkylinesOverwatch.Data data = SkylinesOverwatch.Data.Instance;

                    foreach (ushort i in data.VehiclesUpdated)
                    {
                        Vehicle v = instance.m_vehicles.m_buffer[(int)i];

                        bool isBlocked = !data.IsCar(i) && v.m_blockCounter == 255; // we will let the game decide when to remove a blocked car
                        bool isConfused = v.Info.m_vehicleAI.GetLocalizedStatus(i, ref v, out instanceID) == _confused; 

                        if (!isBlocked && !isConfused)
                            continue;

                        RemoveVehicle(i);
                    }

                    foreach (ushort i in _helper.ManualRemovalRequests)
                        RemoveVehicle(i);

                    _helper.ManualRemovalRequests.Clear();
                }
            }
            catch (Exception e)
            {
                string error = String.Format("Failed to {0}\r\n", !_initialized ? "initialize" : "update");
                error += String.Format("Error: {0}\r\n", e.Message);
                error += "\r\n";
                error += "==== STACK TRACE ====\r\n";
                error += e.StackTrace;

                _helper.Log(error);

                if (!_initialized)
                    _terminated = true;
            }

            base.OnUpdate(realTimeDelta, simulationTimeDelta);
        }

        public override void OnReleased ()
        {
            _initialized = false;
            _terminated = false;

            base.OnReleased();
        }

        private void RemoveVehicle(ushort vehicle)
        {
            if (!_selected.IsEmpty && _selected.Vehicle == vehicle)
            {
                WorldInfoPanel.HideAllWorldInfoPanels();

                if (!InstanceManager.IsValid(_dummy) || _dummy.Vehicle == vehicle)
                {
                    _dummy = default(InstanceID);
                    _dummy.Type = InstanceType.Vehicle;

                    foreach (ushort i in SkylinesOverwatch.Data.Instance.Vehicles)
                    {
                        if (i == vehicle) continue;

                        _dummy.Vehicle = i;
                        break;
                    }
                }

                Singleton<InstanceManager>.instance.SelectInstance(_dummy);
                Singleton<InstanceManager>.instance.FollowInstance(_dummy);
            }

            VehicleManager instance = Singleton<VehicleManager>.instance;

            HashSet<ushort> removals = new HashSet<ushort>();
            ushort current = vehicle;

            while (current != 0)
            {
                removals.Add(current);

                current = instance.m_vehicles.m_buffer[(int)current].m_trailingVehicle;
            }

            current = instance.m_vehicles.m_buffer[(int)vehicle].m_firstCargo;

            while (current != 0)
            {
                removals.Add(current);

                current = instance.m_vehicles.m_buffer[(int)current].m_nextCargo;
            }

            foreach (ushort i in removals)
            {
                try
                {
                    instance.ReleaseVehicle(i);
                }
                catch (Exception e)
                {
                    string error = String.Format("Failed to release {0}\r\n", i);
                    error += String.Format("Error: {0}\r\n", e.Message);
                    error += "\r\n";
                    error += "==== STACK TRACE ====\r\n";
                    error += e.StackTrace;

                    _helper.Log(error);
                }
            }

            SkylinesOverwatch.Helper.Instance.RequestVehicleRemoval(vehicle);
        }
    }
}

