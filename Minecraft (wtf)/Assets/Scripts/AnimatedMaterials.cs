using UnityEngine;
using System.Collections.Generic;

public class AnimatedMaterials : MonoBehaviour
{
    [SerializeField] List<AnimatedMaterial> materials = new List<AnimatedMaterial>();
    [SerializeField] float timeBetweenChanges;

    float time;

    void Start()
    {
        foreach (AnimatedMaterial material in materials)
        {
            material.material.mainTexture = material.textures[0];
        }
    }

    void Update()
    {
        time += Time.deltaTime;
        if(time >= timeBetweenChanges)
        {
            time -= timeBetweenChanges;
            foreach (AnimatedMaterial material in materials)
            {
                int currentTextureIndex = material.textures.IndexOf(material.material.mainTexture);
                if (currentTextureIndex + 1 >= material.textures.Count) material.material.mainTexture = material.textures[0];
                else material.material.mainTexture = material.textures[currentTextureIndex + 1];
            }
        }
    }

    [System.Serializable]
    public struct AnimatedMaterial
    {
        public Material material;
        public List<Texture> textures;
    }
}
