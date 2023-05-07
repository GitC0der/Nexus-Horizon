using UnityEngine;

namespace DefaultNamespace
{
    public class ShaderController : MonoBehaviour
    {
        public Color ambientLightColor;
        public Color fogColor;
        public Material skyboxMaterial;
        public GameObject sunSource;

        void Start()
        {
            // Change the ambient light color
            RenderSettings.ambientLight = ambientLightColor;

            // Change the fog color
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            
            RenderSettings.skybox = skyboxMaterial;
            
            // Disable the sun source
            sunSource.SetActive(false);
            
            //RenderSettings.fogDensity = 0.05f;
            RenderSettings.ambientIntensity = 0.2f;
            
            GameObject directionalLight = GameObject.Find("Directional Light");
            if (directionalLight != null)
            {
                directionalLight.SetActive(false);
            }

            skyboxMaterial.color = new Color(0, 0, 0, 1);

        }
    }
}