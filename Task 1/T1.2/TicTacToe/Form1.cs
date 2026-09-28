using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TicTacToe
{
    public partial class Form1 : Form
    {
        private readonly Button[] cells = new Button[9];
        private readonly Label statusLabel = new Label();
        private readonly Label scoreLabel = new Label();
        private readonly CheckBox playAsO = new CheckBox();
        private readonly Random random = new Random();

        // X always moves first. By default the player is X; the toggle makes the player O,
        // which means the computer becomes X and starts the game.
        private string playerMark = "X";
        private string computerMark = "O";
        private string currentMark = "X";

        private bool gameOver;
        private int playerWins, computerWins, draws;

        private static readonly int[][] Lines =
        {
            new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 },   // rows
            new[] { 0, 3, 6 }, new[] { 1, 4, 7 }, new[] { 2, 5, 8 },   // columns
            new[] { 0, 4, 8 }, new[] { 2, 4, 6 }                       // diagonals
        };

        public Form1()
        {
            InitializeComponent();
            BuildUi();
            StartNewGame();
        }

        // ---------- UI building ----------

        private void BuildUi()
        {
            // Form properties
            Text = "Tic Tac Toe";
            ClientSize = new Size(380, 505);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            // Status label
            statusLabel.SetBounds(30, 15, 320, 35);
            statusLabel.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(statusLabel);

            // Cells creation and properties
            for (int i = 0; i < 9; i++)
            {
                var cell = new Button();
                cell.SetBounds(30 + (i % 3) * 110, 60 + (i / 3) * 110, 100, 100);
                cell.Font = new Font("Segoe UI", 40f, FontStyle.Bold);
                cell.FlatStyle = FlatStyle.Flat;
                cell.BackColor = Color.White;
                cell.Click += Cell_Click;
                cells[i] = cell;
                Controls.Add(cell);
            }

            // Score label
            scoreLabel.SetBounds(30, 395, 320, 25);
            scoreLabel.Font = new Font("Segoe UI", 11f);
            scoreLabel.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(scoreLabel);

            // Play as O checkbox
            playAsO.Text = "Play as O (computer moves first)";
            playAsO.SetBounds(30, 425, 320, 24);
            playAsO.CheckedChanged += playAsO_CheckedChanged;
            Controls.Add(playAsO);

            // New game button
            var newGameButton = new Button();
            newGameButton.Text = "New Game";
            newGameButton.SetBounds(30, 458, 150, 34);
            newGameButton.Click += (s, e) => StartNewGame();
            Controls.Add(newGameButton);

            // Reset scores button
            var resetButton = new Button();
            resetButton.Text = "Reset Scores";
            resetButton.SetBounds(200, 458, 150, 34);
            resetButton.Click += (s, e) =>
            {
                playerWins = computerWins = draws = 0;
                StartNewGame();
            };
            Controls.Add(resetButton);
        }

        // ---------- Game logic ----------

        private void StartNewGame()
        {
            foreach (Button cell in cells)
            {
                cell.Text = string.Empty;
                cell.BackColor = Color.White;
            }

            currentMark = "X";
            gameOver = false;
            UpdateStatus();
            UpdateScore();
            LetComputerPlayIfItsTurn();   // when the player is O, the computer opens as X
        }

        private void playAsO_CheckedChanged(object sender, EventArgs e)
        {
            playerMark = playAsO.Checked ? "O" : "X";
            computerMark = playAsO.Checked ? "X" : "O";

            if (AnyCellTaken())
            {
                StartNewGame();           // game in progress (or finished) -> restart it
            }
            else
            {
                UpdateStatus();           // empty board -> nothing to restart
                LetComputerPlayIfItsTurn();
            }
        }

        private void Cell_Click(object sender, EventArgs e)
        {
            var cell = (Button)sender;
            if (gameOver || currentMark != playerMark || cell.Text.Length > 0)
            {
                return;
            }

            PlaceMark(cell);
            LetComputerPlayIfItsTurn();
        }

        private void LetComputerPlayIfItsTurn()
        {
            if (!gameOver && currentMark == computerMark)
            {
                ComputerMove();
            }
        }

        private void PlaceMark(Button cell)
        {
            cell.Text = currentMark;
            cell.ForeColor = currentMark == "X" ? Color.SteelBlue : Color.Firebrick;

            int[] winningLine = GetWinningLine();
            if (winningLine != null) // game over with a winner
            {
                foreach (int i in winningLine)
                {
                    cells[i].BackColor = Color.LightGreen;
                }

                if (currentMark == playerMark) { playerWins++; } else { computerWins++; }
                statusLabel.Text = currentMark == playerMark ? "You win!" : "Computer wins!";
                EndGame();
            }
            else if (IsBoardFull()) // game over with a draw
            {
                draws++;
                statusLabel.Text = "It's a draw!";
                EndGame();
            }
            else // game continues
            {
                currentMark = currentMark == "X" ? "O" : "X";
                UpdateStatus();
            }
        }

        private void EndGame()
        {
            gameOver = true;
            UpdateScore();
        }

        private void UpdateStatus()
        {
            statusLabel.Text = currentMark == playerMark
                ? "Your turn (" + playerMark + ")"
                : "Computer's turn (" + computerMark + ")";
        }

        private void UpdateScore()
        {
            scoreLabel.Text = "You: " + playerWins + "     Computer: " + computerWins + "     Draws: " + draws;
        }

        // ---------- Board checks ----------

        private int[] GetWinningLine()
        {
            foreach (int[] line in Lines)
            {
                string first = cells[line[0]].Text;
                if (first.Length > 0 && first == cells[line[1]].Text && first == cells[line[2]].Text)
                {
                    return line;
                }
            }
            return null;
        }

        private bool IsBoardFull()
        {
            foreach (Button cell in cells)
            {
                if (cell.Text.Length == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private bool AnyCellTaken()
        {
            foreach (Button cell in cells)
            {
                if (cell.Text.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        // ---------- Computer opponent ----------
        // Empty board: open on a random cell.
        // Otherwise priorize win -> block -> centre -> random free cell.

        private void ComputerMove()
        {
            int move = AnyCellTaken() ? -1 : random.Next(9);
            if (move < 0) { move = FindCompletingMove(computerMark); }
            if (move < 0) { move = FindCompletingMove(playerMark); }
            if (move < 0 && cells[4].Text.Length == 0) { move = 4; }
            if (move < 0) { move = PickRandomFree(new[] { 0, 1, 2, 3, 5, 6, 7, 8 }); }

            PlaceMark(cells[move]);
        }

        // Returns the empty cell that would give 'mark' three in a row, or -1.
        private int FindCompletingMove(string mark)
        {
            foreach (int[] line in Lines)
            {
                int marks = 0;
                int empty = -1;
                foreach (int i in line)
                {
                    if (cells[i].Text == mark) { marks++; }
                    else if (cells[i].Text.Length == 0) { empty = i; }
                }

                if (marks == 2 && empty >= 0)
                {
                    return empty;
                }
            }
            return -1;
        }

        // Returns a random free cell from the given candidates, or -1 if none are free.
        private int PickRandomFree(int[] candidates)
        {
            var free = new List<int>();
            foreach (int i in candidates)
            {
                if (cells[i].Text.Length == 0) { free.Add(i); }
            }
            return free.Count == 0 ? -1 : free[random.Next(free.Count)];
        }
    }
}