using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.XR;
using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;

namespace Brasa
{
    /// <summary>
    /// VR point-and-teleport locomotion for Quest / OpenXR.
    /// Hold thumbstick forward or index trigger on either controller to aim a parabolic arc onto the restaurant floor;
    /// release to teleport to the pointed valid floor location.
    /// </summary>
    public sealed class PointTeleportLocomotion : MonoBehaviour
    {
        [Header("Rig References")]
        public XROrigin origin;
        public Transform leftController;
        public Transform rightController;

        [Header("Visual Materials")]
        public Material validArcMaterial;
        public Material invalidArcMaterial;

        [Header("Arc & Teleport Settings")]
        public float arcSpeed = 6.8f;
        public float arcGravity = 9.81f;
        public int arcSteps = 36;
        public float arcStepTime = 0.035f;
        public float clearanceRadius = 0.24f;

        private LineRenderer arcRenderer;
        private Transform reticleTransform;
        private MeshRenderer reticleOuterRenderer;
        private MeshRenderer reticleInnerRenderer;

        private InputAction leftTriggerAction;
        private InputAction rightTriggerAction;
        private InputAction leftStickAction;
        private InputAction rightStickAction;

        private int activeHand = -1; // -1 = none, 0 = left, 1 = right
        private bool hasValidTarget;
        private Vector3 currentTargetPoint;
        private Vector3[] arcPoints;

        private static readonly Vector2 LeftPlantPos = new(-5.35f, -4.6f);
        private static readonly Vector2 RightPlantPos = new(5.25f, -4.5f);

        void Awake()
        {
            ResolveReferences();
            arcPoints = new Vector3[arcSteps + 1];
            SetupInputActions();
            CreateVisuals();
            SetVisualsVisible(false);
        }

        void OnEnable()
        {
            leftTriggerAction?.Enable();
            rightTriggerAction?.Enable();
            leftStickAction?.Enable();
            rightStickAction?.Enable();
        }

        void OnDisable()
        {
            leftTriggerAction?.Disable();
            rightTriggerAction?.Disable();
            leftStickAction?.Disable();
            rightStickAction?.Disable();
            activeHand = -1;
            hasValidTarget = false;
            SetVisualsVisible(false);
        }

        void OnDestroy()
        {
            leftTriggerAction?.Dispose();
            rightTriggerAction?.Dispose();
            leftStickAction?.Dispose();
            rightStickAction?.Dispose();
        }

        private void ResolveReferences()
        {
            if (!origin) origin = GetComponent<XROrigin>();
            if (!origin) origin = FindFirstObjectByType<XROrigin>();
            if (!origin) return;

            if (!leftController)
            {
                var t = origin.transform.Find("Camera Offset/LeftHand");
                if (t) leftController = t;
            }
            if (!rightController)
            {
                var t = origin.transform.Find("Camera Offset/RightHand");
                if (t) rightController = t;
            }
        }

        private void SetupInputActions()
        {
            leftTriggerAction = new InputAction("LeftTeleportTrigger", InputActionType.Value);
            leftTriggerAction.AddBinding("<XRController>{LeftHand}/trigger");
            leftTriggerAction.AddBinding("<XRController>{LeftHand}/triggerPressed");

            rightTriggerAction = new InputAction("RightTeleportTrigger", InputActionType.Value);
            rightTriggerAction.AddBinding("<XRController>{RightHand}/trigger");
            rightTriggerAction.AddBinding("<XRController>{RightHand}/triggerPressed");

            leftStickAction = new InputAction("LeftTeleportStick", InputActionType.Value);
            leftStickAction.AddBinding("<XRController>{LeftHand}/thumbstick");
            leftStickAction.AddBinding("<XRController>{LeftHand}/primary2DAxis");

            rightStickAction = new InputAction("RightTeleportStick", InputActionType.Value);
            rightStickAction.AddBinding("<XRController>{RightHand}/thumbstick");
            rightStickAction.AddBinding("<XRController>{RightHand}/primary2DAxis");
        }

        private void CreateVisuals()
        {
            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (!unlitShader) unlitShader = Shader.Find("Sprites/Default");

            if (!validArcMaterial)
            {
                validArcMaterial = new Material(unlitShader) { name = "TeleportValidRuntime" };
                validArcMaterial.color = new Color(0.87f, 0.66f, 0.25f, 1f);
            }
            if (!invalidArcMaterial)
            {
                invalidArcMaterial = new Material(unlitShader) { name = "TeleportInvalidRuntime" };
                invalidArcMaterial.color = new Color(0.72f, 0.25f, 0.24f, 1f);
            }

            var arcObject = new GameObject("TeleportArc");
            arcObject.transform.SetParent(transform, false);
            arcRenderer = arcObject.AddComponent<LineRenderer>();
            arcRenderer.useWorldSpace = true;
            arcRenderer.widthMultiplier = 0.014f;
            arcRenderer.numCapVertices = 4;
            arcRenderer.numCornerVertices = 4;
            arcRenderer.shadowCastingMode = ShadowCastingMode.Off;
            arcRenderer.receiveShadows = false;
            arcRenderer.lightProbeUsage = LightProbeUsage.Off;
            arcRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            arcRenderer.sharedMaterial = validArcMaterial;

            var reticleRoot = new GameObject("TeleportReticle");
            reticleTransform = reticleRoot.transform;
            reticleTransform.SetParent(transform, false);

            var outerDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            outerDisc.name = "ReticleOuter";
            Destroy(outerDisc.GetComponent<Collider>());
            outerDisc.transform.SetParent(reticleTransform, false);
            outerDisc.transform.localPosition = new Vector3(0f, 0.006f, 0f);
            outerDisc.transform.localScale = new Vector3(0.42f, 0.003f, 0.42f);
            reticleOuterRenderer = outerDisc.GetComponent<MeshRenderer>();
            reticleOuterRenderer.shadowCastingMode = ShadowCastingMode.Off;
            reticleOuterRenderer.receiveShadows = false;
            reticleOuterRenderer.sharedMaterial = validArcMaterial;

            var innerDisc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            innerDisc.name = "ReticleInner";
            Destroy(innerDisc.GetComponent<Collider>());
            innerDisc.transform.SetParent(reticleTransform, false);
            innerDisc.transform.localPosition = new Vector3(0f, 0.010f, 0f);
            innerDisc.transform.localScale = new Vector3(0.14f, 0.003f, 0.14f);
            reticleInnerRenderer = innerDisc.GetComponent<MeshRenderer>();
            reticleInnerRenderer.shadowCastingMode = ShadowCastingMode.Off;
            reticleInnerRenderer.receiveShadows = false;
            reticleInnerRenderer.sharedMaterial = validArcMaterial;
        }

        void Update()
        {
            if (!origin) return;

            GetHandInput(0, out bool leftPressed, out bool leftReleased, out bool leftCancelled);
            GetHandInput(1, out bool rightPressed, out bool rightReleased, out bool rightCancelled);

            if (activeHand == -1)
            {
                if (rightPressed && rightController) activeHand = 1;
                else if (leftPressed && leftController) activeHand = 0;
            }

            if (activeHand == -1)
            {
                SetVisualsVisible(false);
                return;
            }

            Transform handTransform = activeHand == 1 ? rightController : leftController;
            bool released = activeHand == 1 ? rightReleased : leftReleased;
            bool cancelled = activeHand == 1 ? rightCancelled : leftCancelled;

            if (!handTransform || cancelled)
            {
                activeHand = -1;
                hasValidTarget = false;
                SetVisualsVisible(false);
                return;
            }

            UpdateAimArc(handTransform);

            if (released)
            {
                if (hasValidTarget)
                {
                    TeleportTo(currentTargetPoint);
                }
                activeHand = -1;
                hasValidTarget = false;
                SetVisualsVisible(false);
            }
        }

        private void GetHandInput(int handIndex, out bool pressed, out bool released, out bool cancelled)
        {
            XRNode node = handIndex == 0 ? XRNode.LeftHand : XRNode.RightHand;
            InputDevice device = InputDevices.GetDeviceAtXRNode(node);

            float triggerVal = 0f;
            Vector2 stickVal = Vector2.zero;

            if (device.isValid)
            {
                if (device.TryGetFeatureValue(CommonUsages.trigger, out float devTrigger))
                    triggerVal = Mathf.Max(triggerVal, devTrigger);
                if (device.TryGetFeatureValue(CommonUsages.triggerButton, out bool devTriggerBtn) && devTriggerBtn)
                    triggerVal = Mathf.Max(triggerVal, 1f);
                if (device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 devStick))
                    stickVal = devStick;
            }

            InputAction trigAct = handIndex == 0 ? leftTriggerAction : rightTriggerAction;
            InputAction stickAct = handIndex == 0 ? leftStickAction : rightStickAction;

            if (trigAct != null)
                triggerVal = Mathf.Max(triggerVal, trigAct.ReadValue<float>());

            if (stickAct != null)
            {
                Vector2 actStick = stickAct.ReadValue<Vector2>();
                if (actStick.sqrMagnitude > stickVal.sqrMagnitude)
                    stickVal = actStick;
            }

            cancelled = stickVal.y < -0.55f;
            pressed = !cancelled && (triggerVal > 0.55f || stickVal.y > 0.55f);
            released = !cancelled && (triggerVal < 0.25f && stickVal.y < 0.25f);
        }

        private void UpdateAimArc(Transform hand)
        {
            Vector3 startPos = hand.position + hand.forward * 0.04f;
            Vector3 velocity = hand.forward * arcSpeed;
            Vector3 gravity = new Vector3(0f, -arcGravity, 0f);

            arcPoints[0] = startPos;
            int pointCount = 1;
            hasValidTarget = false;

            Vector3 prevPoint = startPos;
            for (int i = 1; i <= arcSteps; i++)
            {
                float t = i * arcStepTime;
                Vector3 nextPoint = startPos + velocity * t + 0.5f * gravity * (t * t);

                if (Physics.Linecast(prevPoint, nextPoint, out RaycastHit hit, ~0, QueryTriggerInteraction.Ignore))
                {
                    arcPoints[pointCount++] = hit.point;
                    if (IsValidTeleportPoint(hit))
                    {
                        hasValidTarget = true;
                        currentTargetPoint = new Vector3(hit.point.x, 0f, hit.point.z);
                    }
                    break;
                }

                arcPoints[pointCount++] = nextPoint;
                prevPoint = nextPoint;
            }

            if (arcRenderer)
            {
                arcRenderer.enabled = true;
                arcRenderer.positionCount = pointCount;
                for (int i = 0; i < pointCount; i++)
                {
                    arcRenderer.SetPosition(i, arcPoints[i]);
                }
                arcRenderer.sharedMaterial = hasValidTarget ? validArcMaterial : invalidArcMaterial;
            }

            if (reticleTransform)
            {
                reticleTransform.gameObject.SetActive(hasValidTarget);
                if (hasValidTarget)
                {
                    reticleTransform.position = currentTargetPoint;
                }
            }
        }

        private bool IsValidTeleportPoint(RaycastHit hit)
        {
            // Must hit a flat upward-facing floor surface at ground level (y ~= 0)
            if (hit.normal.y < 0.75f) return false;
            if (Mathf.Abs(hit.point.y) > 0.06f) return false;

            Vector3 pt = new Vector3(hit.point.x, 0f, hit.point.z);

            // Must be within restaurant interior walls
            if (pt.x < -5.65f || pt.x > 5.65f || pt.z < -4.95f || pt.z > 4.95f)
                return false;

            // Closed WC and storage rooms in the back-left corner
            if (pt.x < -0.70f && pt.z > 3.20f)
                return false;

            // Avoid corner decorative plant pots
            Vector2 pt2D = new Vector2(pt.x, pt.z);
            if (Vector2.Distance(pt2D, LeftPlantPos) < 0.45f || Vector2.Distance(pt2D, RightPlantPos) < 0.45f)
                return false;

            // Ensure player body/head capsule does not collide with tables, chairs, counter, or walls
            Vector3 capsuleBottom = pt + Vector3.up * (clearanceRadius + 0.08f);
            Vector3 capsuleTop = pt + Vector3.up * 1.65f;
            if (Physics.CheckCapsule(capsuleBottom, capsuleTop, clearanceRadius, ~0, QueryTriggerInteraction.Ignore))
                return false;

            return true;
        }

        private void TeleportTo(Vector3 targetFloorPoint)
        {
            Vector3 camPos = origin.Camera ? origin.Camera.transform.position : origin.transform.position;
            Vector3 deltaXZ = new Vector3(targetFloorPoint.x - camPos.x, 0f, targetFloorPoint.z - camPos.z);

            origin.transform.position = new Vector3(
                origin.transform.position.x + deltaXZ.x,
                targetFloorPoint.y,
                origin.transform.position.z + deltaXZ.z
            );
        }

        private void SetVisualsVisible(bool visible)
        {
            if (arcRenderer) arcRenderer.enabled = visible;
            if (reticleTransform) reticleTransform.gameObject.SetActive(visible && hasValidTarget);
        }
    }
}
