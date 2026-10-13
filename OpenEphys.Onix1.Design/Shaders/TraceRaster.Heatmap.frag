// Fills each visible channel's row with the color of its signal: at each pixel column, whichever of the min and
// max lies further from zero, with its sign, through a diverging colormap black at zero. Magnitudes up to the
// threshold are black.

// Each channel's color, with alpha 0 where its row is not filled.
uniform sampler2D colorTexture;

// The colormap, its two halves for the two signs meeting in the middle.
uniform sampler2D colormapTexture;
uniform int colormapLength;

uniform int firstRow;
uniform int firstVisible;
uniform int lastVisible;
uniform float origin;
uniform float rowHeight;
uniform float range;
uniform float threshold;

out vec4 fragmentColor;

void main()
{
    int p = int(gl_FragCoord.x);
    float y = floor(gl_FragCoord.y);

    // NB: rows end where the plot's rows do, floor(origin + k * rowHeight), so the row worked out by division is
    // checked against those edges.
    int channel = firstRow + int(floor((y - origin) / rowHeight));
    if (y < floor(origin + float(channel - firstRow) * rowHeight))
        channel--;
    else if (y >= floor(origin + float(channel - firstRow + 1) * rowHeight))
        channel++;

    if (channel < firstVisible || channel >= lastVisible || texelFetch(colorTexture, ivec2(channel, 0), 0).a == 0.0)
        discard;

    float low;
    float high;
    if (!Bin(channel, p, low, high))
        discard;

    float value = abs(high) >= abs(low) ? high : low;
    float magnitude = abs(value);
    if (magnitude <= threshold)
    {
        fragmentColor = vec4(0.0, 0.0, 0.0, 1.0);
        return;
    }

    int middle = colormapLength / 2;
    int level = int(min(1.0, magnitude / range) * float(middle - 1) + 0.5);
    int index = value >= 0.0 ? middle + level : middle - 1 - level;
    fragmentColor = texelFetch(colormapTexture, ivec2(index, 0), 0);
}
