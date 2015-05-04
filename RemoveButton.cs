using System;
using System.Collections.Generic;
using System.Threading;

using ICities;
using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using ColossalFramework.Threading;
using ColossalFramework.UI;
using UnityEngine;

namespace RemoveStuckVehicles
{
    internal class RemoveButton : MonoBehaviour
    {
        private Settings _settings;
        private Helper _helper;

        private List<UIButton> _buttons;

        private void Awake()
        {
            _settings = Settings.Instance;
            _helper = Helper.Instance;

            _buttons = new List<UIButton>();
        }

        private void Start()
        {
            foreach (UIDynamicPanels.DynamicPanelInfo dynamicPanel in UIView.library.m_DynamicPanels)
            {
                VehicleWorldInfoPanel[] panels = dynamicPanel.instance.gameObject.GetComponents<VehicleWorldInfoPanel>();

                foreach (VehicleWorldInfoPanel panel in panels)
                {
                    UIButton button = panel.component.AddUIComponent<UIButton>();

                    button.eventClick += RemoveButtonHandler;

                    button.text = "Remove Vehicle";
                    button.name = String.Format("{0} :: {1}", _settings.Tag, button.text);
                    button.autoSize = true;

                    UIButton target = panel.Find<UIButton>("Target");

                    button.font              = target.font;
                    button.textScale         = target.textScale;
                    button.textColor         = target.textColor;
                    button.disabledTextColor = target.disabledTextColor;
                    button.hoveredTextColor  = target.hoveredTextColor;
                    button.focusedTextColor  = target.focusedTextColor;
                    button.pressedTextColor  = target.pressedTextColor;

                    button.useDropShadow     = target.useDropShadow;
                    button.dropShadowColor   = target.dropShadowColor;
                    button.dropShadowOffset  = target.dropShadowOffset;

                    button.AlignTo(panel.Find<UILabel>("Type"), UIAlignAnchor.TopRight);
                    button.relativePosition += new Vector3(button.width + 7, 0);

                    _buttons.Add(button);
                }
            }
        }

        private void OnDestroy()
        {
            foreach (UIButton button in _buttons)
                UnityEngine.Object.Destroy(button.gameObject);
        }

        private void RemoveButtonHandler(UIComponent component, UIMouseEventParameter param)
        {
            ushort vehicle = WorldInfoPanel.GetCurrentInstanceID().Vehicle;

            if (vehicle == 0) return;

            _helper.ManualRemovalRequests.Add(vehicle);
        }
    }
}
