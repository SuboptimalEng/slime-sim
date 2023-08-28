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
    RenderTexture resultTexture;
    RenderTexture trailMapTexture;
    RenderTexture diffusedTrailMapTexture;

    void Start()
    {
        // set depth to 0 for 2D textures
        int depth = 0;
        RenderTextureFormat format = RenderTextureFormat.ARGB32;
        RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default;

        // create the result texture and enable UAV access
        resultTexture = new RenderTexture(width, height, depth, format, readWrite);
        resultTexture.enableRandomWrite = true;
        resultTexture.Create();

        trailMapTexture = new RenderTexture(width, height, depth, format, readWrite);
        trailMapTexture.enableRandomWrite = true;
        trailMapTexture.Create();

        diffusedTrailMapTexture = new RenderTexture(width, height, depth, format, readWrite);
        diffusedTrailMapTexture.enableRandomWrite = true;
        diffusedTrailMapTexture.Create();

        // set up a few agents to simulate
        agents = new Agent[numOfAgents];
        for (int i = 0; i < numOfAgents; i++)
        {
            agents[i].position = Vector2.zero;
            agents[i].direction = Random.insideUnitCircle.normalized;
        }

        // set up agentsBuffer to be the correct size
        agentsBuffer = new ComputeBuffer(numOfAgents, Agent.Size);
    }

    void Update()
    {
        // note: do I need to do this?
        // resultTexture.DiscardContents();

        // set the "agents buffer" array with the latest position + direction data from "agents"
        agentsBuffer.SetData(agents);

        // todo: maybe used time.fixedDeltaTime? what's the difference anyway?
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.SetInt("numOfAgents", numOfAgents);

        int kernelHandle1 = computeShader.FindKernel("CSMainNew");
        computeShader.SetBuffer(kernelHandle1, "agents", agentsBuffer);
        computeShader.SetTexture(kernelHandle1, "ResultTexture", resultTexture);
        computeShader.Dispatch(kernelHandle1, resultTexture.width / 8, resultTexture.height / 8, 1);

        int kernelHandle2 = computeShader.FindKernel("CSTrailMap");
        // todo: figure out why we need to set the resultTexture again even though
        // we don't need to create the variable for #pragma kernel CSTrailMap
        computeShader.SetTexture(kernelHandle2, "ResultTexture", resultTexture);
        computeShader.SetTexture(kernelHandle2, "TrailMapTexture", trailMapTexture);
        computeShader.Dispatch(
            kernelHandle2,
            trailMapTexture.width / 8,
            trailMapTexture.height / 8,
            1
        );

        int kernelHandle3 = computeShader.FindKernel("CSDiffuse");
        computeShader.SetInt("width", width);
        computeShader.SetInt("height", height);
        computeShader.SetTexture(kernelHandle3, "ResultTexture", resultTexture);
        computeShader.SetTexture(kernelHandle3, "TrailMapTexture", trailMapTexture);
        computeShader.SetTexture(kernelHandle3, "DiffusedTrailMapTexture", diffusedTrailMapTexture);
        computeShader.Dispatch(
            kernelHandle3,
            diffusedTrailMapTexture.width / 8,
            diffusedTrailMapTexture.height / 8,
            1
        );

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        // meshRenderer.material.mainTexture = resultTexture;
        // meshRenderer.material.mainTexture = trailMapTexture;
        meshRenderer.material.mainTexture = diffusedTrailMapTexture;

        // copy diffusedTrailMapTexture into trailMapTexture so that the trailMap
        // can decrement the values in the blurred sections of the trail
        Graphics.Blit(diffusedTrailMapTexture, trailMapTexture);

        // update the "agents" array with the positions + directions from the compute shader
        agentsBuffer.GetData(agents);
    }

    void OnDestroy()
    {
        agentsBuffer.Release();
    }
}
