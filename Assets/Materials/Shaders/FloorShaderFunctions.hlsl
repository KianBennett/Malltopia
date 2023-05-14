void GetFloorColour_float(float2 pos, float cellSize, float cellPadding, float textureScale,
                            UnityTexture2DArray textureArray, float textureCount, UnitySamplerState samplerState, 
                            UnityTexture2D cellsTexture, float2 cellsTextureSize, UnitySamplerState cellsSamplerState,
                            out float4 colour)
{
    float cellSizeTotal = cellSize + 2 * cellPadding;
    
    float cellCoordX = ceil(pos.x / cellSizeTotal);
    float cellCoordY = ceil(pos.y / cellSizeTotal);
    // Accuracy isn't very good if it's sampling directly on the border between two colours, so offset it slightly so it def gets the right one
    float2 uv = float2(cellCoordX / cellsTextureSize.x - 0.005f, cellCoordY / cellsTextureSize.y - 0.005f);
    
    float4 cellTextureSample = SAMPLE_TEXTURE2D(cellsTexture, cellsSamplerState, uv);
    float textureArrayIndex = cellTextureSample.a * textureCount;
    
    float4 textureArraySample = SAMPLE_TEXTURE2D_ARRAY(textureArray, samplerState, pos * textureScale, textureArrayIndex);

    colour = textureArraySample;
}