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

        private ShaderProgram program;
        private Texture ceilingTexture, groundTexture, wallTexture;
        private Camera camera;
        private List<GameObject> gameObjects = new();

        private float totalTime;
        private float deltaTime;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            CreateShaderProgram();
            CreateTextures();
            CameraSetup();
            OpenGLSetup();

            Maze maze = new Maze(12);
            gameObjects.AddRange(maze.GetGameObjects(1,0,2));
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            camera.rotation.Y = totalTime * 90f;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            UpdateTimeVariables((float)args.Time);
            FrameCleanup(args);
            ShaderRender();
            CameraRender();

            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Render(program);
            }

            SwapBuffers();// Troca de buffers, apresentando o desenho completo ao usuário.
        }

        
        private void CameraSetup()
        {
            //camera = new OrthographicCamera(Size.X / 100f, Size.Y / 100f);
            camera = new PerspectiveCamera(90f, (float)Size.X / Size.Y);
            camera.position = new Vector3(0f,0.5f,0f);
            camera.rotation.X = -20f;
        }
        private void CameraRender()
        {
            // Envio das matrizes da câmera
            program.SetUniform("u_View", camera.ViewMatrix);
            program.SetUniform("u_Projection", camera.ProjectionMatrix);
        }

        private void CreateShaderProgram()
        {
            // Com as novas classes de shader, podemos carregá-los diretamente de arquivos.
            VertexShader vertShader = VertexShader.LoadFromFile("./assets/shaders/shader.vert");
            FragmentShader fragShader = FragmentShader.LoadFromFile("./assets/shaders/shader.frag");

            // De forma similar, a classe ProgramShader junta os diferentes shaders.
            program = new(vertShader, fragShader);
        }

        private void CreateTextures()
        {
            // Carregamento de texturas, que serão armazenadas na placa de vídeo.
            wallTexture = new Texture("./assets/textures/wall.png"); // 0
            groundTexture = new Texture("./assets/textures/ground.png"); // 1
            ceilingTexture = new Texture("./assets/textures/ceiling.png"); // 2
        }

        private void FrameCleanup(FrameEventArgs args)
        {
            // Escolha da cor de limpeza.
            GL.ClearColor(CLEAR_COLOR.X,CLEAR_COLOR.Y,CLEAR_COLOR.Z,CLEAR_COLOR.W);
            // Limpeza das cores e profundidade da tela.
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        private void OpenGLSetup()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
        }

        private void ShaderRender()
        {
            program.Use();// Escolha do Shader Program.
            // Para usar texturas, dizemos em qual "caixinha" a queremos colocar. Para acessar essa
            //caixinha nos shaders, atribuímos o id da caixinha à variável uniforme correspondente.
            wallTexture.Use(0);
            groundTexture.Use(1);
            ceilingTexture.Use(2);
        }

        private void UpdateTimeVariables(float _deltaTime)
        {
            deltaTime = _deltaTime;
            totalTime += deltaTime;
        }
    }
}
