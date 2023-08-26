using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Agent
{
    public Vector2 position;
    public Vector2 direction;

    public static int Size
    {
        get { return sizeof(float) * 2 * 2; }
    }
}

public class SlimeSimulation : MonoBehaviour
{
    public ComputeShader computeShader;

    Agent[] agents;
    int numberOfAgents;
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

        numberOfAgents = 10;
        agents = new Agent[numberOfAgents];
        for (int i = 0; i < numberOfAgents; i++)
        {
            agents[i].position = Vector2.zero;
            // agents[i].position = Random.insideUnitCircle.normalized;
            agents[i].direction = Random.insideUnitCircle.normalized;
            // Debug.Log(agents[i].position);
        }
    }

    void Update()
    {
        // note: do I need to do this?
        renderTexture.DiscardContents();

        int kernelHandle = computeShader.FindKernel("CSMain");

        ComputeBuffer agentsBuffer = new ComputeBuffer(numberOfAgents, Agent.Size);
        agentsBuffer.SetData(agents);
        computeShader.SetBuffer(kernelHandle, "agents", agentsBuffer);

        computeShader.SetTexture(kernelHandle, "ResultTexture", renderTexture);

        computeShader.SetFloat("time", Time.time);
        computeShader.Dispatch(kernelHandle, renderTexture.width / 8, renderTexture.height / 8, 1);

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.mainTexture = renderTexture;

        // before releasing the data, update
        agentsBuffer.GetData(agents);

        agentsBuffer.Release();
    }
}
