using UnityEngine;
using UnityEngine.XR;

namespace Brasa
{
    /// <summary>Tracking visualization only. No interaction, locomotion or gameplay.</summary>
    public sealed class TrackedControllerVisual : MonoBehaviour
    {
        public XRNode node;
        public GameObject model;
        void OnEnable() { if(model) model.SetActive(false); }
        void Update()
        {
            if(!model)return;
            var device=InputDevices.GetDeviceAtXRNode(node);
            bool visible=device.isValid&&device.TryGetFeatureValue(CommonUsages.isTracked,out bool tracked)&&tracked;
            if(model.activeSelf!=visible)model.SetActive(visible);
        }
    }
}
