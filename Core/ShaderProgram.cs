
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CG.Core
{
    // Classe de Shader program, que junta dois ou mais shaders em um único programa, garantindo o
    //seu correto funcionamento em conjunto.
    internal class ShaderProgram
    {
        private int _id;
        public int Id => Id;

        public ShaderProgram(params Shader[] shaders)
        {
            _id = GL.CreateProgram();

            foreach (Shader shader in shaders)
            {
                GL.AttachShader(_id, shader.Id);
            }
            GL.LinkProgram(_id);

            string log = GL.GetProgramInfoLog(_id);
            if (log != null && log != "")
            {
                Console.WriteLine($"Erro de link no Shader Program: {log}");
            }
        }

        public void Use()
        {
            GL.UseProgram(_id);
        }

        public void Delete()
        {
            GL.DeleteProgram(_id);
        }

        public void SetUniform(string name, int value)
        {
            GL.ProgramUniform1(_id, GL.GetUniformLocation(_id, name), value);
        }

        public void SetUniform(string name, int v1, int v2)
        {
            GL.ProgramUniform2(_id, GL.GetUniformLocation(_id, name), v1, v2);
        }

        public void SetUniform(string name, Vector2i value)
        {
            SetUniform(name, value.X, value.Y);
        }

        public void SetUniform(string name, int v1, int v2, int v3)
        {
            GL.ProgramUniform3(_id, GL.GetUniformLocation(_id, name), v1, v2, v3);
        }

        public void SetUniform(string name, Vector3i value)
        {
            SetUniform(name, value.X, value.Y, value.Z);
        }

        public void SetUniform(string name, int v1, int v2, int v3, int v4)
        {
            GL.ProgramUniform4(_id, GL.GetUniformLocation(_id, name), v1, v2, v3, v4);
        }

        public void SetUniform(string name, Vector4i value)
        {
            SetUniform(name, value.X, value.Y, value.Z, value.W);
        }

        public void SetUniform(string name, float value)
        {
            GL.ProgramUniform1(_id, GL.GetUniformLocation(_id, name), value);
        }

        public void SetUniform(string name, float v1, float v2)
        {
            GL.ProgramUniform2(_id, GL.GetUniformLocation(_id, name), v1, v2);
        }

        public void SetUniform(string name, Vector2 value)
        {
            SetUniform(name, value.X, value.Y);
        }

        public void SetUniform(string name, float v1, float v2, float v3)
        {
            GL.ProgramUniform3(_id, GL.GetUniformLocation(_id, name), v1, v2, v3);
        }

        public void SetUniform(string name, Vector3 value)
        {
            SetUniform(name, value.X, value.Y, value.Z);
        }

        public void SetUniform(string name, float v1, float v2, float v3, float v4)
        {
            GL.ProgramUniform4(_id, GL.GetUniformLocation(_id, name), v1, v2, v3, v4);
        }

        public void SetUniform(string name, Vector4 value)
        {
            SetUniform(name, value.X, value.Y, value.Z, value.W);
        }

        public void SetUniform(string name, Matrix2 value)
        {
            GL.ProgramUniformMatrix2(_id, GL.GetUniformLocation(_id, name), true, ref value);
        }

        public void SetUniform(string name, Matrix3 value)
        {
            GL.ProgramUniformMatrix3(_id, GL.GetUniformLocation(_id, name), true, ref value);
        }

        public void SetUniform(string name, Matrix4 value)
        {
            GL.ProgramUniformMatrix4(_id, GL.GetUniformLocation(_id, name), true, ref value);
        }
    }
}