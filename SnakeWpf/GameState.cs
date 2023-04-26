using System;
using System.Collections.Generic;

namespace SnakeWpf
{
    public class GameState
    {
        public int Rows { get; }
        public int Columns { get; }
        public GridValue[,] Grid { get; }
        public Direction Dir { get; private set; }
        public int Score { get; private set; }
        public bool GameOver { get; private set; }

        private readonly LinkedList<Direction> directionChanges = new LinkedList<Direction>();
        private readonly LinkedList<Position> snakePositions = new LinkedList<Position>();

        private Random random = new Random();

        public GameState(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Grid = new GridValue[rows, columns];
            Dir = Direction.Right;

            AddSnake();
            AddFood();
        }

        private void AddSnake()
        {
            int r = Rows / 2;
            for(int c = 1; c <= 3; c++)
            {
                Grid[r, c] = GridValue.Snake;
                snakePositions.AddFirst(new Position(r, c));
            }
        }

        private IEnumerable<Position> EmptyPosition()
        {
            for (int r = 0; r < Rows; r++)
            {
                for(int c =0; c < Columns; c++)
                {
                    if (Grid[r, c] == GridValue.Empty)
                    {
                        yield return new Position(r, c);
                    }
                }
            }
        }

        private void AddFood()
        {
            List<Position> empty = new List<Position>(EmptyPosition());
            if (empty.Count == 0) return;
            Position pos = empty[random.Next(empty.Count)];
            Grid[pos.Row, pos.Column] = GridValue.Food;
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

        private void AddHead(Position pos)
        {
            snakePositions.AddFirst(pos);
            Grid[pos.Row, pos.Column] = GridValue.Snake;
        }

        private void RemoveTail()
        {
            Position tail = snakePositions.Last.Value;
            Grid[tail.Row, tail.Column] = GridValue.Empty;
            snakePositions.RemoveLast();
        }

        private Direction GetLastDirection()
        {
            Direction direction;
            if (directionChanges.Count == 0)
            {
                direction = Dir;
            }
            else
            {
                direction = directionChanges.Last.Value;
            }
            return direction;
        }

        private bool CanChangeDirection(Direction newDir)
        {
            var res = false;
            var lastDirection = GetLastDirection();
            if (directionChanges.Count < 2)
            {
                res = newDir != lastDirection 
                   && newDir != lastDirection.Opposite();
            }
            return res;
        }

        public void ChangeDirection(Direction dir)
        {
            if (CanChangeDirection(dir))
            {
                directionChanges.AddLast(dir);
            }
        }

        private bool OutsideGrid(Position position)
        {
            return position.Row < 0 
                || position.Row >= Rows 
                || position.Column < 0 
                || position.Column >= Columns;
        } 

        private GridValue WillHit(Position newHeadPosition)
        {
            var gridValue = GridValue.Empty;
            if (OutsideGrid(newHeadPosition))
            {
                gridValue = GridValue.Outside;
            }else if (newHeadPosition != TailPosition())
            {
                gridValue = Grid[newHeadPosition.Row, newHeadPosition.Column];
            }
            return gridValue;
        }

        public void Move()
        {
            if (directionChanges.Count > 0)
            {
                Dir = directionChanges.First.Value;
                directionChanges.RemoveFirst();
            }

            Position newHeadPosition = HeadPosition().Translate(Dir);
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
                Score++;
                AddFood();
            }
        }

    }
}
