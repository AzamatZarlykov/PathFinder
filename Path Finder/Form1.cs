﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Path_Finder
{
    /// <summary>
    /// The Visual part of the program.
    /// </summary>
    public partial class Form1 : Form
    {
        private const int BUTTONWIDTH = 120;
        private const int BUTTONHEIGHT = 50;

        private const int LABELWIDTH = 100;
        private const int LABELHEIGHT = 40;

        private readonly Pen pen = new Pen(Brushes.Gray, 2);
        private readonly StringFormat format = new StringFormat();
        private readonly Font font = new Font("Times New Roman", 11);
        private readonly Board board = new Board();

        private bool isMouseDown, isStartMoving, isEndMoving, isWallMoving;

        private const string startingNodeName = "Starting Node: ";
        private const string targetNodeName = "Target Node: ";
        private const string bombNodeName = "Bomb Node: ";
        private const string unvisitedNodeName = "Unvisited Node: ";
        private const string visitedNodeName = "Visited Node: ";
        private const string shortestPathNodeName = "Shortest-Path Node: ";

        private const string visualizeName = "Visulize";
        private const string addDropOffName = "Bomb";

        private const string randomMazeName = "Random Maze";
        private const string recursiveMazeNamze = "Recursive Maze";

        public Form1()
        {
            InitializeComponent();

            ToolStripMenuItem[] algorithms =
            {
                new ToolStripMenuItem("Breadth-First Search", null),
                new ToolStripMenuItem("Depth-First Search", null),
                new ToolStripMenuItem("Depth-First Search Smart", null),
                new ToolStripMenuItem("A* Search", null),
                new ToolStripMenuItem("Dijhstra's Algorithm", null),
            };

            ToolStripMenuItem[] mazeGenerators =
            {
                new ToolStripMenuItem("Random Maze", null),
                new ToolStripMenuItem("Recursive Division", null),
            };

            ToolStripMenuItem[] mainItems =
            {
                new ToolStripMenuItem("Algorithms", null, algorithms),
                new ToolStripMenuItem("Maze & Pattern", null, mazeGenerators),
            };

            MenuStrip menu = new MenuStrip();

            foreach (ToolStripMenuItem item in mainItems)
            {
                menu.Items.Add(item);
            }
            
            Controls.Add(menu);

            CreateButton(addDropOffName, 1250, Board.MARGIN + Board.SQUARE + 2, BUTTONWIDTH, BUTTONHEIGHT);

            CreateButton(visualizeName, 1380, Board.MARGIN + Board.SQUARE + 2, BUTTONWIDTH, BUTTONHEIGHT);

            CreateTextLabel(startingNodeName, 2 * Board.MARGIN, 2 * Board.SQUARE + Board.MARGIN, 
                                            LABELWIDTH, LABELHEIGHT);

            CreateTextLabel(targetNodeName, 3 * Board.MARGIN + Board.SQUARE + LABELWIDTH, 
                                            2 * Board.SQUARE + Board.MARGIN, LABELWIDTH, LABELHEIGHT);
            
            CreateTextLabel(bombNodeName, 4 * Board.MARGIN + 2 * Board.SQUARE + 2 * LABELWIDTH, 
                                            2 * Board.SQUARE + Board.MARGIN, LABELWIDTH, LABELHEIGHT);
            
            CreateTextLabel(unvisitedNodeName, 5 * Board.MARGIN + 3 * Board.SQUARE + 3 * LABELWIDTH,
                                            2 * Board.SQUARE + Board.MARGIN, LABELWIDTH, LABELHEIGHT);
            
            CreateTextLabel(visitedNodeName, 7 * Board.MARGIN + 4 * Board.SQUARE + 4 * LABELWIDTH,
                                            2 * Board.SQUARE + Board.MARGIN, LABELWIDTH, LABELHEIGHT);
            
            CreateTextLabel(shortestPathNodeName, 10 * Board.MARGIN + 6 * Board.SQUARE + 5 * LABELWIDTH,
                                            2 * Board.SQUARE + Board.MARGIN, LABELWIDTH, LABELHEIGHT);


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
        private void CreateButton(string name, int posX, int posY, int width, int height)
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


        }



        protected override void OnMouseDown(MouseEventArgs e)
        {
            Position position = new Position((e.X - Board.MARGIN) / Board.SQUARE, (e.Y - Board.MARGIN) / Board.SQUARE);
            
            if (board.InsideTheBoard(e.X, e.Y))
            {
                if(board.IsEmpty(position.x, position.y))
                {
                    board.SetWall(position.x, position.y);
                    Invalidate();
                }
                else if(board.IsWall(position.x, position.y))
                {
                    board.RemoveWall(position.x, position.y);
                    Invalidate();
                }
                isMouseDown = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Position position = new Position((e.X - Board.MARGIN) / Board.SQUARE, (e.Y - Board.MARGIN) / Board.SQUARE);
            
            if (isMouseDown && board.InsideTheBoard(e.X, e.Y))
            {
                if (board.IsEmpty(position.x, position.y) && !(isStartMoving || isEndMoving))
                {
                    board.SetWall(position.x, position.y, false);
                    isWallMoving = true;
                }
                else if (board.IsWall(position.x, position.y) && !(isStartMoving || isEndMoving))
                {
                    board.RemoveWall(position.x, position.y, false);
                }
                else if ((board.IsStartPosition(position.x, position.y) || isStartMoving) && !isWallMoving)
                {
                    board.SetStartPosition(position.x, position.y);
                    isStartMoving = true;
                    
                }
                else if ((board.IsEndPosition(position.x, position.y) || isEndMoving) && !isWallMoving)
                {
                    board.SetEndPosition(position.x, position.y);
                    isEndMoving = true;
                }
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            isMouseDown = isStartMoving = isEndMoving = isWallMoving = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            DrawBoard(g);

            DrawGrid(g);

            DrawToolBoxBorders(g, new Position(7 * BUTTONWIDTH - Board.MARGIN, Board.MARGIN + Board.SQUARE), 
                    new Position(7 * BUTTONWIDTH - Board.MARGIN, Board.MARGIN + Board.SQUARE + Board.TOOLBOXHEIGHT - Board.SQUARE + Board.MARGIN));

            DrawToolBoxItems(g);

            DrawStartPosition(g, board.startPosition.x * Board.SQUARE + Board.MARGIN,
                                       board.startPosition.y * Board.SQUARE + Board.MARGIN);

            DrawEndPosition(g, board.endPosition.x * Board.SQUARE + Board.MARGIN,
                              board.endPosition.y * Board.SQUARE + Board.MARGIN);

            DrawWalls(g);
        }

        private void DrawToolBoxBorders(Graphics g, Position from, Position to)
        {
            g.DrawLine(pen, from.x, from.y, to.x, to.y);
        }

        private void DrawWalls(Graphics g)
        {
            for(int i = 0; i < Board.ROWSIZE; i++)
            {
                for(int j = 0; j < Board.COLUMNSIZE; j++)
                {
                    if(board.grid[i,j] == Cell.WALL)
                    {
                        g.FillRectangle(Brushes.Black, j * Board.SQUARE + Board.MARGIN,
                                        i * Board.SQUARE + Board.MARGIN, 
                                        Board.SQUARE, Board.SQUARE);
                    }
                }
            }
        }

        private void DrawToolBoxItems(Graphics g)
        {
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;

            DrawStartPosition(g, Board.MARGIN + 1 + LABELWIDTH, 2 * Board.SQUARE + 4);
            DrawEndPosition(g, Board.MARGIN + 2 * LABELWIDTH + Board.SQUARE, 2 * Board.SQUARE + 4);

            DrawEllipseRectange(g, 2 * Board.MARGIN + 3 * LABELWIDTH + 2 * Board.SQUARE, 2 * Board.SQUARE + 4);
            DrawUnvisitedNode(g, 2 * Board.MARGIN + 1 + 4 * LABELWIDTH + 4 * Board.SQUARE, 2 * Board.SQUARE + 4);

            DrawVisitedNodes(g, 2 * Board.MARGIN + 5 * LABELWIDTH + 5 * Board.SQUARE, 2 * Board.SQUARE + 4);
            DrawVisitedNodes(g, 3 * Board.MARGIN + 5 * LABELWIDTH + 6 * Board.SQUARE, 2 * Board.SQUARE + 4, isFirstDestination:false);

            DrawShortestPathNode(g, 4 * Board.MARGIN + 6 * LABELWIDTH + 9 * Board.SQUARE, 2 * Board.SQUARE + 4);
        }

        private void DrawShortestPathNode(Graphics g, int posX, int posY)
        {
            Rectangle path = new Rectangle(posX, posY, Board.SQUARE, Board.SQUARE);
            g.FillRectangle(Brushes.Yellow, path);
        }

        private void DrawVisitedNodes(Graphics g, int posX, int PosY, bool isFirstDestination = true)
        {
            Rectangle visited = new Rectangle(posX, PosY, Board.SQUARE, Board.SQUARE);
            if (isFirstDestination)
            {
                g.FillRectangle(Brushes.Aqua, visited);
            }
            else
            {
                g.FillRectangle(Brushes.MediumOrchid, visited);
            }
        }

        private void DrawUnvisitedNode(Graphics g, int posX, int posY)
        {
            pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;

            Rectangle unvisitedNode = new Rectangle(posX, posY, Board.SQUARE, Board.SQUARE);
            
            g.DrawRectangle(pen, unvisitedNode);
        }

        private void DrawEllipseRectange(Graphics g, int posX, int posY)
        {
            Pen extraPen = new Pen(Brushes.Purple, 3);
            Pen extraInnerPen = new Pen(Brushes.Purple, 4);

            Rectangle circle = new Rectangle(posX - 1, posY - 1, Board.SQUARE - 1, Board.SQUARE - 1);
            Rectangle innerCircle = new Rectangle(posX + Board.MARGIN + 1, posY + Board.MARGIN + 1, Board.MARGIN, Board.MARGIN);
            
            g.DrawEllipse(extraPen, circle);
            g.DrawEllipse(extraInnerPen, innerCircle);
        }

        private void DrawStartPosition(Graphics g, int posX, int posY)
        {
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;

            Rectangle startRectangle = new Rectangle(posX, posY, Board.SQUARE, Board.SQUARE);

            g.FillRectangle(Brushes.Aqua, startRectangle);
            g.DrawString("S", font, Brushes.Black, startRectangle, format);
        }

        private void DrawEndPosition(Graphics g, int posX, int posY)
        {
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;

            Rectangle endRectangle = new Rectangle(posX, posY, Board.SQUARE, Board.SQUARE);

            g.FillRectangle(Brushes.Lime, endRectangle);
            g.DrawString("E", font, Brushes.Black, endRectangle, format);
        }

        private void DrawBoard(Graphics g)
        {
            Rectangle boardOutLine = new Rectangle(
                                0, 0,
                                Board.WIDTH,
                                Board.HEIGHT
                                );

            Rectangle toolBoxOutLine = new Rectangle( 
                                Board.MARGIN, Board.MARGIN + Board.SQUARE, 
                                Board.TOOLBOXWIDTH,
                                Board.TOOLBOXHEIGHT - Board.SQUARE + Board.MARGIN
                                );

            Rectangle gridOutLine = new Rectangle(
                                Board.MARGIN,
                                Board.TOOLBOXHEIGHT + 3 * Board.MARGIN,
                                Board.TOOLBOXWIDTH,
                                Board.HEIGHT - (Board.TOOLBOXHEIGHT + 4 * Board.MARGIN)
                                );

            g.DrawRectangle(pen, boardOutLine);
            g.DrawRectangle(pen, toolBoxOutLine);
            g.DrawRectangle(pen, gridOutLine);
       
        }

        private void DrawGrid(Graphics g)
        {
            for (int x = 5; x <= Board.SQUARE * Board.COLUMNSIZE; x += Board.SQUARE)
            {
                g.DrawLine(pen, x, (Board.TOOLBOXHEIGHT + 3 * Board.MARGIN), x, Board.SQUARE * Board.ROWSIZE + Board.MARGIN);
            }
            for (int y = (Board.TOOLBOXHEIGHT + 3 * Board.MARGIN); y <= Board.SQUARE * Board.ROWSIZE; y += Board.SQUARE)
            {
                g.DrawLine(pen, 5, y, Board.SQUARE * Board.COLUMNSIZE + Board.MARGIN, y);
            }
        }
    }
}
