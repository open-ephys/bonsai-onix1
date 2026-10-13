// A quad over the whole texture.
void main()
{
    gl_Position = vec4(float(gl_VertexID & 1) * 2.0 - 1.0, float((gl_VertexID >> 1) & 1) * 2.0 - 1.0, 0.0, 1.0);
}
