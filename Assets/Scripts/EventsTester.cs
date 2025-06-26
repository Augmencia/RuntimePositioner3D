using System.Collections.Generic;
using Augmencia.RuntimePositioner3D;
using UnityEngine;

[RequireComponent(typeof(Positioner3D))]
public class EventsTester : MonoBehaviour
{
    private Positioner3D _positioner3D;
    void Start()
    {
        _positioner3D = GetComponent<Positioner3D>();
        _positioner3D.OnAxisSelectionChanged += _OnAxisSelectionChanged;
    }

    void OnDestroy()
    {
        _positioner3D.OnAxisSelectionChanged -= _OnAxisSelectionChanged;
    }

    private void _OnAxisSelectionChanged(AxisBase axis)
    {
        string name = null;
        for (Transform t = axis?.transform; t != null && t != _positioner3D.transform; t = t.parent)
        {
            if (name == null)
            {
                name = t.gameObject.name;
            }
            else
            {
                name = t.gameObject.name + '.' + name;
            }
        }
        Debug.Log($"Axis: {name ?? "(null)"}");
    }
}
