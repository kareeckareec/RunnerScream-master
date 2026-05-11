using System.Collections.Generic;
using UnityEngine;

public class ChildMaterialAssigner : MonoBehaviour
{
    [Header("Список материалов для выбора")]
    public List<Material> materials = new List<Material>();

    
   

    void Start()
    {
        var renderer = this.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material randomMat = this.GetRandomMaterial();
            if (randomMat != null)
                renderer.material = randomMat;
        }
    }
    public Material GetRandomMaterial()
    {
        if (materials == null || materials.Count == 0)
        {
            Debug.LogWarning($"[{name}] Нет материалов для выбора!");
            return null;
        }

        return materials[Random.Range(0, materials.Count)];
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
