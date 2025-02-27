using System;
using System.Windows;
using System.Windows.Controls;

namespace Chess
{
    public partial class MainWindow : Window
    {
        private const string defaultMode = "local";
        private const string defaultFENPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private const int defaultBotDifficulty = 4;

        private const double boardHorizontalMarginPercent = 0.5;
        private const int minBoardSize = 120;
        private const int maxBoardSize = 640;

        private readonly ChessBoard mainBoard;
        private readonly GameManager mainGameManager;

        private void ResetSettingsMenu()
        {
            cb_GameMode.SelectedIndex = 0;
            ShowMenuGameModeSettings(defaultMode);

            cb_PlayerSide.SelectedIndex = 0;

            lb_difficultyLabel.Content = defaultBotDifficulty.ToString();
            sld_DifficultySlider.Value = defaultBotDifficulty;

            tb_StartPosition.Text = defaultFENPosition;
        }

        private void ShowMenuGameModeSettings(string mode)
        {
            if (mode == "local")
            {
                sp_ColorPanel.Visibility = Visibility.Collapsed;
                sp_DifficultyPanel.Visibility = Visibility.Collapsed;
            }
            else if (mode == "AI")
            {
                sp_ColorPanel.Visibility = Visibility.Visible;
                sp_DifficultyPanel.Visibility = Visibility.Visible;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            ResetSettingsMenu();

            mainBoard = new(this, cnv_MainCanvas, 320);
            mainGameManager = new(mainBoard);

            // "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1" -- starting position
            // "k7/8/8/8/8/8/8/QK6 b - - 0 30" -- check test
            // "k7/7P/8/8/8/8/8/QK6 b - - 0 30" -- pawn succession test
        }

        private void cb_GameMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedMode = (string)(cb_GameMode.SelectedValue as ComboBoxItem).Tag;
            ShowMenuGameModeSettings(selectedMode);
        }

        private void sld_DifficultySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lb_difficultyLabel.Content = ((int)e.NewValue).ToString();
        }

        private void btn_PastePosition_Click(object sender, RoutedEventArgs e)
        {
            tb_StartPosition.Text = Clipboard.GetText().Trim();
        }

        private void btn_CopyPosition_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tb_StartPosition.Text.Trim());
        }

        private void btn_StartGame_Click(object sender, RoutedEventArgs e)
        {
            string selectedMode = (string)(cb_GameMode.SelectedValue as ComboBoxItem).Tag;

            try
            {
                mainGameManager.LoadFEN(tb_StartPosition.Text);

            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Podano nieprawdiłową pozycję startową", "Błąd",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            sp_GameSettings.Visibility = Visibility.Collapsed;

            if (selectedMode == "local")
            {
                mainGameManager.StartLocalGame();
            }
            else if (selectedMode == "AI")
            {
                int difficulty = int.Parse((string)lb_difficultyLabel.Content);
                string playerSide = (string)(cb_PlayerSide.SelectedValue as ComboBoxItem).Tag;

                mainGameManager.StartGameAgainstBot(difficulty, playerSide == "white");
            }

            cnv_MainCanvasWrapper.Visibility = Visibility.Visible;
        }

        private void cnv_MainCanvasWrapper_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double calculatedSideMargin = boardHorizontalMarginPercent * mainBoard.BoardSize / 2;
            double freeSpace = e.NewSize.Width - mainBoard.BoardSize;

            int newBoardSize = (int)(mainBoard.BoardSize + freeSpace - calculatedSideMargin);

            newBoardSize = Math.Clamp(newBoardSize, minBoardSize, maxBoardSize);
            if (mainBoard.BoardSize != newBoardSize && newBoardSize % 8 == 0)
                mainBoard.Resize(newBoardSize);

            Canvas.SetLeft(cnv_MainCanvas, freeSpace / 2);
        }
    }
}
