using System.Text;
using CG.Core;
using OpenTK.Mathematics;

namespace CG
{
    internal class Maze
    {
        public MazeCell[,] cells;
        int width, depth;
        public Maze(int _size, bool debug = false) : this(_size,_size, debug) {}
        public Maze(int _width, int _depth, bool debug = false)
        {
            width = _width;
            depth = _depth;
            cells = new MazeCell[width,depth];
            PopulateGrid();
            Generate(width,depth,debug);
        }

        // Algoritmo de Prim
        // https://weblog.jamisbuck.org/2011/1/10/maze-generation-prim-s-algorithm
        private void Generate(int width, int depth, bool debug = false)
        {
            // Escolhe uma célula aleatória para começar e adiciona seus vizinhos à "fronteira"
            MazeCell startingCell = cells[RandomManager.Integer(width),RandomManager.Integer(depth)];
            List<MazeCell> inMazeCells = new List<MazeCell>{startingCell};
            List<MazeCell> frontierCells = GetNeighbors(startingCell);
            int step = 0;
            if (debug)
                PrintMaze(inMazeCells, frontierCells, $"Step {step}: Inicial");
            while (inMazeCells.Count < (width*depth))
            {
                // Escolhe uma célula da fronteira aleatoriamente
                int nextFrontierIdx = RandomManager.Integer(frontierCells.Count);
                MazeCell nextFrontierCell = frontierCells[nextFrontierIdx];
                frontierCells.RemoveAt(nextFrontierIdx);
                // A partir da célula da fronteira, escolhe uma célula vizinha que já esteja no labirinto 
                List<MazeCell> validCells = GetNeighbors(nextFrontierCell).Intersect(inMazeCells).ToList();
                int validCellIdx = RandomManager.Integer(validCells.Count);
                MazeCell selectedCell = validCells[validCellIdx];
                // Um caminho é "cavado" entre as células selecionadas
                selectedCell.RemoveWallTo(nextFrontierCell);
                nextFrontierCell.RemoveWallTo(selectedCell);
                // Adiciona a nova célula no labirinto, e adiciona novos vizinhos nas células de fronteira
                inMazeCells.Add(nextFrontierCell);
                List<MazeCell> newFrontierCells = GetNeighbors(nextFrontierCell).Except(inMazeCells).Except(frontierCells).ToList();
                frontierCells.AddRange(newFrontierCells);
                if (debug)
                    PrintMaze(inMazeCells,frontierCells,$"Step {step++}");
            }
        }
        public List<GameObject> GetGameObjects(int groundTextureIdx = 0,int wallTextureIdx = 1,int ceilingTextureIdx = 2)
        {
            List<GameObject> result = new List<GameObject>();
            for (int x = 0; x < width; x++)
            {
                for(int z = 0; z < depth; z++)
                {
                    result.AddRange(cells[x,z].GenerateGameObjects(groundTextureIdx,wallTextureIdx,ceilingTextureIdx));
                }
            }
            return result;
        }
        private List<MazeCell> GetNeighbors(MazeCell cell)
        {
            List<MazeCell> neighbors = new List<MazeCell>();
            int x = cell.x;
            int z = cell.z;
            if (cell.x > 0)
                neighbors.Add(cells[x-1,z]);
            if (cell.x < width-1)
                neighbors.Add(cells[x+1,z]);
            if (cell.z > 0)
                neighbors.Add(cells[x,z-1]);
            if (cell.z < depth-1)
                neighbors.Add(cells[x,z+1]);
            return neighbors;
        }
        private void PopulateGrid()
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    cells[x,z] = new MazeCell(x,z);
                }
            }
        }
        private void PrintMaze(List<MazeCell>? inMaze = null, List<MazeCell>? frontier = null, string header = "") 
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(header))
                sb.AppendLine(header);
            
            for (int z = 0; z < depth; z++)
            {
                // Top walls
                for (int x = 0; x < width; x++)
                {
                    sb.Append(cells[x,z].HasWall(MazeDirection.North) ? "+---" : "+   ");
                }
                sb.AppendLine("+");
                // Left walls
                for (int x = 0; x < width; x++)
                {
                    MazeCell cell = cells[x,z];
                    sb.Append(cell.HasWall(MazeDirection.West) ? "|" : " ");
                    // Mostrar status
                    if (inMaze != null && inMaze.Contains(cell))
                        sb.Append("   ");
                    else if (frontier != null && frontier.Contains(cell))
                        sb.Append(" ? ");
                    else sb.Append(" # ");
                }
                // Rightmost walls
                sb.AppendLine(cells[width-1,z].HasWall(MazeDirection.East) ? "|" : " ");
            }

            // Bottom walls
            for (int x = 0; x < width; x++)
            {
                sb.Append(cells[x,depth-1].HasWall(MazeDirection.South) ? "+---" : "+   ");
            }
            sb.AppendLine("+");
            Console.WriteLine(sb.ToString());
            Console.WriteLine("Pressione qualquer tecla para continuar...");
            Console.ReadKey(true);
        }
    }

    internal class MazeCell
    {

        public int x,z;
        public int wallFlags = 15;
        public MazeCell(int _x, int _z)
        {
            x = _x;
            z = _z;
        }

        public void AddWall(MazeDirection direction)
        {
            wallFlags = wallFlags | (int)direction;
        }
        public List<GameObject> GenerateGameObjects(int groundTextureIdx,int wallTextureIdx,int ceilingTextureIdx)
        {
            List<GameObject> result = new List<GameObject>();
            // Ground
            Mesh groundMesh = Mesh.CreatePrimitivePlane(1);
            GameObject groundGameObj = new GameObject(groundMesh,groundTextureIdx);
            groundGameObj.transform.position = new Vector3(x,0f,z);
            result.Add(groundGameObj);
            // Ceiling
            Mesh ceilingMesh = Mesh.CreatePrimitivePlane(1);
            GameObject ceilingGameObj = new GameObject(ceilingMesh,ceilingTextureIdx);
            ceilingGameObj.transform.position = new Vector3(x,1f,z);
            result.Add(ceilingGameObj);
            // Walls
            int flags = WallsToRender();
            List<MazeDirection> directions = new List<MazeDirection>{ MazeDirection.North, MazeDirection.East, MazeDirection.South, MazeDirection.West };
            foreach (MazeDirection direction in directions)
            {
                if ((flags & (int)direction) != 0)
                    result.Add(GenerateWallGameObject(wallTextureIdx,direction));
            }
            return result;
        }

        public GameObject GenerateWallGameObject(int wallTextureIdx, MazeDirection direction)
        {
            Mesh mesh = Mesh.CreatePrimitivePlane(1);
            GameObject result = new GameObject(mesh,wallTextureIdx);
            switch (direction)
            {
                case MazeDirection.North:
                {
                    result.transform.position = new Vector3(x,0.5f,z-0.5f);
                    result.transform.rotation = new Vector3(90f,0f,0f);
                    break;
                }
                case MazeDirection.East:
                {
                    result.transform.position = new Vector3(x+0.5f,0.5f,z);
                    result.transform.rotation = new Vector3(0f,-90f,90f);
                    break;
                }
                case MazeDirection.South:
                {;
                    result.transform.position = new Vector3(x,0.5f,z+0.5f);
                    result.transform.rotation = new Vector3(90f,0f,0f);
                    break;
                }
                default:
                {
                    result.transform.position = new Vector3(x-0.5f,0.5f,z);
                    result.transform.rotation = new Vector3(0f,90f,-90f);
                    break;
                }
            }
            return result;
        }
        public bool HasWall(MazeDirection direction)
        {
            return (wallFlags & (int)direction) != 0;
        }
        public void RemoveWall(MazeDirection direction)
        {
            wallFlags = wallFlags & ~(int)direction;
        }
        public MazeDirection GetDirectionTo(MazeCell otherCell)
        {
            if(x < otherCell.x) return MazeDirection.East;
            if(x > otherCell.x) return MazeDirection.West;
            if(z < otherCell.z) return MazeDirection.South;
            return MazeDirection.North;
        }
        public void RemoveWallTo(MazeCell otherCell) => RemoveWall(GetDirectionTo(otherCell));
        public int WallsToRender()
        {
            int wallsToRenderFlags = wallFlags;
            if (x > 0)
                wallsToRenderFlags = wallsToRenderFlags & ~(int)MazeDirection.West;
            if (z > 0)
                wallsToRenderFlags = wallsToRenderFlags & ~(int)MazeDirection.North;
            return wallsToRenderFlags;
        }
    }

    enum MazeDirection : int
    {
        North = 1,
        East = 2,
        South = 4,
        West = 8
    }
}