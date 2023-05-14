void GetGridColour_float(float2 pos, float cellSize, float cellPadding, bool showDivisionLines, float cellDivisions, float divisionLineWidth, float lineAlpha, float divisionLineAlpha, UnityTexture2D cellFillTexture, UnitySamplerState samplerState, float2 textureSize, out float4 colour)
{
	float cellSizeTotal = cellSize + 2 * cellPadding;

	float2 posLocalToCell = float2(abs(pos.x % cellSizeTotal), abs(pos.y % cellSizeTotal));

	// Cell border
	if (posLocalToCell.x < cellPadding || posLocalToCell.x > cellSizeTotal - cellPadding ||
		posLocalToCell.y < cellPadding || posLocalToCell.y > cellSizeTotal - cellPadding)
	{
        colour = float4(1, 1, 1, lineAlpha);
		return;
	}

	if (showDivisionLines)
	{
		float divisionSize = cellSize / cellDivisions;
		float halfDivisionLineWidth = divisionLineWidth / 2;
		float2 posLocalToCellInner = float2((posLocalToCell.x - cellPadding) % divisionSize, (posLocalToCell.y - cellPadding) % divisionSize);

		// Cell subdivisions
		for (int i = 0; i < cellDivisions; i++)
		{
			if (posLocalToCellInner.x < halfDivisionLineWidth || posLocalToCellInner.x > divisionSize - halfDivisionLineWidth ||
				posLocalToCellInner.y < halfDivisionLineWidth || posLocalToCellInner.y > divisionSize - halfDivisionLineWidth)
			{
                colour = float4(1, 1, 1, divisionLineAlpha);
				return;
			}
		}
	}

    float cellCoordX = ceil(pos.x / cellSizeTotal);
    float cellCoordY = ceil(pos.y / cellSizeTotal);
	// Accuracy isn't very good if it's sampling directly on the border between two colours, so offset it slightly so it def gets the right one
    float2 uv = float2(cellCoordX / textureSize.x - 0.005f, cellCoordY / textureSize.y - 0.005f);
	
    float4 texSample = SAMPLE_TEXTURE2D(cellFillTexture, samplerState, uv);

    colour = texSample;
}