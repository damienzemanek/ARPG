using UnityEngine;

[CreateAssetMenu(fileName = "EnableRenderLayer", menuName = "ARPG/SO/Method/EnableRenderLayer")]
public class MeshRendererSO_MethodVTable : SO_MethodVTable
{
    public void EnableRenderLayer(MeshRenderer renderer, int renderLayerIndex)
        => renderer.renderingLayerMask |= 1u << renderLayerIndex;
    
    public void DisableRenderLayer(MeshRenderer renderer, int renderLayerIndex) 
        => renderer.renderingLayerMask &= ~(1u << renderLayerIndex);
}