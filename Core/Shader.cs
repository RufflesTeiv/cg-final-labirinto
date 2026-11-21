using OpenTK.Graphics.OpenGL;

namespace CG.Core
{
    // Classe abstrata de shader, que cuida da compilação de shaders individuais. Suas especializa-
    //ções VertexShader e FragmentShader podem ser utilizadas para carregar shaders a partir de u-
    //ma string ou a partir de arquivos.
    internal abstract class Shader
    {
        private int _id = 0;
        public int Id => _id;

        protected abstract ShaderType Type { get; }


        protected Shader(string source)
        {
            _id = GL.CreateShader(Type);

            GL.ShaderSource(_id, source);
            GL.CompileShader(_id);
            string log = GL.GetShaderInfoLog(_id);
            if(log != null && log != "")
            {
                Console.WriteLine($"Erro de compilação de shader({Type.ToString()}): {log}");
            }
        }

        public void Delete()
        {
            if (_id != 0)
            {
                GL.DeleteShader(_id);
            }
        }
    }

    // Especialização para shaders de vértice.
    internal class VertexShader : Shader
    {
        protected override ShaderType Type => ShaderType.VertexShader;

        public VertexShader(string source) : base(source) {}

        public static VertexShader LoadFromFile(string filePath)
        {
            return new VertexShader(File.ReadAllText(filePath));
        }
    }

    // Especialização para shaders de fragmento.
    internal class FragmentShader : Shader
    {
        protected override ShaderType Type => ShaderType.FragmentShader;

        public FragmentShader(string source) : base(source) {}

        public static FragmentShader LoadFromFile(string filePath)
        {
            return new FragmentShader(File.ReadAllText(filePath));
        }
    }
}
