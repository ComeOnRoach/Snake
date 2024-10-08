﻿
using System.Data.Common;
using System.Windows.Controls.Primitives;

namespace Snake
{
    public class GameState
    {
        public int Rows { get; }
        public int Columns { get; }
        public GridValue[,] Grid { get; }
        public Direction Direction { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }

        private readonly LinkedList<Direction> dirChanges = new LinkedList<Direction>();
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();
        private readonly Random random = new Random();

        public GameState(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Grid = new GridValue[rows, columns];
            Direction = Direction.Right;
            AddSnake();
            AddFood();
        }

        private void AddSnake()
        {
            int row = Rows / 2;
            for (int column = 1; column <= 3; column++)
            {
                Grid[row, column] = GridValue.Snake;
                snakePositions.AddFirst(new Position(row, column));
            }
        }

        private IEnumerable<Position> EmptyPositions()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    if (Grid[row, column] == GridValue.Empty)
                    {
                        yield return new Position(row, column);

                    }
                }
            }
        }

        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPositions());
            if (empty.Count == 0)
            {
                return;
            }
            Position position = empty[random.Next(empty.Count)];
            Grid[position.Row, position.Column] = GridValue.Food;
        }

        public Position HeadPosition()
        {
            return snakePositions.First.Value;
        }

        public Position TailPosition()
        {
            return snakePositions.Last.Value;
        }

        public IEnumerable<Position> SnakePositions()
        {
            return snakePositions;
        }

        private void AddHead(Position position)
        {
            snakePositions.AddFirst(position);
            Grid[position.Row, position.Column] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            Position tail = snakePositions.Last.Value;
            Grid[tail.Row, tail.Column] = GridValue.Empty;
            snakePositions.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            if (dirChanges.Count == 0)
            {
                return Direction;
            }

            return dirChanges.Last.Value;
        }

        private bool CanChangeDirectoin(Direction newDir)
        {
            if (dirChanges.Count == 2)
            {
                return false;
            }

            Direction lastDir = GetLastDirection();

            return lastDir != newDir && newDir != lastDir.Opposite(); 
        }

        public void ChangeDirection(Direction direction)
        {
            if (CanChangeDirectoin(direction))
            {
                dirChanges.AddLast(direction);
            }

        }

        private bool OutsideGrid(Position position)
        {
            return position.Row < 0 || position.Row >= Rows || position.Column < 0 || position.Column >= Columns;
        }

        private GridValue WillHit(Position newHeadPosition)
        {
            if (OutsideGrid(newHeadPosition))
            {
                return GridValue.Outside;
            }

            if (newHeadPosition == TailPosition())
            {
                return GridValue.Empty;
            }
            return Grid[newHeadPosition.Row, newHeadPosition.Column];
        }

        public void Move()
        {
            if (dirChanges.Count > 0)
            {
                Direction = dirChanges.First.Value;
                dirChanges.RemoveFirst();
            }

            Position newHeadPosition = HeadPosition().Translate(Direction);
            GridValue hit = WillHit(newHeadPosition);
            if (hit == GridValue.Outside || hit == GridValue.Snake)
            {
                GameOver = true;
            }
            else if (hit == GridValue.Empty)
            {
                RemoveTail();
                AddHead(newHeadPosition);
            }
            else if (hit == GridValue.Food)
            {
                AddHead(newHeadPosition);
                Score += 100;
                AddFood();
            }

        }
    }
}
