using CG.Core;
using OpenTK.Mathematics;

namespace CG
{
    internal class GameObject
    {
        public Mesh mesh;
        public Transform transform = new();
        public int textureIndex;
        public Vector4 color = Vector4.One;

        public GameObject(Mesh _mesh, int _texture_index, Transform? _transform = null, Vector4? _color = null)
        {
            mesh = _mesh;
            textureIndex = _texture_index;
            transform = _transform != null ? _transform : new();
            color = _color != null ? (Vector4)_color : Vector4.One;
        }

        public void Render(ShaderProgram shader)
        {
            shader.SetUniform("u_Color",color.X,color.Y,color.Z,color.W);
            shader.SetUniform("u_Texture",textureIndex);
            shader.SetUniform("u_Model",transform.ModelMatrix);
            mesh.Draw();
        }
    }
}