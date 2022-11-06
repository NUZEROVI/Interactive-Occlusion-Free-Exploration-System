using UnityEngine;

namespace UnityVolumeRendering
{
    [ExecuteInEditMode]
    public class VolumeRenderedObject_Mask : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        public TransferFunction transferFunction;

        [SerializeField, HideInInspector]
        public TransferFunction2D transferFunction2D;

        [SerializeField, HideInInspector]
        public VolumeDataset dataset;

        [SerializeField, HideInInspector]
        public MeshRenderer meshRenderer;

        [SerializeField, HideInInspector]
        private RenderMode renderMode;
        [SerializeField, HideInInspector]
        private TFRenderMode tfRenderMode;
        [SerializeField, HideInInspector]
        private bool lightingEnabled;

        [SerializeField, HideInInspector]
        private Vector2 visibilityWindow = new Vector2(0.0f, 1.0f);
        [SerializeField, HideInInspector]
        private bool rayTerminationEnabled = true;
        [SerializeField, HideInInspector]
        private bool dvrBackward = false;
 
        public void SetRenderMode(RenderMode mode)
        {
            if (renderMode != mode)
            {
                renderMode = mode;
                SetVisibilityWindow(0.0f, 1.0f); // reset visibility window
            }
            UpdateMaterialProperties();
        }

        public void SetTransferFunctionMode(TFRenderMode mode)
        {
            tfRenderMode = mode;
            if (tfRenderMode == TFRenderMode.TF1D && transferFunction != null)
                transferFunction.GenerateTexture();
            else if (transferFunction2D != null)
                transferFunction2D.GenerateTexture();
            UpdateMaterialProperties();
        }

        public TFRenderMode GetTransferFunctionMode()
        {
            return tfRenderMode;
        }

        public RenderMode GetRenderMode()
        {
            return renderMode;
        }

        public bool GetLightingEnabled()
        {
            return lightingEnabled;
        }

        public void SetLightingEnabled(bool enable)
        {
            if (enable != lightingEnabled)
            {
                lightingEnabled = enable;
                UpdateMaterialProperties();
            }
        }

        public void SetVisibilityWindow(float min, float max)
        {
            SetVisibilityWindow(new Vector2(min, max));
        }

        public void SetVisibilityWindow(Vector2 window)
        {
            if (window != visibilityWindow)
            {
                visibilityWindow = window;
                UpdateMaterialProperties();
            }
        }

        public Vector2 GetVisibilityWindow()
        {
            return visibilityWindow;
        }

        public bool GetRayTerminationEnabled()
        {
            return rayTerminationEnabled;
        }

        public void SetRayTerminationEnabled(bool enable)
        {
            if (enable != rayTerminationEnabled)
            {
                rayTerminationEnabled = enable;
                UpdateMaterialProperties();
            }
        }

        public bool GetDVRBackwardEnabled()
        {
            return dvrBackward;
        }

        public void SetDVRBackwardEnabled(bool enable)
        {
            if (enable != dvrBackward)
            {
                dvrBackward = enable;
                UpdateMaterialProperties();
            }
        }

        public void UpdateMaterialProperties()
        {
            bool useGradientTexture = tfRenderMode == TFRenderMode.TF2D || renderMode == RenderMode.IsosurfaceRendering || lightingEnabled;
            meshRenderer.sharedMaterial.SetTexture("_GradientTex", useGradientTexture ? dataset.GetGradientTexture() : null);

            if (tfRenderMode == TFRenderMode.TF2D)
            {
                meshRenderer.sharedMaterial.SetTexture("_TFTex", transferFunction2D.GetTexture());
                meshRenderer.sharedMaterial.EnableKeyword("TF2D_ON");
            }
            else
            {
                meshRenderer.sharedMaterial.SetTexture("_TFTex", transferFunction.GetTexture());
                meshRenderer.sharedMaterial.DisableKeyword("TF2D_ON");
            }

            if (lightingEnabled)
                meshRenderer.sharedMaterial.EnableKeyword("LIGHTING_ON");
            else
                meshRenderer.sharedMaterial.DisableKeyword("LIGHTING_ON");

            switch (renderMode)
            {
                case RenderMode.DirectVolumeRendering:
                    {
                        meshRenderer.sharedMaterial.EnableKeyword("MODE_DVR");
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_MIP");
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_SURF");
                        break;
                    }
                case RenderMode.MaximumIntensityProjectipon:
                    {
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_DVR");
                        meshRenderer.sharedMaterial.EnableKeyword("MODE_MIP");
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_SURF");
                        break;
                    }
                case RenderMode.IsosurfaceRendering:
                    {
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_DVR");
                        meshRenderer.sharedMaterial.DisableKeyword("MODE_MIP");
                        meshRenderer.sharedMaterial.EnableKeyword("MODE_SURF");
                        break;
                    }
            }

            meshRenderer.sharedMaterial.SetFloat("_MinVal", visibilityWindow.x);
            meshRenderer.sharedMaterial.SetFloat("_MaxVal", visibilityWindow.y);

            if (rayTerminationEnabled)
            {
                meshRenderer.sharedMaterial.EnableKeyword("RAY_TERMINATE_ON");
            }
            else
            {
                meshRenderer.sharedMaterial.DisableKeyword("RAY_TERMINATE_ON");
            }

            if (dvrBackward)
            {
                meshRenderer.sharedMaterial.EnableKeyword("DVR_BACKWARD_ON");
            }
            else
            {
                meshRenderer.sharedMaterial.DisableKeyword("DVR_BACKWARD_ON");
            }
        }

        private void Start()
        {
            UpdateMaterialProperties();
        }
    }
}
