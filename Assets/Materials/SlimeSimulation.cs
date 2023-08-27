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

    // define render texture dimensions
    [Range(128, 512)]
    public int width;

    [Range(128, 512)]
    public int height;

    [Range(8, 64)]
    public int numOfAgents;

    Agent[] agents;
    ComputeBuffer agentsBuffer;
    RenderTexture renderTexture;

    int kernelHandle;

    void Start()
    {
        // set depth to 0 for 2D textures
        int depth = 0;
        RenderTextureFormat format = RenderTextureFormat.ARGB32;
        RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default;

        // create the render texture
        renderTexture = new RenderTexture(width, height, depth, format, readWrite);
        // enable UAV access
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        // set up a few agents to simulate
        agents = new Agent[numOfAgents];
        for (int i = 0; i < numOfAgents; i++)
        {
            agents[i].position = Vector2.zero;
            agents[i].direction = Random.insideUnitCircle.normalized;
        }

        // set up agentsBuffer to be the correct size
        agentsBuffer = new ComputeBuffer(numOfAgents, Agent.Size);

        kernelHandle = computeShader.FindKernel("CSMainNew");
    }

    void Update()
    {
        // note: do I need to do this?
        // renderTexture.DiscardContents();

        // set the "agents buffer" array with the latest position + direction data from "agents"
        agentsBuffer.SetData(agents);

        computeShader.SetInt("numOfAgents", numOfAgents);
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.SetBuffer(kernelHandle, "agents", agentsBuffer);
        computeShader.SetTexture(kernelHandle, "ResultTexture", renderTexture);

        computeShader.Dispatch(kernelHandle, renderTexture.width / 8, renderTexture.height / 8, 1);

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.mainTexture = renderTexture;

        // update the "agents" array with the positions + directions from the compute shader
        agentsBuffer.GetData(agents);
    }

    void OnDestroy()
    {
        agentsBuffer.Release();
    }
}
