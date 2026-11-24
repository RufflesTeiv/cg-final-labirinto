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
        private List<Texture> textures;
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
        private bool hasKey = false;
        private bool win = false;

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            CreateShaderProgram();
            CreateTextures();
            OpenGLSetup();

            // Maze
            maze = new Maze(3);
            gameObjects.AddRange(maze.GetGameObjects());

            // Player
            currentCell = maze.startingCell;
            CameraSetup();
            Console.WriteLine($"Starting position: [{maze.startingCell.x},{maze.startingCell.z}]");
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
            if (win || e.IsRepeat) return;
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
            camera.position = currentCell.CameraPosition();
            // camera.rotation.X = -20f;
        }
        private void CameraRender()
        {
            // Envio das matrizes da câmera
            program.SetUniform("u_View", camera.ViewMatrix);
            program.SetUniform("u_Projection", camera.ProjectionMatrix);
        }
        private void CheckKey()
        {
            if (!currentCell.isKey) return;
            currentCell.PickUpKey();
            hasKey = true;
            gameObjects.Remove(maze.keyGameObj);
            Console.WriteLine("Pegou chave!");
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
            textures =
            [
                new Texture("./assets/textures/wall.png"),
                new Texture("./assets/textures/ground.png"),
                new Texture("./assets/textures/ceiling.png"),
                new Texture("./assets/textures/chave.png"),
                new Texture("./assets/textures/porta.png")
            ];
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
            if (currentCell.isExit && currentCell.exitDirection == direction && hasKey)
                WinTheGame();
            if (currentCell.HasWall(direction))
                return;
            currentCell = maze.MoveToCell(currentCell.x,currentCell.z,direction) ?? currentCell;
            CheckKey();
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
            for (int i = 0; i < textures.Count; i++)
            {
                textures[i].Use(i);
            }
        }
        private Vector3 TargetPosition() => currentCell.CameraPosition();
        private Vector3 TargetRotation() => new Vector3(cameraXInclination,MazeUtility.DirectionToYRotation(facingDirection),0f);
        private void TurnLeft() => facingDirection = MazeUtility.RotateDirectionLeft(facingDirection);
        private void TurnRight() => facingDirection = MazeUtility.RotateDirectionRight(facingDirection);
        private void UpdateTimeVariables(float _deltaTime)
        {
            deltaTime = _deltaTime;
            totalTime += deltaTime;
        }
        private void WinTheGame()
        {
            win = true;
            Console.WriteLine("Ganhou o jogo!");
        }
    }
}
