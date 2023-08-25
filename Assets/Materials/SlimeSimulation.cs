using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeSimulation : MonoBehaviour
{
    public ComputeShader computeShader;

    RenderTexture renderTexture;

    void Start()
    {
        // Define Render Texture dimensions and other settings
        int width = 512;
        int height = 512;
        RenderTextureFormat format = RenderTextureFormat.ARGB32;
        int depth = 0; // Set to 0 for 2D textures
        RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default;
        // Create the Render Texture
        renderTexture = new RenderTexture(width, height, depth, format, readWrite);
        renderTexture.enableRandomWrite = true; // Enable UAV access
        renderTexture.Create();
    }

    void Update()
    {
        renderTexture.DiscardContents();

        int kernelHandle = computeShader.FindKernel("CSMain");
        computeShader.SetTexture(kernelHandle, "ResultTexture", renderTexture);
        computeShader.SetFloat("time", Time.time);
        computeShader.Dispatch(kernelHandle, renderTexture.width / 8, renderTexture.height / 8, 1);

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.mainTexture = renderTexture;
    }
}
