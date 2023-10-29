using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Tetris
{
    public partial class MainWindow : Window
    {
        private readonly ImageSource[] tileImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Assets/TileEmpty.Png",UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileCyan.Png",UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileBlue.Png",UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileOrange.Png",UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileYellow.Png",UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileGreen.Png",UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TilePurple.Png",UriKind.Relative)),
            new BitmapImage(new Uri("Assets/TileRed.Png",UriKind.Relative))
        };

        private readonly ImageSource[] BlockImages = new ImageSource[]
        {
            new BitmapImage(new Uri("Assets/Block-Empty.Png",UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-I.Png",UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-J.Png",UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-L.Png",UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-O.Png",UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-S.Png",UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-T.Png",UriKind.Relative)),
            new BitmapImage(new Uri("Assets/Block-Z.Png",UriKind.Relative))
        };

        private readonly Image[,] imageControls;
        private readonly int[] maxMinDecreaseDelay = { 500, 100, 50 };

        private GameState gameState = new GameState();

        public MainWindow()
        {
            InitializeComponent();
            imageControls = SetupGameCanvas(gameState.GameGrid);
        }

        private Image[,] SetupGameCanvas(GameGrid grid)
        {
            Image[,] imageControls = new Image[grid.Rows, grid.Columns];
            int cellSize = 25;

            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    Image imageControl = new Image
                    {
                        Width = cellSize,
                        Height = cellSize
                    };

                    Canvas.SetTop(imageControl, (r - 2) * cellSize + 10);
                    Canvas.SetLeft(imageControl, c * cellSize);
                    GameCanvas.Children.Add(imageControl);
                    imageControls[r,c] = imageControl;
                }
            }

            return imageControls;
        }

        private void DrawGrid(GameGrid grid)
        {
            for (int r = 0; r < grid.Rows; r++)
            {
                for (int c = 0; c < grid.Columns; c++)
                {
                    int id = grid[r, c];
                    imageControls[r, c].Opacity = 1;
                    imageControls[r, c].Source = tileImages[id];
                }
            }
        }

        private void DrawBlock(Block.Block block)
        {
            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row, p.Column].Opacity = 1;
                imageControls[p.Row, p.Column].Source = tileImages[block.Id];
            }
        }

        private void DrawNextBlock(Block.BlockQueue blockQueue)
        {
            Block.Block nextBlock = blockQueue.NextBlock;
            NextImage.Source = BlockImages[nextBlock.Id];
        }

        private void DrawHeldBlock(Block.Block heldBlock)
        {
            if (heldBlock == null)
            {
                HoldImage.Source = BlockImages[0];
            }
            else
            {
                HoldImage.Source = BlockImages[heldBlock.Id];
            }
        }

        private void DrawGhostBlock(Block.Block block)
        {
            int dropDistance = gameState.BlockDropDistance();

            foreach (Position p in block.TilePositions())
            {
                imageControls[p.Row + dropDistance, p.Column].Opacity = 0.25;
                imageControls[p.Row + dropDistance, p.Column].Source = tileImages[block.Id];
            }
        }

        private void Draw(GameState gameState)
        {
            DrawGrid(gameState.GameGrid);
            DrawGhostBlock(gameState.CurrentBlock);
            DrawBlock(gameState.CurrentBlock);
            DrawNextBlock(gameState.BlockQueue);
            DrawHeldBlock(gameState.HeldBlock);
            ScoreText.Text = $"Score: {gameState.Score}";
            LevelText.Text = $"Level: {gameState.CurrentLevel()}";
        }

        private async void BtnPlayAgain_Click(object sender, RoutedEventArgs e)
        {
            gameState = new GameState();
            GameoverMenu.Visibility = Visibility.Hidden;
            await GameLoop();
        }

        private async Task GameLoop()
        {
            Draw(gameState);

            while (!gameState.GameOver)
            {
                int delay = Math.Max(maxMinDecreaseDelay[1], maxMinDecreaseDelay[0] - maxMinDecreaseDelay[2] * gameState.CurrentLevel());
                await Task.Delay(delay);
                gameState.MoveBlockDown();
                Draw(gameState);
            }

            GameoverMenu.Visibility = Visibility.Visible;
            FinalScoreText.Text = $"Score: {gameState.Score}";
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!gameState.GameOver)
            {
                switch (e.Key)
                {
                    case Key.Left:
                        gameState.MoveBlockLeft();
                        break;
                    case Key.Right:
                        gameState.MoveBlockRight();
                        break;
                    case Key.Down:
                        gameState.MoveBlockDown();
                        break;
                    case Key.Up:
                        gameState.RotateBlockCW();
                        break;
                    case Key.Z:
                        gameState.RotateBlockCCW();
                        break;
                    case Key.C:
                        gameState.HoldBlock();
                        break;
                    case Key.Space:
                        gameState.DropBlock();
                        break;
                    default:
                        return;
                }
            }

            Draw(gameState);
        }

        private async void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            await GameLoop();
        }
    }
}
