using System.Text;
using CG.Core;
using OpenTK.Mathematics;

namespace CG
{
    internal class Maze
    {
        public MazeCell[,] cells;
        int width, depth;
        public MazeCell startingCell, keyCell, exitCell;
        public GameObject keyGameObj, exitGameObj;
        public Maze(int _size, bool debug = false) : this(_size,_size, debug) {}
        public Maze(int _width, int _depth, bool debug = false)
        {
            width = Math.Max(_width,2);
            depth = Math.Max(_depth,2);
            cells = new MazeCell[width,depth];
            PopulateGrid();
            Generate(width,depth,debug);
            SetSpecialCells();
            SetSpecialGameObjs();
        }

        public MazeCell? MoveToCell(int x, int z, MazeDirection direction)
        { 
            int newX=x;
            int newZ=z;
            switch (direction)
            {
                case MazeDirection.North:
                    newZ = z-1;
                    break;
                case MazeDirection.South:
                    newZ = z+1;
                    break;
                case MazeDirection.East:
                    newX = x+1;
                    break;
                case MazeDirection.West:
                    newX = x-1;
                    break;
            }
            if (newX >= 0 && newX < width && newZ >=0 && newZ < depth)
                return cells[newX,newZ];
            return null;
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
        private List<MazeCell> GetCorners()
        {
            return
            [
              cells[0,0],
              cells[0,depth-1],
              cells[width-1,0],
              cells[width-1,depth-1]  
            ];
        }
        public List<GameObject> GetGameObjects()
        {
            List<GameObject> result = new List<GameObject>();
            for (int x = 0; x < width; x++)
            {
                for(int z = 0; z < depth; z++)
                {
                    result.AddRange(cells[x,z].GenerateGameObjects());
                }
            }
            result.Add(keyGameObj);
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
        public List<MazeDirection> GetPossibleExitDirections(MazeCell cell)
        {
            List<MazeDirection> exitDirections = new List<MazeDirection>();
            if (cell.x == 0) exitDirections.Add(MazeDirection.West);
            if (cell.x == width-1) exitDirections.Add(MazeDirection.East);
            if (cell.z == 0) exitDirections.Add(MazeDirection.North);
            if (cell.z == depth-1) exitDirections.Add(MazeDirection.South);
            return exitDirections;
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
        private void SetSpecialCells()
        {
            List<MazeCell> corners = GetCorners();
            int startingCellIdx = RandomManager.Integer(corners.Count);
            startingCell = corners[startingCellIdx];
            corners.RemoveAt(startingCellIdx);
            int keyCellIdx = RandomManager.Integer(corners.Count);
            keyCell = corners[keyCellIdx];
            keyCell.SetIsKey(true);
            corners.RemoveAt(keyCellIdx);
            int exitCellIdx = RandomManager.Integer(corners.Count);
            exitCell = corners[exitCellIdx];
            exitCell.SetupExit(GetPossibleExitDirections(exitCell));
        }
        private void SetSpecialGameObjs()
        {
            Mesh keyMesh = Mesh.CreatePrimitivePlane(1);
            keyGameObj = new GameObject(keyMesh,(int)TextureIdx.Key);
            keyGameObj.transform.position = new Vector3(keyCell.x,0.001f,keyCell.z);
        }
    }
}