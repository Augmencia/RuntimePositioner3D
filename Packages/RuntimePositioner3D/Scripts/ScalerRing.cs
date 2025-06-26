using UnityEngine;

namespace Augmencia.RuntimePositioner3D
{
    public class ScalerRing : AxisBase
    {
        [SerializeField] private Collider _collider;
        [SerializeField] private MeshRenderer _meshRenderer;

        private Positioner3D _positioner = null;
        private float _originalDistance;
        private Vector3 _originalScale;
        private Material _originalMaterial;
        private Vector3 _originalHitPoint;
        private Vector3 _originalVector;

        internal override void Initialize(Positioner3D positioner)
        {
            _positioner = positioner;
            _originalMaterial = _meshRenderer.material;
        }

        void LateUpdate()
        {
            // Billboarding
            if (_positioner?.Camera != null)
            {
                transform.LookAt(_positioner.Camera.transform);
                transform.Rotate(0, 180, 0);
            }
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
                Plane plane = new Plane(_positioner.Camera.transform.forward, _positioner.ManipulatedObject.position);
                Ray screenRay = new Ray(_positioner.Camera.transform.position, (hit.point - _positioner.Camera.transform.position).normalized);
                if (plane.Raycast(screenRay, out float distance))
                {
                    _originalHitPoint = screenRay.GetPoint(distance);
                    _originalDistance = Vector3.Distance(_originalHitPoint, _positioner.ManipulatedObject.position);
                    _originalScale = _positioner.ManipulatedObject.localScale;
                    _originalVector = _originalHitPoint - _positioner.ManipulatedObject.position;
                    return true;
                }
            }
            return false;
        }

        internal override void Drag(Vector3 screenPoint)
        {
            Plane plane = new Plane(_positioner.Camera.transform.forward, _positioner.ManipulatedObject.position);
            Ray ray = _positioner.Camera.ScreenPointToRay(screenPoint);
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                float dist = Vector3.Distance(hitPoint, _positioner.ManipulatedObject.position);
                if (_positioner.ScaleWhenGoingToTheOppositeDirection)
                {
                    Vector3 currentVector = hitPoint - _positioner.ManipulatedObject.position;
                    bool isTowardOriginalHit = Vector3.Dot(_originalVector, currentVector) >= 0;
                    if (dist < _originalDistance && isTowardOriginalHit)
                    {
                        _positioner.ManipulatedObject.localScale = _originalScale * _originalDistance / (_originalDistance + (_originalDistance - dist) / _positioner.ScaleDownReducingFactor);
                    }
                    else if (!isTowardOriginalHit)
                    {
                        _positioner.ManipulatedObject.localScale = _originalScale * _originalDistance / ((dist + _originalDistance) / _positioner.ScaleDownReducingFactor + _originalDistance);
                    }
                    else
                    {
                        _positioner.ManipulatedObject.localScale = _originalScale * dist / _originalDistance;
                    }
                }
                else
                {
                    _positioner.ManipulatedObject.localScale = _originalScale * dist / _originalDistance;
                }
            }
        }
    }
}
