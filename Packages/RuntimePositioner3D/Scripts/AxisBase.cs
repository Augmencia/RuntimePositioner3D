using UnityEngine;

namespace Augmencia.RuntimePositioner3D
{
    public abstract class AxisBase : MonoBehaviour
    {
        internal abstract void Initialize(Positioner3D positioner);
        internal abstract void SetMaterial(Material material);
        internal abstract void ResetMaterial();
        internal abstract bool TryStartDragging(RaycastHit hit);
        internal abstract void Drag(Vector3 screenPoint);
    }
}
