using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AgentV2
{
    public Vector2 position;
    public float angleInRadians;
    public int speciesIndex;
    public Vector4 speciesMask;

    public static int Size
    {
        get { return sizeof(float) * 2 + sizeof(float) + sizeof(int) + sizeof(float) * 4; }
    }
}

[System.Serializable]
public struct SpeciesSettingsV2
{
    [RangeWithStep(0, 50, 2f)]
    public float sensorOffset;

    [RangeWithStep(0, 180, 5f)]
    public float sensorAngle;

    [RangeWithStep(0, 180, 5f)]
    public float rotationAngle;

    public Color color;

    public static int Size
    {
        get { return sizeof(float) * 3 + sizeof(float) * 4; }
    }
}

public class SlimeSimulationV2 : MonoBehaviour
{
    public ComputeShader computeShader;

    [Header("Texture")]
    [Range(128, 2048)]
    public int width;

    [Range(128, 2048)]
    public int height;

    [Header("General")]
    [RangeWithStep(32, 1048576, 32f)]
    public float numOfAgents;

    [RangeWithStep(0, 250, 10f)]
    public float distFromMapEdge;

    [Header("Speed")]
    [RangeWithStep(8, 256, 8f)]
    public float speed;

    [Header("Trail")]
    [RangeWithStep(0.0f, 1.5f, 0.1f)]
    public float diffuseRate;

    [RangeWithStep(0.1f, 1.0f, 0.1f)]
    public float trailDecayRate;

    [RangeWithStep(0.1f, 1.0f, 0.1f)]
    public float diffuseDecayRate;

    public List<SpeciesSettingsV2> speciesSettingsList;

    [Header("Color")]
    public Gradient gradient;

    AgentV2[] agents;
    ComputeBuffer agentsBuffer;

    SpeciesSettingsV2[] speciesSettings;
    ComputeBuffer speciesSettingsBuffer;

    RenderTexture positionTexture;
    RenderTexture trailMapTexture;
    RenderTexture diffuseMapTexture;
    RenderTexture colorMapTexture;
    Texture2D gradientTexture;

    void Start()
    {
        // if (speciesSettingsList == null)
        // {
        //     speciesSettingsList = new List<SpeciesSettings>();
        // }

        // set depth to 0 for 2D textures
        int depth = 0;
        RenderTextureFormat format = RenderTextureFormat.ARGB32;
        RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default;

        // create the position texture and enable UAV access
        positionTexture = new RenderTexture(width, height, depth, format, readWrite);
        positionTexture.enableRandomWrite = true;
        positionTexture.Create();

        trailMapTexture = new RenderTexture(width, height, depth, format, readWrite);
        trailMapTexture.enableRandomWrite = true;
        trailMapTexture.Create();

        diffuseMapTexture = new RenderTexture(width, height, depth, format, readWrite);
        diffuseMapTexture.enableRandomWrite = true;
        diffuseMapTexture.Create();

        colorMapTexture = new RenderTexture(width, height, depth, format, readWrite);
        colorMapTexture.enableRandomWrite = true;
        colorMapTexture.Create();

        InitializeAgents();
        InitializeSpeciesSettings();
        InitializeGradientTexture();
    }

    public void InitializeSpeciesSettings()
    {
        int numOfSpecies = speciesSettingsList.Count;
        speciesSettings = new SpeciesSettingsV2[numOfSpecies];
        for (int i = 0; i < numOfSpecies; i++)
        {
            speciesSettings[i].sensorOffset = speciesSettingsList[i].sensorOffset;
            speciesSettings[i].sensorAngle = speciesSettingsList[i].sensorAngle;
            speciesSettings[i].rotationAngle = speciesSettingsList[i].rotationAngle;
            Color c = speciesSettingsList[i].color;
            speciesSettings[i].color = new Vector4(c.r, c.g, c.b, c.a);
        }
        speciesSettingsBuffer = new ComputeBuffer(numOfSpecies, SpeciesSettingsV2.Size);
    }

    public void InitializeAgents()
    {
        // set up a few agents to simulate
        int numOfAgentsInt = Mathf.RoundToInt(numOfAgents);
        agents = new AgentV2[numOfAgentsInt];
        for (int i = 0; i < numOfAgentsInt; i++)
        {
            // part 1 - all agents at center facing outwards
            agents[i].position = Vector2.zero;
            Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
            agents[i].angleInRadians = Mathf.Atan2(direction.y, direction.x);

            float randomValue = UnityEngine.Random.Range(0f, 2f);
            int randomZeroOrOne = Mathf.FloorToInt(randomValue);
            agents[i].speciesIndex = randomZeroOrOne;
            agents[i].speciesMask =
                randomZeroOrOne == 0 ? new Vector4(1, 0, 0, 0) : new Vector4(0, 1, 0, 0);

            // part 2 - circle facing inwards
            // float initialRadius = Mathf.Min(width, height) / 2 - distFromMapEdge;
            // agents[i].position = initialRadius * UnityEngine.Random.insideUnitCircle;
            // Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
            // agents[i].angleInRadians = Mathf.Atan2(direction.y, direction.x);
            // agents[i].speciesIndex = 0;
        }

        // set up agentsBuffer to be the correct size
        agentsBuffer = new ComputeBuffer(numOfAgentsInt, AgentV2.Size);
    }

    void InitializeGradientTexture()
    {
        int textureWidth = 256; // Set the desired width of the texture
        int textureHeight = 1; // Since it's a 1D gradient, set the height to 1

        gradientTexture = new Texture2D(
            textureWidth,
            textureHeight,
            TextureFormat.RGBA32,
            0,
            false
        );

        for (int x = 0; x < textureWidth; x++)
        {
            float t = (float)x / (float)(textureWidth - 1); // Normalize x to [0, 1]
            Color color = gradient.Evaluate(t); // Evaluate the gradient color at position t
            gradientTexture.SetPixel(x, 0, color); // Set the color at the pixel position
        }

        gradientTexture.Apply();
    }

    public void RandomizeGradient()
    {
        if (gradient == null)
        {
            return;
        }

        // Randomize color keys
        GradientColorKey[] colorKeys = gradient.colorKeys;
        for (int i = 0; i < colorKeys.Length; i++)
        {
            colorKeys[i].color = UnityEngine.Random.ColorHSV();
        }

        // Randomize alpha keys
        GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
        for (int i = 0; i < alphaKeys.Length; i++)
        {
            // alphaKeys[i].alpha = UnityEngine.Random.Range(0.0f, 1.0f);
            alphaKeys[i].alpha = 1;
        }

        // Assign the modified keys back to the gradient
        gradient.SetKeys(colorKeys, alphaKeys);

        InitializeGradientTexture();
    }

    public void RandomizeSpeciesSettings()
    {
        for (int i = 0; i < speciesSettingsList.Count; i++)
        {
            SpeciesSettingsV2 s = speciesSettingsList[i];

            // speciesSettingsList[i].color = new Color(1, 1, 1, 1);
            s.sensorOffset = UnityEngine.Random.Range(1, 50);
            s.sensorAngle = UnityEngine.Random.Range(1, 180);
            s.rotationAngle = UnityEngine.Random.Range(1, 180);

            speciesSettingsList[i] = s;

            // speciesSettings[i].sensorAngle = speciesSettingsList[i].sensorAngle;
            // speciesSettings[i].rotationAngle = speciesSettingsList[i].rotationAngle;
            // Color c = speciesSettingsList[i].color;
            // speciesSettings[i].color = new Vector4(c.r, c.g, c.b, c.a);
        }

        InitializeSpeciesSettings();
    }

    void PrintAgentsPositions()
    {
        string[] arr = new string[agents.Length];
        for (int i = 0; i < agents.Length; i++)
        {
            arr[i] = agents[i].position.ToString();
        }
        Debug.Log(string.Join(", ", arr));
    }

    void Update()
    {
        if (agents == null)
        {
            return;
        }

        // DiscardContents() -> tells unity you no longer need this data.
        // it does not guarantee that memory is immediately released.
        // positionTexture.DiscardContents();

        // Release() -> tells unity immediately discard this memory
        // useful to reset previous positions value to float4(0, 0, 0, 0)
        // this texture should only store the current position of the agent
        positionTexture.Release();

        // set the "agents buffer" array with the latest position + direction data from "agents"
        agentsBuffer.SetData(agents);
        speciesSettingsBuffer.SetData(speciesSettings);

        int kernelHandle1 = computeShader.FindKernel("CSPositionMap");
        computeShader.SetInt("width", width);
        computeShader.SetInt("height", height);
        computeShader.SetFloat("speed", speed);
        computeShader.SetFloat("distFromMapEdge", distFromMapEdge);

        computeShader.SetFloat("time", Time.time);
        computeShader.SetFloat("deltaTime", Time.deltaTime);

        computeShader.SetFloat("numOfAgents", numOfAgents);
        // todo: update this
        // computeShader.SetFloat("numOfSpecies", 2);

        computeShader.SetBuffer(kernelHandle1, "AgentsBuffer", agentsBuffer);
        computeShader.SetBuffer(kernelHandle1, "SpeciesSettingsBuffer", speciesSettingsBuffer);
        computeShader.SetTexture(kernelHandle1, "PositionTexture", positionTexture);
        computeShader.SetTexture(kernelHandle1, "TrailMapTexture", trailMapTexture);
        computeShader.Dispatch(kernelHandle1, Mathf.RoundToInt(numOfAgents) / 32, 1, 1);

        // todo: figure out why we need to set the positionTexture again even though
        // we don't need to create the variable for #pragma kernel CSTrailMap
        int kernelHandle2 = computeShader.FindKernel("CSTrailMap");
        computeShader.SetFloat("trailDecayRate", trailDecayRate);
        computeShader.SetBuffer(kernelHandle2, "SpeciesSettingsBuffer", speciesSettingsBuffer);
        computeShader.SetTexture(kernelHandle2, "PositionTexture", positionTexture);
        computeShader.SetTexture(kernelHandle2, "TrailMapTexture", trailMapTexture);
        computeShader.Dispatch(
            kernelHandle2,
            trailMapTexture.width / 8,
            trailMapTexture.height / 8,
            1
        );

        int kernelHandle3 = computeShader.FindKernel("CSDiffuseMap");
        computeShader.SetInt("width", width);
        computeShader.SetInt("height", height);
        computeShader.SetFloat("diffuseRate", diffuseRate);
        computeShader.SetFloat("diffuseDecayRate", diffuseDecayRate);
        computeShader.SetBuffer(kernelHandle3, "SpeciesSettingsBuffer", speciesSettingsBuffer);
        computeShader.SetTexture(kernelHandle3, "PositionTexture", positionTexture);
        computeShader.SetTexture(kernelHandle3, "TrailMapTexture", trailMapTexture);
        computeShader.SetTexture(kernelHandle3, "DiffuseMapTexture", diffuseMapTexture);
        computeShader.SetTexture(kernelHandle3, "ColorMapTexture", colorMapTexture);
        computeShader.SetTexture(kernelHandle3, "GradientTexture", gradientTexture);
        computeShader.Dispatch(
            kernelHandle3,
            diffuseMapTexture.width / 8,
            diffuseMapTexture.height / 8,
            1
        );

        // copy diffuseTrailMapTexture into trailMapTexture so that the trailMap
        // can decrement the values in the blurred sections of the trail
        Graphics.Blit(diffuseMapTexture, trailMapTexture);

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        // meshRenderer.material.mainTexture = positionTexture;
        // meshRenderer.material.mainTexture = trailMapTexture;
        // meshRenderer.material.mainTexture = diffuseMapTexture;
        meshRenderer.material.mainTexture = colorMapTexture;

        // note: render scene to main camera
        // Camera mainCamera = Camera.main;
        // mainCamera.targetTexture = colorMapTexture;

        // update the "agents" array with the positions + directions from the compute shader
        agentsBuffer.GetData(agents);
    }

    void OnDestroy()
    {
        if (agentsBuffer == null)
        {
            return;
        }
        if (speciesSettingsBuffer == null)
        {
            return;
        }
        agentsBuffer.Release();
        speciesSettingsBuffer.Release();
        Destroy(gradientTexture);
    }
}
