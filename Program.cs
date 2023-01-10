using System;
using System.Collections.Generic;
using System.Linq;

namespace AStar_Pathfinding
{
    public enum TypeOfNode { Start, Normal, End, Wall };

    public readonly struct Coordinates
    {
        public readonly int X;
        public readonly int Y;

        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
    
    public class Node
    {
        public TypeOfNode TypeOfNode;
        //Vorherige Node / weg zu dieser Node
        public Node Previous; 
        // X und Y position
        public Coordinates Coordinates;
        //G = Distance from starting node / H = Distance from end node / F = G + H
        public int G, H, F;

        public Node(Coordinates coordinates)
        {
            Coordinates = coordinates;
            TypeOfNode = TypeOfNode.Normal;
        }
    }

    public class Map
    {
        private int Width { get; set; }
        private int Height { get; set; }
        private Node _start;
        private Node _end;
        private readonly Node[,] _map;

        // Auch offene Liste gennant
        private readonly List<Node> _searching = new List<Node>(); 
        // Auch geschlossene Liste gennant
        public readonly List<Node> Searched = new List<Node>(); 

        public Map(int wi, int he)
        {
            Width = wi;
            Height = he;

            _map = new Node[wi, he];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    _map[i, j] = new Node(new Coordinates(i, j));
                }
            }
        }
        public void SetStart(Coordinates cor)
        {
            _map[cor.X, cor.Y].Coordinates = cor;
            _map[cor.X, cor.Y].TypeOfNode = TypeOfNode.Start;
            _start = _map[cor.X, cor.Y];
        }
        public void SetEnd(Coordinates cor)
        {
            _map[cor.X, cor.Y].Coordinates = cor;
            _map[cor.X, cor.Y].TypeOfNode = TypeOfNode.End;
            _end = _map[cor.X, cor.Y];
        }

        public Node GetEnd()
        {
            return _end;
        }
        public void SetWalls(List<int[]> walls)
        {
            foreach (int[] t in walls)
            {
                _map[t[0], t[1]].TypeOfNode = TypeOfNode.Wall;
            }
        }
        
        //Errechnet die G Cost / die Distanz zu dem Startpunkt
        private void CalculateGCost(int x, int y, Node previous) 
        {
            int cost = 10;

            if (_map[x, y].TypeOfNode == TypeOfNode.Start || _map[x, y].TypeOfNode == TypeOfNode.End) return;
            if (previous.Coordinates.X != x && previous.Coordinates.Y != y)
            {
                cost += 4;
            }

            _map[x, y].G = previous.G + cost;
            _map[x, y].Previous = previous;
        }
        
        //Errechnet für die ganze Karte die H Cost / die Distanz zu dem Endpunkt
        public void CalculateAllHCost() 
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    CalculateHCost(i, j);
                }
            }
        }
        
        //Errechnet für dieses eine Feld die H Cost / die Distanz zu dem Endpunkt
        private void CalculateHCost(int x, int y)
        {
            if (_map[x, y].TypeOfNode == TypeOfNode.End) return;
            //Wie weit das ende von dem jetzigen Punkt entfernt ist
            _map[x, y].H = (Math.Abs(_end.Coordinates.X - x) + Math.Abs(_end.Coordinates.Y - y)) * 10;
        }
        
        //F Cost ist H Cost + G Cost
        private void CalculateFCost(int x, int y) => _map[x, y].F = _map[x, y].G + _map[x, y].H; 
        public void StartPathFinding()
        {
            _searching.Add(_start);
            ShowMap();
            Search();
        }
        //Findet die benachbarten Nodes und fügt sie in die offene Liste hinzu, wenn sie dort nicht schon sind mit einem besseren vorgänger.
        private void PathFinding(int x, int y) 
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (!((x + i >= 0 && x + i < Width) & (y + j >= 0 && y + j < Height))) continue;
                    if (_map[x + i, y + j].TypeOfNode == TypeOfNode.Wall) continue;
                    if (_map[x + i, y + j].G == 0 & _map[x + i, y + j].TypeOfNode != TypeOfNode.Start)
                    {
                        _map[x + i, y + j].Previous = _map[x, y];
                        _searching.Add(_map[x + i, y + j]);
                    }
                    else if (_map[x + i, y + j].G > _map[x, y].G + (i == 0 && j == 0 ? 10 : 14))
                    {
                        _map[x + i, y + j].Previous = _map[x, y];
                        var t = Searched.Find(
                            item => item.Coordinates.X == x + 
                                i && item.Coordinates.Y == y + j);
                        Searched.Remove(t);
                        _searching.Add(_map[x + i, y + j]);
                    }
                }
            }
        }
        //Anweisungen wie der Weg gefunden wird
        private void Search() 
        {
            Node node;
            do
            {
                foreach (var item in _searching)
                {
                    CalculateGCost(item.Coordinates.X, item.Coordinates.Y, item.Previous); 
                    CalculateFCost(item.Coordinates.X, item.Coordinates.Y);
                }

                //Sucht die niedrigste F Cost
                int lowestF = _searching.Min(c => c.F); 
                //Sucht in der offenen Liste nach den Nodes mit der niedrigsten F Cost
                var nodes = _searching.FindAll(c => c.F == lowestF); 
                //Sucht unter den niedrigsten F Cost Nodes nach der niedrigsten H Cost
                int lowestH = nodes.Min(c => c.H); 
                //Wählt die Node mit der niedrigsten F und H Cost aus
                node = nodes.Find(c => c.H == lowestH); 

                //Sollte die Node zufälligerweise die End-Node sein, wird die Suche nach der End-Node abgebrochen
                if (node.TypeOfNode == TypeOfNode.End)
                    break;

                //Sucht für die Node mit der niedrigsten F und H Cost die Nachbarn und fügt sie der offenen Liste hinzu
                PathFinding(node.Coordinates.X, node.Coordinates.Y); 
                //Entfernt die Node von der offenen Liste
                _searching.Remove(node); 
                //und fügt sie der geschlossenen Liste hinzu
                Searched.Add(node);
            } while (_searching.Count != 0 && node.TypeOfNode != TypeOfNode.End); 
        }
        public void ShowMap()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Console.SetCursorPosition(i, j);

                    switch (_map[i,j].TypeOfNode)
                    {
                        case TypeOfNode.Start:
                            Console.ForegroundColor = ConsoleColor.Blue;
                            break;
                        case TypeOfNode.End:
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Gray;
                            break;
                    }
                    
                    //Schreibt ein X wenn es sich bei dem Feld um eine Wand / das Ende / den Start handelt
                    Console.WriteLine(_map[i, j].TypeOfNode == TypeOfNode.Wall |
                                      _map[i, j].TypeOfNode == TypeOfNode.End | 
                                      _map[i, j].TypeOfNode == TypeOfNode.Start ? "X" : " "); 
                }
            }
        }

        public void ShowPath(Node node)
        {
            while (true)
            {
                System.Threading.Thread.Sleep(100);
                Console.SetCursorPosition(node.Coordinates.X, node.Coordinates.Y);
                Console.BackgroundColor = ConsoleColor.White;
                Console.WriteLine("*");
                Console.BackgroundColor = ConsoleColor.Black;
                switch (node.TypeOfNode)
                {
                    case TypeOfNode.Start:
                        //Die Reihe ist fertig, es wird an das ende der Map gesetzt um weiter Infos
                        //auf der Konsole auszugeben ohne die Karte zu überschreiben
                        Console.SetCursorPosition(0, Height + 1);
                        break;
                    default:
                        //Zeigt die nächste Node in der Reihe
                        node = node.Previous;
                        continue;
                }

                break;
            }
        }
    }

    internal static class Program
    {
        private static void Main()
        {
            Console.ReadKey(true);
            
            int lineCount = 0; 
            int longestLine = 0; 
            string line; 
            
            List<int[]> walls = new List<int[]>();

            Coordinates start = new Coordinates();
            Coordinates end = new Coordinates();

            System.IO.StreamReader file =
                new System.IO.StreamReader(@"..\..\Input.txt");
            while ((line = file.ReadLine()) != null)
            {
                if (line.Length > longestLine)
                {
                    longestLine = line.Length;
                }

                int counter2 = 0;
                foreach (char x in line)
                {
                    switch (x)
                    {
                        case 'X':
                            walls.Add(new[] { counter2, lineCount });
                            break;

                        case 'S':
                            start = new Coordinates(counter2, lineCount);
                            break;

                        case 'E':
                            end = new Coordinates(counter2, lineCount);
                            break;
                    }
                    counter2++;
                }
                lineCount++;
            }

            var map = new Map(longestLine, lineCount);

            map.SetStart(start);
            map.SetEnd(end);
            map.SetWalls(walls);

            file.Dispose();
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            map.CalculateAllHCost();
            map.StartPathFinding();
            sw.Stop();

            map.ShowMap();
            map.ShowPath(map.GetEnd());

            Console.WriteLine("The Path took {0} ms to calculate", sw.ElapsedMilliseconds);

            foreach (var node in map.Searched)
            {
                //Alle untersuchten Nodes werden einmal aufgeführt
                Console.WriteLine("{0,-6} : G = {1,-4}| H = {2,-4}| F = {3,-4}", 
                    node.Coordinates.X + ", " + node.Coordinates.Y, node.G, node.H, node.F);
            }

            Console.ReadKey(true);
        }
    }
}
