using System.Drawing;
using CG.Core;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace CG
{
    internal class Game : GameWindow
    {
        private Mesh _mesh;
        private ShaderProgram _program;
        private Texture _ceiling_texture, _ground_texture, _wall_texture;
        private Transform _transform = new();
        private Camera _camera;

        private float _time;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            _mesh = Mesh.CreatePrimitiveCube(2f);
            // _transform.position.Y = -0.5f;

            // Com as novas classes de shader, podemos carregá-los diretamente de arquivos.
            VertexShader vertShader = VertexShader.LoadFromFile("./assets/shaders/shader.vert");
            FragmentShader fragShader = FragmentShader.LoadFromFile("./assets/shaders/shader.frag");

            // De forma similar, a classe ProgramShader junta os diferentes shaders.
            _program = new(vertShader, fragShader);

            // Carregamento de texturas, que serão armazenadas na placa de vídeo.
            _wall_texture = new Texture("./assets/textures/wall.png");
            _ground_texture = new Texture("./assets/textures/ground.png");
            _ceiling_texture = new Texture("./assets/textures/ceiling.png");

            //_camera = new OrthographicCamera(Size.X / 100f, Size.Y / 100f);
            _camera = new PerspectiveCamera(90f, (float)Size.X / Size.Y);
            // _camera.position.Z = 3f;// afasta a câmera para trás.

            GL.Enable(EnableCap.DepthTest);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            // _transform.rotation.Z = _time * 90f;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            _time += (float)args.Time;

            // Escolha da cor de limpeza.
            GL.ClearColor(0f, 0.3f, 0.5f, 1f);
            // Limpeza das cores e profundidade da tela.
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _program.Use();// Escolha do Shader Program.

            // Para usar texturas, dizemos em qual "caixinha" a queremos colocar. Para acessar essa
            //caixinha nos shaders, atribuímos o id da caixinha à variável uniforme correspondente.
            _wall_texture.Use(0);
            _ground_texture.Use(1);
            _ceiling_texture.Use(2);

            // Envio das matrizes da câmera
            _program.SetUniform("u_View", _camera.ViewMatrix);
            _program.SetUniform("u_Projection", _camera.ProjectionMatrix);

            // Antes do desenho, vamos enviar à placa de vídeo os novos valores das variáveis uni-
            //forme dos shaders usando as funções glUniform.
            _program.SetUniform("u_Color", 1.0f, 1.0f, 1.0f, 1.0f);
            _program.SetUniform("u_Texture", 0);// Usar textura na caixinha 0.
            _program.SetUniform("u_Model", _transform.ModelMatrix);// Transformação do objeto

            // Desenho, branco e à esquerda.
            _mesh.Draw();

            SwapBuffers();// Troca de buffers, apresentando o desenho completo ao usuário.
        }
    }
}
