using UnityEngine;

[SelectionBase]
public class CellController : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_MeshRenderer;

    private GameObject m_Unit;

    public void SetMaterial(Material material)
    {
        m_MeshRenderer.material = material;
    }

    public void PlaceUnit(GameObject unit)
    {
        if (m_Unit == null && unit != null)
        {
            m_Unit = Instantiate(unit, transform.position, Quaternion.identity, transform);
        }
    }

    public void RemoveUnit()
    {
        if (m_Unit != null)
        {
            Destroy(m_Unit);
            m_Unit = null;
        }
    }
}