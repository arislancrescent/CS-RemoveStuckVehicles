using System;
using System.Collections.Generic;
using System.Threading;

using ICities;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using UnityEngine;

namespace RemoveStuckTrains
{
    public class Remover : ThreadingExtensionBase
    {
        private Settings _settings;
        private Helper _helper;

        private string _confused = ColossalFramework.Globalization.Locale.Get("VEHICLE_STATUS_CONFUSED");

        private bool _initialized;
        private bool _terminated;
        private bool _cleanslate;

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

            _initialized = false;
            _terminated = false;
            _cleanslate = false;

            base.OnCreated(threading);
        }

        public override void OnBeforeSimulationTick()
        {
            if (_terminated) return;

            if (!_helper.GameLoaded)
            {
                _initialized = false;
                _cleanslate = false;
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
                        _helper.Log("Skylines Overwatch not found. Terminating...");
                        _terminated = true;

                        return;
                    }

                    SkylinesOverwatch.Settings.Instance.Enable.VehicleMonitor  = true;

                    _initialized = true;
                    _cleanslate = false;

                    _helper.Log("Initialized");
                }
                else if (!_cleanslate)
                {
                    VehicleManager instance = Singleton<VehicleManager>.instance;

                    List<ushort> trains = new List<ushort>();
                    trains.AddRange(SkylinesOverwatch.Data.Instance.PassengerTrains);
                    trains.AddRange(SkylinesOverwatch.Data.Instance.CargoTrains);

                    foreach (ushort i in trains)
                    {
                        instance.ReleaseVehicle(i);

                        SkylinesOverwatch.Helper.Instance.RequestVehicleRemoval(i);
                    }

                    _cleanslate = true;
                }
                else
                {
                    VehicleManager instance = Singleton<VehicleManager>.instance;
                    InstanceID instanceID = new InstanceID();

                    List<ushort> trains = new List<ushort>();
                    trains.AddRange(SkylinesOverwatch.Data.Instance.PassengerTrains);
                    trains.AddRange(SkylinesOverwatch.Data.Instance.CargoTrains);

                    foreach (ushort i in trains)
                    {
                        Vehicle train = instance.m_vehicles.m_buffer[(int)i];

                        bool isBlocked = train.m_blockCounter == 255;
                        bool isConfused = train.Info.m_vehicleAI.GetLocalizedStatus(i, ref train, out instanceID) == _confused; 

                        if (isBlocked || isConfused)
                        {
                            instance.ReleaseVehicle(i);

                            SkylinesOverwatch.Helper.Instance.RequestVehicleRemoval(i);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string error = "Failed to initialize\r\n";
                error += String.Format("Error: {0}\r\n", e.Message);
                error += "\r\n";
                error += "==== STACK TRACE ====\r\n";
                error += e.StackTrace;

                _helper.Log(error);

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
    }
}

