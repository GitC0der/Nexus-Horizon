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

        }

        public void EnableShader() {
            // Change the ambient light color
            RenderSettings.ambientLight = ambientLightColor;

            // Change the fog color
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            
            RenderSettings.skybox = skyboxMaterial;
            
            // Disable the sun source
            //sunSource.SetActive(false);
            sunSource.GetComponent<Light>().color = new Color(0.9f, 0.85f, 0.1f);
            
            //RenderSettings.ambientIntensity = 0.2f;
            RenderSettings.ambientIntensity = 0.55f;

            //skyboxMaterial.color = new Color(0, 0, 0, 1);
            skyboxMaterial.color = new Color(0.35f, 0.35f, 0.35f, 1);
        }
    }
}