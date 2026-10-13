// One quad for each visible channel's pixel column, covering the pixel rows its span crosses. Instances run in
// channel order, so a trace that runs into its neighbors' rows lies over them.

// Each channel's color, with alpha 0 where it is not drawn.
uniform sampler2D colorTexture;

uniform int width;
uniform int height;
uniform int firstRow;
uniform int firstVisible;

// Where values fall in a row: the top of the first row, in pixels down from the texture's top, the height of a
// row, the value at a row's middle and the pixels per unit.
uniform float origin;
uniform float rowHeight;
uniform float center;
uniform float scale;

// 1, or less for an expanded channel, which is drawn translucent under the outlines that show its shape.
uniform float alpha;

flat out vec4 color;

void main()
{
    int channel = firstVisible + gl_InstanceID / width;
    int p = gl_InstanceID % width;
    vec4 channelColor = texelFetch(colorTexture, ivec2(channel, 0), 0);

    float low;
    float high;
    bool drawn = channelColor.a > 0.0 && Bin(channel, p, low, high);

    // NB: stretched to reach the pixel before, so the span covers every step between neighbors as a line would,
    // rather than a band a pixel tall that a steep step thins to nothing.
    float spanLow = low;
    float spanHigh = high;
    float previousLow;
    float previousHigh;
    if (drawn && p > 0 && Bin(channel, p - 1, previousLow, previousHigh))
    {
        spanLow = min(low, previousHigh);
        spanHigh = max(high, previousLow);
    }

    float offset = origin + (float(channel - firstRow) + 0.5) * rowHeight + center * scale;
    float top = floor(offset - spanHigh * scale);
    float bottom = floor(offset - spanLow * scale) + 1.0;
    if (!drawn)
        top = bottom = -2.0;

    float x = float(p) + float(gl_VertexID & 1);
    float y = (gl_VertexID & 2) != 0 ? bottom : top;

    // NB: the texture's first row is the top of the plot, and a framebuffer's first row is its bottom, so a pixel
    // row y from the top lands at y from the bottom here, which is the texture's row y.
    gl_Position = vec4(x / float(width) * 2.0 - 1.0, y / float(height) * 2.0 - 1.0, 0.0, 1.0);
    color = vec4(channelColor.rgb, alpha);
}
