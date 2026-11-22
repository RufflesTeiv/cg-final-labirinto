using System.Drawing;
using CG.Core;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CG
{
    internal class Game : GameWindow
    {
        private Vector4 CLEAR_COLOR = new Vector4(0f, 0.3f, 0.5f, 1f);

        private ShaderProgram program;
        private Texture ceilingTexture, groundTexture, wallTexture;
        private Camera camera;
        private float cameraXInclination = -20f;
        private List<GameObject> gameObjects = new();

        private float totalTime;
        private float deltaTime;

        // Maze
        private Maze maze;

        // Player
        private MazeCell currentCell;
        private MazeDirection facingDirection;
        private float moveSpeed = 10f;
        private float rotationSpeed = 10f;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            CreateShaderProgram();
            CreateTextures();
            CameraSetup();
            OpenGLSetup();

            // Maze
            maze = new Maze(12);
            gameObjects.AddRange(maze.GetGameObjects(1,0,2));

            // Player
            currentCell = maze.cells[0,0];
            facingDirection = MazeDirection.North;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            // camera.rotation.Y = totalTime * 90f;
            MoveTowardsTarget();
            RotateTowardsTarget();
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

        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.IsRepeat) return;
            switch (e.Key)
            {
                case Keys.W: MoveForward(); break;
                case Keys.A: TurnLeft(); break;
                case Keys.S: MoveBackward(); break;
                case Keys.D: TurnRight(); break;
            }
        }

        
        private void CameraSetup()
        {
            //camera = new OrthographicCamera(Size.X / 100f, Size.Y / 100f);
            camera = new PerspectiveCamera(90f, (float)Size.X / Size.Y);
            camera.position = new Vector3(0f,0.5f,0f);
            // camera.rotation.X = -20f;
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

        private void MoveInMaze(MazeDirection direction)
        {
            if (currentCell.HasWall(direction))
                return;
            currentCell = maze.MoveToCell(currentCell.x,currentCell.z,direction) ?? currentCell;
        }
        private void MoveBackward() => MoveInMaze(MazeUtility.ReverseDirection(facingDirection));
        private void MoveForward() => MoveInMaze(facingDirection);
        private void MoveTowardsTarget()
        {
            float blend = MathUtility.ExponentialLerpBlend(moveSpeed,deltaTime);
            Vector3 targetPosition = TargetPosition();
            camera.position = Vector3.Lerp(camera.position,targetPosition,blend);
            if (Vector3.DistanceSquared(camera.position,targetPosition) < 0.0001f)
            {
                camera.position = targetPosition;
            }
        }
        private void OpenGLSetup()
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);
        }
        private void RotateTowardsTarget()
        {
            float blend = MathUtility.ExponentialLerpBlend(rotationSpeed,deltaTime);
            Vector3 targetRotation = TargetRotation();
            camera.rotation = MathUtility.LerpEulerDegrees(camera.rotation, targetRotation, blend);
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

        private Vector3 TargetPosition() => new Vector3(currentCell.x,0.5f,currentCell.z);

        private Vector3 TargetRotation()
        {
            float yRotation = 0f;
            switch (facingDirection)
            {
                case MazeDirection.East: return new Vector3(cameraXInclination,-90f,0f);
                case MazeDirection.South: return new Vector3(cameraXInclination,180f,0f);
                case MazeDirection.West: return new Vector3(cameraXInclination,90f,0f);
            }
            return new Vector3(cameraXInclination,yRotation,0f);
        }

        private void TurnLeft() => facingDirection = MazeUtility.RotateDirectionLeft(facingDirection);
        private void TurnRight() => facingDirection = MazeUtility.RotateDirectionRight(facingDirection);

        private void UpdateTimeVariables(float _deltaTime)
        {
            deltaTime = _deltaTime;
            totalTime += deltaTime;
        }
    }
}
