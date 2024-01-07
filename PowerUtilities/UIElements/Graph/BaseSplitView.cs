using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PowerUtilities.UIElements
{
    public class BaseSplitView : TwoPaneSplitView
    {
        public float paneFlowRate = 1f;
        public new class UxmlFactory : UxmlFactory<BaseSplitView, UxmlTraits> { };
        public new class UxmlTraits : TwoPaneSplitView.UxmlTraits
        {

            private UxmlIntAttributeDescription _PanelFlowRate = new UxmlIntAttributeDescription
            {
                name = "pane-flow-percent",
                defaultValue = 100,
            };
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                var panelFlowPercent = _PanelFlowRate.GetValueFromBag(bag, cc);
                base.Init(ve, bag, cc);
                ((BaseSplitView)ve).Init(panelFlowPercent);
            }
        }

        public BaseSplitView()
        {
            RegisterCallback<GeometryChangedEvent>(OnSizeChanged);
        }

        public void Init(int paneFlowPercent)
        {
            var flowRate = Mathf.Clamp01(paneFlowPercent / 100f);
            this.paneFlowRate = flowRate <= 0 ? 1 : flowRate;
        }

        private void OnSizeChanged(GeometryChangedEvent evt)
        {
            var curView = evt.target as BaseSplitView;
            var rootVE = curView.GetRootElement();
            
            var rootWidth = rootVE.style.width;
            var rootHeight = rootVE.style.height;
            
            if(orientation == TwoPaneSplitViewOrientation.Horizontal)
            {
                curView.style.height = rootHeight.value.value * paneFlowRate;
            }
            else
            {
                curView.style.width = rootWidth.value.value * paneFlowRate;
            }

        }
    }
}