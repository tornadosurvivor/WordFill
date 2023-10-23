using System.Data;
using System.Text.RegularExpressions;

namespace WordFill
{
    public class Solution
    {
        private char[,] puzzle;
        private Dictionary<int, List<string>> words = new Dictionary<int, List<string>>();
        private List<WordStart> wordStartList = new List<WordStart>();
        private Stack<PencilMark> history = new Stack<PencilMark>();

        public Solution(char[,] puzzle, string[] words) 
        {
            this.puzzle = puzzle;
            
            foreach (string word in words)
            {
                if (!this.words.ContainsKey(word.Length))
                    this.words.Add(word.Length, new List<string> { word });

                else
                {
                    this.words[word.Length].Add(word);
                }
            }

            wordStartList.AddRange(CollectHorizontalWordSpaces());
            wordStartList.AddRange(CollectVerticalWordSpaces());

            wordStartList = SmartSortWordStartList();
        }

        private List<WordStart> SmartSortWordStartList()
        {
            List<WordStart> betterWordStarts = new List<WordStart>();
            List<WordStart> wordStarts = new List<WordStart>(wordStartList);
            Queue<WordStart> queue =new Queue<WordStart>();

            queue.Enqueue(wordStarts.First());            
            wordStarts.RemoveAt(0);

            while (queue.Count > 0) 
            {
                WordStart wordStart = queue.Dequeue();
                betterWordStarts.Add(wordStart);
                foreach (WordStart otherWordStart in wordStarts.ToList()) 
                { 
                    if (IsIntersect(wordStart, otherWordStart))
                    {
                        queue.Enqueue(otherWordStart);
                        wordStarts.Remove(otherWordStart);
                    }
                }
                
            }

            return betterWordStarts;
        }

        private bool IsIntersect(WordStart wordStart, WordStart otherWordStart)
        {
            HashSet<PencilMark> spaces = new HashSet<PencilMark>();
            if (wordStart.direction==Direction.Across)
            {
                for (int i = 0; i < wordStart.length; ++i)
                {
                    spaces.Add(new PencilMark()
                    {
                        row = wordStart.row,
                        col = wordStart.col + i,
                    });
                }
            }
            else
            {
                for (int i = 0; i < wordStart.length; ++i)
                {
                    spaces.Add(new PencilMark()
                    {
                        row = wordStart.row + i,
                        col = wordStart.col,
                    });
                }
            }

            HashSet<PencilMark> otherSpaces = new HashSet<PencilMark>();

            if (otherWordStart.direction == Direction.Across)
            {
                for (int i = 0; i < otherWordStart.length; ++i)
                {
                    otherSpaces.Add(new PencilMark()
                    {
                        row = otherWordStart.row,
                        col = otherWordStart.col + i,
                    });
                }
            }
            else
            {
                for (int i = 0; i < otherWordStart.length; ++i)
                {
                    otherSpaces.Add(new PencilMark()
                    {
                        row = otherWordStart.row + i,
                        col = otherWordStart.col,
                    });
                }
            }

            return spaces.Intersect(otherSpaces).Count() > 0;
        }

        public void Solve()
        {
            bool result = FillGrid(0);
            if (result)
                PrintPuzzleState(puzzle);
            else
                Console.WriteLine("Puzzle is unsolvable");
        }

        private bool FillGrid(int wordStartListIndex)
        {
            //PrintPuzzleState(puzzle);

            if (wordStartListIndex == wordStartList.Count)
            {
                // Puzzle is solved
                return true;
            }

            WordStart target = wordStartList[wordStartListIndex];
            string[] candidates = words[target.length].ToArray();
             
            foreach (string candidate in candidates)
            {
                int moves = 0;
                if (CanFillBlankWithWord(target, candidate))
                {
                    moves = PencilIn(target, candidate);
                    if (FillGrid(wordStartListIndex + 1))
                    {
                        return true;
                    }
                    else
                    {
                        UndoPuzzle(moves, candidate);
                    }
                }                
            }

            return false;
        }

        private void UndoPuzzle(int moves, string word)
        {
            for (int i = 0; i < moves; i++)
            {
                PencilMark moveToUndo = history.Pop();
                puzzle[moveToUndo.row, moveToUndo.col] = ' ';
            }

            words[word.Length].Add(word);
        }

        private int PencilIn(WordStart space, string word)
        {
            int moves = 0;

            if (space.direction == Direction.Across)
            {
                for (int i = 0; i < word.Length; ++i)
                {
                    if (puzzle[space.row, space.col + i] == ' ')
                    {
                        puzzle[space.row, space.col + i] = word[i];
                        history.Push(new PencilMark
                        {
                            row = space.row,
                            col = space.col + i
                        });
                        ++moves;
                    }
                }
            }
            else //(space.direction == Direction.Down)
            {
                for (int i = 0; i < word.Length; ++i)
                {
                    if (puzzle[space.row + i, space.col] == ' ')
                    {
                        puzzle[space.row + i, space.col] = word[i];
                        history.Push(new PencilMark
                        {
                            row = space.row + i,
                            col = space.col
                        });
                        ++moves;
                    }
                }
            }

            words[word.Length].Remove(word);

            return moves;
        }

        private bool CanFillBlankWithWord(WordStart space, string word)
        {
            if (space.direction == Direction.Across)
            {
                for (int i = 0; i < word.Length; ++i)
                {
                    if (puzzle[space.row, space.col + i] != ' ' && puzzle[space.row, space.col + i] != word[i])
                        return false;
                }

                return true;
            }
            else //(space.direction == Direction.Down)
            {
                for (int i = 0; i < word.Length; ++i)
                {
                    if (puzzle[space.row + i, space.col] != ' ' && puzzle[space.row + i, space.col] != word[i])
                        return false;                   
                }

                return true;
            }
        }


        private List<WordStart> CollectHorizontalWordSpaces()
        {
            List<WordStart> horizontalWordStarts = new List<WordStart>();

            for (int rowNumber = 0; rowNumber < puzzle.GetLength(0); rowNumber++)
            {
                string slice = GetHorizontalSliceAsString(puzzle, rowNumber);
                Regex regex = new Regex(@" {2,}");
                var matches = regex.Matches(slice);

                foreach (Match match in matches)
                {
                    horizontalWordStarts.Add(new WordStart
                    {
                        row = rowNumber,
                        col = match.Index,
                        length = match.Length,
                        direction = Direction.Across
                    });
                }
            }

            return horizontalWordStarts;
        }

        private List<WordStart> CollectVerticalWordSpaces()
        {
            List<WordStart> verticalWordStarts = new List<WordStart>();

            for (int colNumber = 0; colNumber < puzzle.GetLength(1); colNumber++)
            {
                string slice = GetVerticalSliceAsString(puzzle, colNumber);
                Regex regex = new Regex(@" {2,}");
                var matches = regex.Matches(slice);

                foreach (Match match in matches)
                {
                    verticalWordStarts.Add(new WordStart
                    {
                        row = match.Index,
                        col = colNumber,
                        length = match.Length,
                        direction = Direction.Down
                    }) ;
                }
            }

            return verticalWordStarts;
        }

        private string GetHorizontalSliceAsString(char[,] grid, int rowNumber)
        {
            string result = "";

            for (int c = 0; c < grid.GetLength(1); c++)
            {
                result += grid[rowNumber, c];
            }

            return result;
        }

        private string GetVerticalSliceAsString(char[,] grid, int colNumber)
        {
            string result = "";

            for (int r = 0; r < grid.GetLength(0); r++)
            {
                result += grid[r, colNumber];
            }

            return result;
        }

        private void PrintPuzzleState(char[,] grid) 
        {
            Thread.Sleep(25);
            Console.Clear();
            Console.WriteLine();
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    Console.Write(grid[i, j]);
                }
                Console.WriteLine();
            }
        }

        private struct PencilMark
        {
            public int row;
            public int col;
        }

        private struct WordStart
        {
            public int row;
            public int col;
            public int length;
            public Direction direction;
        }

        private enum Direction
        {
            Across,
            Down
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            string[] dimensionsInput = Console.ReadLine().Split(' ');

            int H = int.Parse(dimensionsInput[0]);
            int W = int.Parse(dimensionsInput[1]);

            char[,] puzzle = new char[H,W];

            for (int i=0; i<H; i++)
            {
                string row = Console.ReadLine();
                for (int j=0; j<W; j++)
                {
                    puzzle[i, j] = row[j] == 'o' ? ' ' : '█';
                }
            }

            int numWords = int.Parse(Console.ReadLine());
            string[] words = new string[numWords];
            for (int i=0; i<numWords ; i++)
            {
                words[i] = Console.ReadLine();
            }

            Solution solution = new Solution(puzzle, words);
            solution.Solve();
        }
    }
}