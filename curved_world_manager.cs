/*
 * curved_world_manager.cs
 *
 * This script is used to manage the curved world effect globally for all shaders using the curved world parameters in the scene.
 * It also changes the camera's culling matrix to be orthographic to ensure that the camera can see the entire scene.
 * This fixes mesh culling issues when using the curved world effect.
 */

using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class curved_world_manager : MonoBehaviour
{
  public bool enableCurvedWorld;
  private static readonly string UseCurvedEffect_ShaderProperty = "_Use_Curved_Effect";
  
  [Range(-0.1f, 0.1f)]
  public float CurveAmountX = 0f;
  [Range(-0.1f, 0.1f)]
  public float CurveAmountY = 0f;
  [Range(-0.1f, 0.1f)]
  public float TwistAmount = 0f;
  
  private Vector4 PackedInputsXYZ;
  private static readonly int PackedInputsXYZ_ShaderProperty = Shader.PropertyToID("_Packed_Inputs_XYZ");

  private void Update()
  {
    // Update Shader toggles.
    // These are just simple floats, which the shader sees as bools.
    // This is done to not incur new shader variants.
    if (enableCurvedWorld)
    {
      Shader.SetGlobalFloat(UseCurvedEffect_ShaderProperty, enableCurvedWorld ? 1f : 0f); // Set True
    }
    else
    {
      Shader.SetGlobalFloat(UseCurvedEffect_ShaderProperty, enableCurvedWorld ? 1f : 0f); // Set False
    }
    
    // Pack inputs into vector4 to be read by shader.
    PackedInputsXYZ = new Vector4(CurveAmountX, CurveAmountY, TwistAmount, 0);
    // Set shader globals for all shaders.
    Shader.SetGlobalVector(PackedInputsXYZ_ShaderProperty, PackedInputsXYZ);
  }
  
/*
 * As the curved world shader is done in the vertex shader, this is done before the camera frustum culling.
 * This causes the meshes to cull out too early.
 * To fix this, we need to update the camera's culling matrix to ensure that the camera can see the entire scene.
 * This is done by changing the cameras frustum from perspective to orthographic but still using the perspective projection.
 */
  private void OnEnable()
  {
    // Return if not in play mode.
    if (!Application.isPlaying)
    {
      return;
    }
    
    // Register to the render pipeline manager.
    RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
  }
  private void OnDisable()
  {
    // Unregister from the render pipeline manager.
    RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
  }

  private static void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam)
  {
    // Set the camera's culling matrix to be orthographic.
    cam.cullingMatrix = Matrix4x4.Ortho(-99, 99, -99, 99, 0.001f, 99) * cam.worldToCameraMatrix;
  }

  private static void OnEndCameraRendering(ScriptableRenderContext ctx, Camera cam)
  {
    // Reset the camera's culling matrix to be perspective again when stopped rendering.
    cam.ResetCullingMatrix();
  }
}