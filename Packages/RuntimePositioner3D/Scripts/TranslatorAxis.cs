using System.Linq;
using UnityEngine;

namespace Augmencia.RuntimePositioner3D
{
    public class TranslatorAxis : AxisBase
    {
        [SerializeField] private Collider _collider;
        [SerializeField] private MeshRenderer[] _meshRenderers;
        [SerializeField] private Vector3 _axis;

        private Positioner3D _positioner;
        private float _offsetScaledDistance;
        private int _axisIndex = 0;
        private Material _originalMaterial;

        internal override void Initialize(Positioner3D positioner)
        {
            _positioner = positioner;

            for (int i = 0; i < 3; ++i)
            {
                if (_axis[i] != 0)
                {
                    _axisIndex = i;
                    break;
                }
            }

            _originalMaterial = _meshRenderers[0].material;
        }

        internal override void SetMaterial(Material material)
        {
            foreach (MeshRenderer meshRenderer in _meshRenderers)
            {
                meshRenderer.material = material;
            }
        }

        internal override void ResetMaterial()
        {
            SetMaterial(_originalMaterial);
        }

        internal override bool TryStartDragging(RaycastHit hit)
        {
            if (_collider == hit.collider)
            {
                Ray screenRay = new Ray(_positioner.Camera.transform.position, (hit.point - _positioner.Camera.transform.position).normalized);
                for (int i = 0; i < 3; ++i)
                {
                    if (i != _axisIndex)
                    {
                        Vector3 normal = new Vector3();
                        normal[i] = 1;
                        Plane plane = new Plane(_positioner.transform.TransformDirection(normal), _positioner.transform.position);
                        if (plane.Raycast(screenRay, out float distance))
                        {
                            Vector3 point = screenRay.GetPoint(distance);
                            _offsetScaledDistance = _positioner.transform.InverseTransformPoint(point)[_axisIndex] / transform.lossyScale[_axisIndex];
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        internal override void Drag(Vector3 screenPoint)
        {
            Ray screenRay = _positioner.Camera.ScreenPointToRay(screenPoint);
            for (int i = 0; i < 3; ++i)
            {
                if (i != _axisIndex)
                {
                    Vector3 normal = new Vector3();
                    normal[i] = 1;
                    Plane plane = new Plane(_positioner.transform.TransformDirection(normal), _positioner.transform.position);
                    if (plane.Raycast(screenRay, out float distance))
                    {
                        Vector3 point = screenRay.GetPoint(distance);
                        float axisValue = _positioner.transform.InverseTransformPoint(point)[_axisIndex] - _offsetScaledDistance * transform.lossyScale[_axisIndex];
                        Vector3 pos = Vector3.zero;
                        pos[_axisIndex] = axisValue;
                        _positioner.ManipulatedObject.position = _positioner.transform.TransformPoint(pos);
                        break;
                    }
                }
            }
        }
    }
}