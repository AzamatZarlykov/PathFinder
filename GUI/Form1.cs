﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Path_Finder.Grid;
using Path_Finder.Constants;
using System.Linq;

namespace Path_Finder.GUI
{
    /// <summary>
    /// The Visual part of the program.
    /// </summary>
    public partial class Form1 : Form
    {
        private string currentAlgorithm;
        private bool wDown;

        private List<Position> path = new List<Position>();
        private List<Position> allVisitedPositions = new List<Position>();

        private List<Position> pathToBomb = new List<Position>();
        private List<Position> allPathToBombVisited = new List<Position>();

        private readonly Pen pen = new Pen(Brushes.Gray, 2);
        private readonly StringFormat format = new StringFormat();
        private readonly Font font = new Font("Times New Roman", 11);
        private readonly Board board = new Board();

        private bool isVisualize;
        private bool isMouseDown, isStartMove, isEndMove, isWallMove, isBombMove;

        public Form1()
        {
            InitializeComponent();
        }

        #region Algorithm Events
        private void BFS(object sender, EventArgs args)
        {
            currentAlgorithm = "BFS";
            board.SetAlgorithm("BFS");
        }

        private void DFS(object sender, EventArgs args)
        {
            currentAlgorithm = "DFS";
            board.SetAlgorithm("DFS");
        }

        private void Dijkstra(object sender, EventArgs args)
        {
            currentAlgorithm = "Dijkstra";
            board.SetAlgorithm("Dijkstra");
        }

        private void AStarEuclidean(object sender, EventArgs args)
        {
            currentAlgorithm = "AStarEuclidean";
            board.SetAlgorithm("AStarEuclidean");
        }

        private void AStarManhattan(object sender, EventArgs args)
        {
            currentAlgorithm = "AStarManhattan";
            board.SetAlgorithm("AStarManhattan");
        }

        private void SmartDFS(object sender, EventArgs args)
        {
            currentAlgorithm = "SmartDFS";
            board.SetAlgorithm("SmartDFS");
        }
        #endregion

        #region Maze Events
        private void CallRandomMaze(object sender, EventArgs args) 
        {
            board.GenerateRandomMaze();
            if (!board.BombSet)
            {
                if (board.IsPathFound(board.GetStartingPosition(), board.GetEndPosition()))
                {
                    Invalidate();
                }
                else
                {
                    CallRandomMaze(sender, args);
                }
            } else
            {
                if (board.IsPathFound(board.GetStartingPosition(), board.GetBombPosition()) && 
                    board.IsPathFound(board.GetBombPosition(), board.GetEndPosition()))
                {
                    Invalidate();
                } else
                {
                    CallRandomMaze(sender, args);
                }
            }

        }

        private void CallRecursiveMaze(object sender, EventArgs args)
        {
            board.GenerateRecursiveMaze();

            if (!board.BombSet)
            {
                if (board.IsPathFound(board.GetStartingPosition(), board.GetEndPosition()))
                {
                    Invalidate();
                }
                else
                {
                    CallRecursiveMaze(sender, args);
                }
            }
            else
            {
                if (board.IsPathFound(board.GetStartingPosition(), board.GetBombPosition()) &&
                    board.IsPathFound(board.GetBombPosition(), board.GetEndPosition()))
                {
                    Invalidate();
                }
                else
                {
                    CallRecursiveMaze(sender, args);
                }
            }
        }
        #endregion

        #region Creating Button and Label on Form
        private Button CreateButton(string name, int posX, int posY, int width, int height)
        {
            Button button = new Button();
            this.Controls.Add(button);
            button.Name = name;
            button.Text = name;
            button.Location = new Point(posX, posY);
            button.Height = height;
            button.Width = width;
            button.BackColor = Color.White;
            button.ForeColor = Color.Black;
            button.Font = new Font("Georgia", 12);

            if (name == ViewConstants.clearBoardName)
            {
                button.Click += new EventHandler(ClearBoard);
            }
            else if (name == ViewConstants.addBombName)
            {
                button.Click += new EventHandler(AddBomb);
            }
            else if (name == ViewConstants.visualizeName)
            {
                button.Click += new EventHandler(Visualize);
                button.BackColor = Color.Red;
            } 
            return button;
        }

        private void CreateTextLabel(string name, int posX, int posY, int width, int height)
        {
            Label label = new Label();
            this.Controls.Add(label);
            label.Name = name;
            label.Text = name;
            label.Location = new Point(posX, posY);
            label.Height = height;
            label.Width = width;
            label.Font = font;
            label.AutoSize = true;
        }
        #endregion

        #region Button Events

        private void ClearAllPaths()
        {
            if (board.BombSet)
            {
                pathToBomb.Clear();
                allPathToBombVisited.Clear();
            }
            path.Clear();
            allVisitedPositions.Clear();
        }

        private void ClearBoard(Object sender, EventArgs args)
        {
            currentAlgorithm = "";
            
            ClearAllPaths();

            isVisualize = false;
            board.SetAlgorithm("None");

            RemoveBomb(sender, args);
            board.ClearBoard();
            Invalidate();
        }

        private void AddBomb(Object sender, EventArgs args)
        {
            ClearAllPaths();

            Position bombPosition = board.GetBombPosition();

            if(!board.IsTaken(bombPosition.x, bombPosition.y))
            {
                board.AddBomb();
                addRemoveButton.Name = ViewConstants.removeBombName;
                addRemoveButton.Text = ViewConstants.removeBombName;
                addRemoveButton.Click -= AddBomb;
                addRemoveButton.Click += RemoveBomb;
                Invalidate();
            }
        }

        private void RemoveBomb(Object sender, EventArgs args)
        {
            if(board.BombSet)
            {
                board.RemoveBomb();
                addRemoveButton.Name = ViewConstants.addBombName;
                addRemoveButton.Text = ViewConstants.addBombName;
                addRemoveButton.Click -= RemoveBomb;
                addRemoveButton.Click += AddBomb;
                Invalidate();
            }
        }

        private void Visualize(Object sender, EventArgs args)
        {
            if (!board.AnyAlgorithmSet())
            {
                MessageBox.Show("The Algorithm should be selected");
            } 
            else
            {
                isVisualize = true;

                // Before calling any search algorithms, reset the visited 
                // property in each Cell of the grid
                board.ResetSearching();

                if(!board.BombSet)
                {
                    (path, allVisitedPositions) = board.GetSearchPath(board.GetStartingPosition(),
                                                  board.GetEndPosition());
                } else
                {
                    (pathToBomb, allPathToBombVisited) = board.GetSearchPath(
                        board.GetStartingPosition(), board.GetBombPosition());

                    board.ResetSearching();

                    (path, allVisitedPositions) = board.GetSearchPath(board.GetBombPosition(),
                                                  board.GetEndPosition());

                    path = (from p in pathToBomb.Union(path) select p).ToList();
                }

                if (!board.BombSet)
                {
                    if (!board.PathFound)
                    {
                        MessageBox.Show("The path is not found");
                    }
                }
                else
                {
                    if (!board.PathToBombFound)
                    {
                        MessageBox.Show("The path to the bomb is not found");
                    }
                    else if (!board.PathFound)
                    {
                        MessageBox.Show("The path from the bomb is not found");
                    }
                }
                Invalidate();
            }
        }
        #endregion

        #region Keyboard Events
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Keys.W == e.KeyCode)
            {
                wDown = true;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (Keys.W == e.KeyCode)
            {
                wDown = false;
            }
        }

        #endregion

        #region Mouse Events
        protected override void OnMouseDown(MouseEventArgs e)
        {
            Position position = new Position
                        (
                            (e.X - BoardConstants.MARGIN) / BoardConstants.SQUARE, 
                            (e.Y - ViewConstants.LEFTOVER) / BoardConstants.SQUARE
                        );

            if (board.InsideTheBoard(e.X, e.Y))
            {
                if (board.IsEmpty(position.x, position.y))
                {
                    if (wDown)
                    {
                        board.SetCell(position.x, position.y, CellType.WEIGHT);
                    }
                    else
                    {
                        board.SetCell(position.x, position.y, CellType.WALL);
                    }
                }
                else if (board.IsWall(position.x, position.y) ||
                         board.IsWeightNode(position.x, position.y))
                {
                    board.SetCellToEmpty(position.x, position.y);
                }
                isMouseDown = true;

                Invalidate();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Position position = new Position
                    (
                        (e.X - BoardConstants.MARGIN) / BoardConstants.SQUARE, 
                        (e.Y - ViewConstants.LEFTOVER) / BoardConstants.SQUARE 
                    );
            
            if (isMouseDown && board.InsideTheBoard(e.X, e.Y))
            {
                if ((board.IsStartPosition(position.x, position.y) || isStartMove) &&
                !isWallMove && !isBombMove && !wDown)
                {
                    board.SetStartPosition(position.x, position.y);
                    isStartMove = true;
                }
                else if ((board.IsEndPosition(position.x, position.y) || isEndMove) &&
                    !isWallMove && !isBombMove && !wDown)
                {
                    board.SetEndPosition(position.x, position.y);
                    isEndMove = true;
                }
                else if (board.BombSet)
                {
                    if ((board.IsBombPosition(position.x, position.y) || isBombMove) &&
                        !isWallMove && !wDown)
                    {
                        board.SetBombPosition(position.x, position.y);
                        isBombMove = true;
                    }
                }

                else if (board.IsEmpty(position.x, position.y))
                {
                    if (wDown)
                    {
                        board.SetWallOrWeight(position.x, position.y, CellType.WEIGHT, false);
                    }
                    else if (!wDown && !isStartMove && !isEndMove && !isBombMove)
                    {
                        board.SetWallOrWeight(position.x, position.y, CellType.WALL, false);
                        isWallMove = true;
                    }
                } else if (!board.IsEmpty(position.x, position.y))
                {
                    if (board.IsWeightNode(position.x, position.y) || 
                        (board.IsWall(position.x, position.y) && !isStartMove && !isEndMove && 
                        !isBombMove && !wDown))
                    {
                        board.SetCellToEmpty(position.x, position.y, false);
                    }
                }
                Invalidate();
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            isMouseDown = isStartMove = isEndMove = isWallMove = isBombMove = false;
        }
        #endregion

        #region Paint Events
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (isVisualize)
            {
                DrawAllVisitedPositions(e, g);
            }

            DrawToolBoxItems(g);

            if (board.AnyAlgorithmSet())
            {
                visualizeButton.BackColor = Color.Lime;
            } else
            {
                visualizeButton.BackColor = Color.Red;
            }

            DrawStartPosition
                (
                    g, board.GetStartingPosition().x * BoardConstants.SQUARE + 
                    BoardConstants.MARGIN,
                    board.GetStartingPosition().y * BoardConstants.SQUARE + ViewConstants.LEFTOVER
                );

            DrawEndPosition
                (
                    g, board.GetEndPosition().x * BoardConstants.SQUARE + BoardConstants.MARGIN,
                    board.GetEndPosition().y * BoardConstants.SQUARE + ViewConstants.LEFTOVER
                );

            if(board.BombSet)
            {
                DrawEllipseRectange
                    (
                        g, board.GetBombPosition().x * BoardConstants.SQUARE + 
                        BoardConstants.MARGIN,
                        board.GetBombPosition().y * BoardConstants.SQUARE + ViewConstants.LEFTOVER
                    );
            }

            DrawWalls(g);

            DrawWeights(g);

            DrawCost(g);

            DrawBoard(g);
        }

        private void DrawCost(Graphics g)
        {
            // Create a TextFormatFlags with word wrapping, horizontal center and
            // vertical center specified.
            TextFormatFlags flags = TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;

            Rectangle rect = new Rectangle
                (
                    8 * ViewConstants.BUTTONWIDTH - 2 * BoardConstants.SQUARE,
                    BoardConstants.MARGIN + BoardConstants.SQUARE, ViewConstants.BUTTONWIDTH +
                    ViewConstants.LABELWIDTH - 2 * BoardConstants.MARGIN, 
                    BoardConstants.TOOLBOXHEIGHT - 3 * BoardConstants.MARGIN
                );

            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;
      
            if (board.AnyAlgorithmSet())
            {
                // Draw the text and the surrounding rectangle.
                TextRenderer.DrawText(g, $@"Cost of {currentAlgorithm}: {(path.Count == 0 ? 0 : 
                    path.Count - 1)} ", font, rect, Color.Black, flags);
            } else
            {
                TextRenderer.DrawText(g, $@"Cost of the Algorithm: {(path.Count == 0 ? 0 : 
                    path.Count - 1)} ", font, rect, Color.Black, flags);
            }
            g.DrawRectangle(pen, rect);
        }
        
        private void DrawPosition(Graphics g, Position p, Brush color)
        {
            int valueX = p.x * BoardConstants.SQUARE + BoardConstants.MARGIN;
            int valueY = p.y * BoardConstants.SQUARE + ViewConstants.LEFTOVER;

            Rectangle r = new Rectangle
                    (
                        valueX, valueY,
                        BoardConstants.SQUARE, BoardConstants.SQUARE
                    );
            g.FillRectangle(color, r);
        }

        private void DrawPathHelper(Graphics g, List<Position> passedList, Brush color)
        {
            for (int i = 0; i < passedList.Count; i++)
            {
                DrawPosition(g, passedList[i], color);
            }
        }


        private void DrawAllVisitedPositions(PaintEventArgs e, Graphics g)
        {

            if(board.BombSet)
            {
                DrawPathHelper(g, allPathToBombVisited, Brushes.PowderBlue);

                DrawPathHelper(g, allVisitedPositions, Brushes.MediumOrchid);

                DrawPathHelper(g, pathToBomb, Brushes.Tomato);
            } else
            {
                DrawPathHelper(g, allVisitedPositions, Brushes.PowderBlue);

                DrawPathHelper(g, path, Brushes.Tomato);
            }
        }

        private void DrawWeights(Graphics g)
        {
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;

            Rectangle rect;
            Cell[,] grid = board.GetGrid();

            for(int i = 0; i < BoardConstants.ROWSIZE; i++)
            {
                for(int j = 0; j < BoardConstants.COLUMNSIZE; j++)
                {
                    if(grid[i,j].type == CellType.WEIGHT)
                    {
                        rect = new Rectangle(j * BoardConstants.SQUARE + BoardConstants.MARGIN,
                                i * BoardConstants.SQUARE + ViewConstants.LEFTOVER,
                                BoardConstants.SQUARE, BoardConstants.SQUARE);

                        g.FillRectangle( Brushes.PeachPuff, rect );

                        g.DrawString("!", font, Brushes.Black, rect, format);
                    }
                }
            }

            
        }

        private void DrawWalls(Graphics g)
        {
            Cell[,] grid = board.GetGrid();
            for(int i = 0; i < BoardConstants.ROWSIZE; i++)
            {
                for(int j = 0; j < BoardConstants.COLUMNSIZE; j++)
                {
                    if(grid[i,j].type == CellType.WALL)
                    {
                        g.FillRectangle
                            (
                                Brushes.Black, j * BoardConstants.SQUARE + BoardConstants.MARGIN,
                                i * BoardConstants.SQUARE + ViewConstants.LEFTOVER, 
                                BoardConstants.SQUARE, BoardConstants.SQUARE
                            );
                    }
                }
            }
        }

        private void DrawToolBoxItems(Graphics g)
        {
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;

            DrawStartPosition
                (
                    g, BoardConstants.MARGIN + 1 + ViewConstants.LABELWIDTH, 
                    2 * BoardConstants.SQUARE + 4
                );
            DrawEndPosition
                (
                    g, BoardConstants.MARGIN + 2 * ViewConstants.LABELWIDTH + 
                    BoardConstants.SQUARE, 
                    2 * BoardConstants.SQUARE + 4
                );

            DrawEllipseRectange
                (
                    g, BoardConstants.MARGIN + 3 * ViewConstants.LABELWIDTH + 2 * 
                    BoardConstants.SQUARE, 2 * BoardConstants.SQUARE + 4
                );
            DrawWallNode
                (
                    g, 3 * BoardConstants.MARGIN + 4 * ViewConstants.LABELWIDTH + 2 * 
                    BoardConstants.SQUARE, 2 * BoardConstants.SQUARE + 4
                );

            DrawVisitedNodes
                (
                    g, 5 * ViewConstants.LABELWIDTH + 4 * BoardConstants.SQUARE, 
                    2 * BoardConstants.SQUARE + 4
                );
            DrawVisitedNodes
                (
                    g, BoardConstants.MARGIN + 5 * ViewConstants.LABELWIDTH + 5 * 
                    BoardConstants.SQUARE, 2 * BoardConstants.SQUARE + 4, isFirstDestination:false
                );

            DrawShortestPathNode
                (
                    g, 2 * BoardConstants.MARGIN + 6 * ViewConstants.LABELWIDTH + 8 * 
                    BoardConstants.SQUARE, 2 * BoardConstants.SQUARE + 4
                );

            DrawWeightNode
                (
                    g, BoardConstants.MARGIN + 7 * ViewConstants.LABELWIDTH + 9 * 
                    BoardConstants.SQUARE + BoardConstants.MARGIN, 2 * BoardConstants.SQUARE + 4
                );
        }

        private void DrawWeightNode(Graphics g, int posX, int posY)
        {
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;

            Rectangle wNode = new Rectangle
                (
                    posX, posY, BoardConstants.SQUARE, BoardConstants.SQUARE
                );

            g.FillRectangle(Brushes.PeachPuff, wNode);
            g.DrawString("!", font, Brushes.Black, wNode, format);
        }

        private void DrawShortestPathNode(Graphics g, int posX, int posY)
        {
            Rectangle path = new Rectangle
                (
                    posX, posY, BoardConstants.SQUARE, BoardConstants.SQUARE
                );
            g.FillRectangle(Brushes.Tomato, path);
        }

        private void DrawVisitedNodes(Graphics g, int posX, int PosY, 
            bool isFirstDestination = true)
        {
            Rectangle visited = new Rectangle
                (
                    posX, PosY, BoardConstants.SQUARE, BoardConstants.SQUARE
                );
            if (isFirstDestination)
            {
                g.FillRectangle(Brushes.PowderBlue, visited);
            }
            else
            {
                g.FillRectangle(Brushes.MediumOrchid, visited);
            }
        }

        private void DrawWallNode(Graphics g, int posX, int posY)
        {
            Rectangle unvisitedNode = new Rectangle
                (
                    posX, posY, BoardConstants.SQUARE, BoardConstants.SQUARE
                );

            g.FillRectangle(Brushes.Black, unvisitedNode);
        }

        private void DrawEllipseRectange(Graphics g, int posX, int posY)
        {
            Pen extraPen = new Pen(Brushes.Purple, 2);
            Pen extraInnerPen = new Pen(Brushes.Purple, 4);

            Rectangle circle = new Rectangle
                                    (
                                        posX - 1, posY - 1, 
                                        BoardConstants.SQUARE - 1, 
                                        BoardConstants.SQUARE - 1
                                    );
            Rectangle innerCircle = new Rectangle
                                    (
                                        posX + BoardConstants.MARGIN + 1, 
                                        posY + BoardConstants.MARGIN + 1, 
                                        BoardConstants.MARGIN, 
                                        BoardConstants.MARGIN
                                    );
            
            g.DrawEllipse(extraPen, circle);
            g.DrawEllipse(extraInnerPen, innerCircle);
        }

        private void DrawStartPosition(Graphics g, int posX, int posY)
        {

            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;

            Rectangle startRectangle = new Rectangle
                                           (
                                                posX, posY, BoardConstants.SQUARE, 
                                                BoardConstants.SQUARE
                                           );

            g.FillRectangle(Brushes.Aqua, startRectangle);
            g.DrawString("S", font, Brushes.Black, startRectangle, format);
        }

        private void DrawEndPosition(Graphics g, int posX, int posY)
        {
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;

            Rectangle endRectangle = new Rectangle
                                            (
                                                posX, posY, BoardConstants.SQUARE, 
                                                BoardConstants.SQUARE
                                            );

            g.FillRectangle(Brushes.Lime, endRectangle);
            g.DrawString("E", font, Brushes.Black, endRectangle, format);
        }

        private void DrawBoard(Graphics g)
        {
            Rectangle boardOutLine = new Rectangle(
                                0, 0,
                                BoardConstants.WIDTH,
                                BoardConstants.HEIGHT
                                );

            Rectangle toolBoxOutLine = new Rectangle(
                                BoardConstants.MARGIN, BoardConstants.MARGIN + 
                                BoardConstants.SQUARE, BoardConstants.TOOLBOXWIDTH,
                                BoardConstants.TOOLBOXHEIGHT - BoardConstants.SQUARE + 
                                BoardConstants.MARGIN
                                );

            Rectangle gridOutLine = new Rectangle(
                                BoardConstants.MARGIN,ViewConstants.LEFTOVER,
                                BoardConstants.TOOLBOXWIDTH,
                                BoardConstants.HEIGHT - (ViewConstants.LEFTOVER + 
                                BoardConstants.MARGIN)
                                );

            g.DrawRectangle(pen, boardOutLine);
            g.DrawRectangle(pen, toolBoxOutLine);
            g.DrawRectangle(pen, gridOutLine);

            for (int x = 5; x <= BoardConstants.SQUARE * BoardConstants.COLUMNSIZE;
             x += BoardConstants.SQUARE)
            {
                g.DrawLine
                    (
                        pen, x, ViewConstants.LEFTOVER,
                        x, BoardConstants.HEIGHT - BoardConstants.MARGIN
                    );
            }
            for (int y = ViewConstants.LEFTOVER; y <= BoardConstants.HEIGHT - BoardConstants.SQUARE;
                    y += BoardConstants.SQUARE)
            {
                g.DrawLine
                    (
                        pen, 5, y, BoardConstants.SQUARE * BoardConstants.COLUMNSIZE +
                        BoardConstants.MARGIN, y
                    );
            }

        }

        #endregion
    }
}
