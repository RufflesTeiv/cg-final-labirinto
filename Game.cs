using System.Drawing;
using CG.Core;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace CG
{
    internal class Game : GameWindow
    {
        private Vector4 CLEAR_COLOR = new Vector4(0f, 0.3f, 0.5f, 1f);

        private ShaderProgram _program;
        private Texture _ceiling_texture, _ground_texture, _wall_texture;
        private Camera _camera;
        private List<GameObject> gameObjects = new();
        private GameObject cubeGameObject;

        private float _time;
        private float _delta_time;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            Mesh _cube_mesh = Mesh.CreatePrimitiveCube(2f);
            // _transform.position.Y = -0.5f;

            // Com as novas classes de shader, podemos carregá-los diretamente de arquivos.
            VertexShader vertShader = VertexShader.LoadFromFile("./assets/shaders/shader.vert");
            FragmentShader fragShader = FragmentShader.LoadFromFile("./assets/shaders/shader.frag");

            // De forma similar, a classe ProgramShader junta os diferentes shaders.
            _program = new(vertShader, fragShader);

            // Carregamento de texturas, que serão armazenadas na placa de vídeo.
            _wall_texture = new Texture("./assets/textures/wall.png"); // 0
            _ground_texture = new Texture("./assets/textures/ground.png"); // 1
            _ceiling_texture = new Texture("./assets/textures/ceiling.png"); // 2

            cubeGameObject = new(_cube_mesh,0);

            gameObjects.Add(cubeGameObject);

            //_camera = new OrthographicCamera(Size.X / 100f, Size.Y / 100f);
            _camera = new PerspectiveCamera(90f, (float)Size.X / Size.Y);
            // _camera.position.Z = 3f;// afasta a câmera para trás.

            GL.Enable(EnableCap.DepthTest);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            cubeGameObject.transform.rotation.Y = _time * 90f;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            UpdateTime((float)args.Time);
            FrameCleanup(args);
            ShaderRender();
            CameraRender();

            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Render(_program);
            }

            SwapBuffers();// Troca de buffers, apresentando o desenho completo ao usuário.
        }

        

        private void CameraRender()
        {
            // Envio das matrizes da câmera
            _program.SetUniform("u_View", _camera.ViewMatrix);
            _program.SetUniform("u_Projection", _camera.ProjectionMatrix);
        }

        private void FrameCleanup(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            // Escolha da cor de limpeza.
            GL.ClearColor(CLEAR_COLOR.X,CLEAR_COLOR.Y,CLEAR_COLOR.Z,CLEAR_COLOR.W);
            // Limpeza das cores e profundidade da tela.
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private void ShaderRender()
        {
            _program.Use();// Escolha do Shader Program.
            // Para usar texturas, dizemos em qual "caixinha" a queremos colocar. Para acessar essa
            //caixinha nos shaders, atribuímos o id da caixinha à variável uniforme correspondente.
            _wall_texture.Use(0);
            _ground_texture.Use(1);
            _ceiling_texture.Use(2);
        }

        private void UpdateTime(float new_time)
        {
            _delta_time = new_time-_time;
            _time = new_time;
        }
    }
}
