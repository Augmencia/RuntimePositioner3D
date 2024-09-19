using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Augmencia.RuntimePositioner3D
{
    public class Positioner3D : MonoBehaviour
    {
        private const float k_ScalerRingFactor = 2.099f;

        [SerializeField]
        private Transform _manipulatedObject = null;
        public Transform ManipulatedObject
        {
            get { return _manipulatedObject; }
            set
            {
                if (value == null)
                {
                    gameObject.transform.SetParent(null);
                    gameObject.SetActive(false);
                }
                else
                {
                    gameObject.SetActive(true);
                    transform.SetParent(value);
                    transform.localScale = Vector3.one;
                    transform.localRotation = Quaternion.identity;
                    transform.localPosition = Vector3.zero;
                    _manipulatedObject = value;
                }
            }
        }

        [SerializeField]
        private Material _selectionMaterial;

        [SerializeField]
        private AxisBase[] _axises;

        [SerializeField]
        private ScalerRing _scalerRing;
        public ScalerRing ScalerRing => _scalerRing;

        [SerializeField]
        private bool _useLocalSpace = false;
        public bool UseLocalSpace 
        {
            get => _useLocalSpace;
            set => _useLocalSpace = value;
        }

        [SerializeField]
        private float _scalerRingRadius = .15f;
        public float ScalerRingRadius 
        {
            get => ScalerRing.transform.localScale.x;
            set
            {
                float v = value * 2f / k_ScalerRingFactor;
                ScalerRing.transform.localScale = new Vector3(v, v, v);
                foreach (TranslatorAxis translatorAxis in _axises.Select(a => a.GetComponent<TranslatorAxis>()).Where(a => a != null))
                {
                    translatorAxis.transform.localPosition = translatorAxis.Axis * (value + .05f);
                }
            }
        }

        internal Camera Camera { get; private set; }

        private AxisBase _selectedAxis = null;
        private InputAction _pointAction;
        private InputAction _clickAction;
        private List<RaycastResult> _uiRaycastResults;

        void Start()
        {
            Camera = Camera.main;
            ScalerRingRadius = _scalerRingRadius;

            List<InputAction> actions = InputSystem.ListEnabledActions();
            _pointAction = actions.First(a => a.name == "Point");
            _clickAction = actions.First(a => a.name == "Click");
            _uiRaycastResults = new List<RaycastResult>();

            foreach (AxisBase axis in _axises)
            {
                axis.Initialize(this);
            }
        }

        void Update()
        {
            if (_clickAction.WasPressedThisFrame() && !EventSystem.current.alreadySelecting && _selectedAxis == null)
            {
                Vector2 screenPoint = _pointAction.ReadValue<Vector2>();
                Ray screenRay = Camera.ScreenPointToRay(screenPoint);
                RaycastHit[] hits = Physics.RaycastAll(screenRay, Camera.farClipPlane, 1 << gameObject.layer);
                if (hits.Length > 0)
                {
                    EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { pointerId = -1, position = screenPoint }, _uiRaycastResults);
                    bool isValidHit;
                    foreach (RaycastHit hit in hits)
                    {
                        // Ensure there is no UI element in front of us
                        isValidHit = true;
                        foreach (RaycastResult result in _uiRaycastResults)
                        {
                            if (result.distance < hit.distance)
                            {
                                isValidHit = false;
                                break;
                            }
                        }

                        if (isValidHit)
                        {
                            foreach (AxisBase axis in _axises)
                            {
                                if (axis.TryStartDragging(hit))
                                {
                                    _Select(axis);
                                    break;
                                }
                            }
                        }

                        if (_selectedAxis != null)
                        {
                            break;
                        }
                    }
                }
            }
            else if (_selectedAxis != null)
            {
                if (_clickAction.WasReleasedThisFrame())
                {
                    _Select(null);
                }
                else
                {
                    Vector2 screenPoint = _pointAction.ReadValue<Vector2>();
                    _selectedAxis.Drag(screenPoint);
                }
            }
        }

        void LateUpdate()
        {
            // Constant screen size
            if (_selectedAxis != ScalerRing && Camera != null)
            {
                // Rescale
                float cameraDistance = Mathf.Abs(Camera.WorldToScreenPoint(transform.position).z);
                float scale = cameraDistance / 5f;
                transform.localScale = new Vector3(scale / transform.parent.lossyScale.x, scale / transform.parent.lossyScale.y, scale / transform.parent.lossyScale.z);
            }

            // Update space
            if (_useLocalSpace)
            {
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                transform.rotation = Quaternion.identity;
            }
        }

        private void _Select(AxisBase axis)
        {
            if (axis == null)
            {
                foreach (AxisBase ax in _axises)
                {
                    ax.ResetMaterial();
                    ax.gameObject.SetActive(true);
                }
            }
            else
            {
                foreach (AxisBase ax in _axises)
                {
                    if (ax == axis)
                    {
                        axis.SetMaterial(_selectionMaterial);
                        ax.gameObject.SetActive(true);
                    }
                    else
                    {
                        ax.gameObject.SetActive(false);
                    }
                }
            }
            _selectedAxis = axis;
        }
    }
}