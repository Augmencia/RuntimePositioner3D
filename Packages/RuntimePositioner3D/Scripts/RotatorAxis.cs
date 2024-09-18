using Unity.Mathematics;
using UnityEngine;

namespace Augmencia.RuntimePositioner3D
{
    public class RotatorAxis : AxisBase
    {
        [SerializeField] private Collider _collider;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Vector3 _axis;

        private Positioner3D _positioner;
        private Vector3 _originalLocalPosition;
        private Quaternion _originalRotation;
        private Material _originalMaterial;

        internal override void Initialize(Positioner3D positioner)
        {
            _positioner = positioner;
            _originalMaterial = _meshRenderer.material;
        }

        internal override void SetMaterial(Material material)
        {
            _meshRenderer.material = material;
        }

        internal override void ResetMaterial()
        {
            SetMaterial(_originalMaterial);
        }

        internal override bool TryStartDragging(RaycastHit hit)
        {
            if (hit.collider == _collider)
            {
                // Ensure the hit is on the visible part of the torus
                float3x3 m = math.float3x3(math.mul(_positioner.Camera.worldToCameraMatrix, hit.collider.transform.localToWorldMatrix));
                float3 x = math.normalize(m[0]);
                float3 y = math.normalize(m[1] - x * math.dot(x, m[1]));
                float3 z = m[2] - x * math.dot(x, m[2]);
                z = math.normalize(z - y * (math.dot(y, m[2])));
                m = math.float3x3(x, y, z);
                float3 n = math.mul(m, hit.collider.transform.InverseTransformPoint(hit.point));
                float orientation = math.dot(math.float3(0,0,-1), n);
                if (orientation <= 0.005)
                {
                    Vector3 worldAxis = _positioner.transform.TransformDirection(_axis).normalized;
                    Plane plane = new Plane(worldAxis, _positioner.transform.position);
                    Ray ray = new Ray(_positioner.Camera.transform.position, (hit.point - _positioner.Camera.transform.position).normalized);
                    if (plane.Raycast(ray, out float distance))
                    {
                        Vector3 destPoint = ray.GetPoint(distance);
                        _originalLocalPosition = _positioner.transform.InverseTransformPoint(destPoint);
                        _originalRotation = _positioner.ManipulatedObject.rotation;
                        return true;
                    }
                }
            }
            return false;
        }

        internal override void Drag(Vector3 screenPoint)
        {
            Vector3 worldAxis = _positioner.transform.TransformDirection(_axis).normalized;
            Plane plane = new Plane(worldAxis, _positioner.transform.position);
            Vector3 originalPosition = _positioner.transform.TransformPoint(_originalLocalPosition);
            Ray ray = _positioner.Camera.ScreenPointToRay(screenPoint);
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 destPoint = ray.GetPoint(distance);
                Vector3 initDir = Vector3.ProjectOnPlane(originalPosition - _positioner.transform.position, worldAxis).normalized;
                Vector3 currentDir = Vector3.ProjectOnPlane(destPoint - _positioner.transform.position, worldAxis).normalized;
                _positioner.ManipulatedObject.rotation = Quaternion.FromToRotation(initDir, currentDir) * _originalRotation;
                if (_positioner.UseLocalSpace)
                {
                    // Update local position & local rotation
                    _originalLocalPosition = _positioner.transform.InverseTransformPoint(destPoint);
                    _originalRotation = _positioner.ManipulatedObject.rotation;
                }
            }
        }
    }
}