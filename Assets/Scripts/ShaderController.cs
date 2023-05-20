﻿using UnityEngine;

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
            sunSource.SetActive(false);
            
            //RenderSettings.ambientIntensity = 0.2f;
            RenderSettings.ambientIntensity = 0.55f;
            
            GameObject directionalLight = GameObject.Find("Directional Light");
            if (directionalLight != null)
            {
                //directionalLight.SetActive(false);
            }

            //skyboxMaterial.color = new Color(0, 0, 0, 1);
            skyboxMaterial.color = new Color(0.35f, 0.35f, 0.35f, 1);
        }
    }
}