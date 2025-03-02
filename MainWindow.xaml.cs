/*
Zadanie zaliczeniowe z c#
Imię i nazwisko ucznia: Filip Gronkowski
Data wykonania zadania: 17.02.2025 - 04.03.2025
Treść zadania: 'Szachy'
Opis funkcjonalności aplikacji: 
    Aplikacja umożliwia grę w szachy z zachowaniem wszystkich zasad gry.
    Przed rozpoczęciem gry można ją skonfigurować. Dostępne parametry to:
        - tryb gry (gra lokalna i przeciwko AI),
        - pozycja startowa (w formacie FEN) oraz jej kopiowanie/wklejanie,
        - po wybraniu trybu 'przeciwko AI':
            * kolor gracza,
            * trudność AI od 4 do 16 (wyznaczająca głębokość liczenia silnika).
    Po rozpoczęciu gry pokazuje się szachownica (skalująca się wraz z rozmiarami okna),
    oraz przyciski, umożliwiające skopiowanie pozycji, obrócenie szachownicy i powrót do lobby.
*/


using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Chess
{
    /// <summary>
    /// Okno główne aplikacji
    /// </summary>
    public partial class MainWindow : Window
    {
        // kolor obecnie wykonującego ruch gracza
        private static readonly SolidColorBrush currentPlayerColor = new(Colors.LightGray);

        private const string defaultMode = "local";
        private const string defaultFENPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        private const string playerLabel = "Gracz";
        private const string botLabel = "Bot";
        private const double boardHorizontalMarginPercent = 0.5;
        private const int defaultBotDifficulty = 4;
        private const int minBoardSize = 120;
        private const int maxBoardSize = 2000;

        private readonly ChessBoard mainBoard;
        private readonly GameManager mainGameManager;

        private DockPanel WhitePlayerPanel;
        private DockPanel BlackPlayerPanel;
        private bool isWhiteToMove;

        /// <summary>
        /// Konstruktor główny
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            ResetSettingsMenu();

            mainBoard = new(this, cnv_MainCanvas, 600)
            {
                PiecesUpdateHandler = OnBoardUpdate
            };

            mainGameManager = new(mainBoard)
            {
                GameOverHandler = OnGameOver
            };
        }

        /// <summary>
        /// ucina resztę z dzielenia od liczby
        /// </summary>
        /// <param name="val">dzielna</param>
        /// <param name="div">dzielnik</param>
        /// <returns>liczba bez reszty</returns>
        private static int CutRemainder(int val, int div)
        {
            return val - (val % div);
        }

        /// <summary>
        /// aktualizuje stan panelów z informacjami o graczach
        /// </summary>
        private void UpdatePlayerPanels()
        {
            if (isWhiteToMove == mainGameManager.IsWhiteToMove)
            {
                (WhitePlayerPanel.Children[0] as Label).Background = mainGameManager.IsWhiteToMove ? currentPlayerColor : null;
                (BlackPlayerPanel.Children[0] as Label).Background = !mainGameManager.IsWhiteToMove ? currentPlayerColor : null;

                isWhiteToMove = !isWhiteToMove;
            }

            // uaktualnienie podglądów róznicy materiału
            static string materialStr(int val) => val > 0 ? "+" + val.ToString() : "";

            (WhitePlayerPanel.Children[1] as Label).Content = materialStr(mainGameManager.MaterialDifference);
            (BlackPlayerPanel.Children[1] as Label).Content = materialStr(-mainGameManager.MaterialDifference);
        }

        /// <summary>
        /// Wywoływane przy aktualizacji stanu wizualnej szachownicy
        /// </summary>
        private void OnBoardUpdate()
        {
            UpdatePlayerPanels();
        }

        /// <summary>
        /// przywraca widok lobby
        /// </summary>
        private void ReturnToLobby()
        {
            cnv_MainCanvasWrapper.Visibility = Visibility.Collapsed;
            p_BottomPlayerPanel.Visibility = Visibility.Collapsed;
            p_TopPlayerPanel.Visibility = Visibility.Collapsed;

            ResetSettingsMenu();
            sp_GameSettings.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Wywoływane przy zakończeniu gry przez <i>mainGameManager</i>
        /// </summary>
        /// <param name="result">rezultat gry</param>
        /// <param name="message">opcjonalna informacja o zakończeniu gry</param>
        private void OnGameOver(GameResult result, string message = "")
        {
            string text;

            switch (result)
            {
                case GameResult.Draw:
                    text = "Remis! " + message;
                    break;
                case GameResult.WhiteWin:
                case GameResult.BlackWin:
                    text = "Mat! " + (result == GameResult.WhiteWin ? "Biały" : "Czarny") + " wygrywa! " + message;
                    break;
                default:
                    text = "Przerwano grę: " + message;
                    // nie wyświetlaj okienka, jeśli nie ma wiadomości
                    if (String.IsNullOrEmpty(message)) return;
                    break;
            }

            MessageBox.Show(text, "Koniec gry", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Pozycjonuje i skaluje szachownicę w zależności od dostępnego miejsca
        /// </summary>
        /// <param name="wrapSize">wielkość opisująca dostępną przestrzeń w oknie</param>
        private void AlignBoardPlacement(Size wrapSize)
        {
            double calculatedSideMargin = boardHorizontalMarginPercent * mainBoard.BoardSize / 2;
            double freeSpace = wrapSize.Width - mainBoard.BoardSize;

            int newBoardSize = (int)(mainBoard.BoardSize + freeSpace - calculatedSideMargin);

            int maxSize = (int)(wrapSize.Height < maxBoardSize ? wrapSize.Height : maxBoardSize);
            if (maxSize < minBoardSize) maxSize = minBoardSize;

            newBoardSize = Math.Clamp(newBoardSize, minBoardSize, maxSize);

            if (mainBoard.BoardSize != newBoardSize)
                mainBoard.Resize(CutRemainder(newBoardSize, 8));

            Canvas.SetLeft(cnv_MainCanvas, freeSpace / 2);
        }

        /// <summary>
        /// inicjalizuje szachownice
        /// </summary>
        private void InitBoard()
        {
            string selectedMode = (string)(cb_GameMode.SelectedValue as ComboBoxItem).Tag;
            string playerSide = (string)(cb_PlayerSide.SelectedValue as ComboBoxItem).Tag;

            mainBoard.Rotation = playerSide == "white" ? BoardRotation.WhiteBottom : BoardRotation.BlackBottom;

            try
            {
                mainGameManager.LoadFEN(tb_StartPosition.Text);
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "Podano nieprawdiłową pozycję startową", "Błąd",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                ReturnToLobby();
                return;
            }

            sp_GameSettings.Visibility = Visibility.Collapsed;
            cnv_MainCanvasWrapper.Visibility = Visibility.Visible;
            isWhiteToMove = mainGameManager.IsWhiteToMove;
            UpdatePlayerPanels();


            if (selectedMode == "local")
            {
                mainGameManager.StartLocalGame();
            }
            else if (selectedMode == "AI")
            {
                int difficulty = int.Parse((string)lb_difficultyLabel.Content);

                mainGameManager.StartGameAgainstBot(difficulty, playerSide == "white");
            }
        }

        /// <summary>
        /// inicjalizuje etykiety graczy
        /// </summary>
        private void InitPlayerLabels()
        {
            string playerSide = (string)(cb_PlayerSide.SelectedValue as ComboBoxItem).Tag;
            string selectedMode = (string)(cb_GameMode.SelectedValue as ComboBoxItem).Tag;

            if (selectedMode == "AI")
            {
                if (playerSide == "white")
                {
                    WhitePlayerPanel = p_BottomPlayerPanel;
                    BlackPlayerPanel = p_TopPlayerPanel;

                    (WhitePlayerPanel.Children[0] as Label).Content = playerLabel;
                    (BlackPlayerPanel.Children[0] as Label).Content = botLabel;
                }
                else
                {
                    WhitePlayerPanel = p_TopPlayerPanel;
                    BlackPlayerPanel = p_BottomPlayerPanel;

                    (WhitePlayerPanel.Children[0] as Label).Content = botLabel;
                    (BlackPlayerPanel.Children[0] as Label).Content = playerLabel;
                }
            }
            else
            {
                WhitePlayerPanel = p_BottomPlayerPanel;
                BlackPlayerPanel = p_TopPlayerPanel;

                (WhitePlayerPanel.Children[0] as Label).Content = playerLabel + " A";
                (BlackPlayerPanel.Children[0] as Label).Content = playerLabel + " B";
            }

            (WhitePlayerPanel.Children[1] as Label).Content = "";
            (WhitePlayerPanel.Children[1] as Label).Content = "";

            WhitePlayerPanel.Visibility = Visibility.Visible;
            BlackPlayerPanel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// resetuje menu ustawień
        /// </summary>
        private void ResetSettingsMenu()
        {
            cb_GameMode.SelectedIndex = 0;
            ShowMenuGameModeSettings(defaultMode);

            cb_PlayerSide.SelectedIndex = 0;

            lb_difficultyLabel.Content = defaultBotDifficulty.ToString();
            sld_DifficultySlider.Value = defaultBotDifficulty;

            tb_StartPosition.Text = defaultFENPosition;
        }

        /// <summary>
        /// Pokazuje ustawienia dla wybranego trybu
        /// </summary>
        /// <param name="mode">wybrany tryb</param>
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

        /*
         * xaml'owe wydarzenia...
         */

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
            InitPlayerLabels();
            InitBoard();
        }

        private void cnv_MainCanvasWrapper_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AlignBoardPlacement(e.NewSize);
        }

        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            // podmienia zawartości paneli
            static void SwapContents(object v1, object v2)
            {
                string temp = (string)(v1 as Label).Content;
                (v1 as Label).Content = (v2 as Label).Content;
                (v2 as Label).Content = temp;

                ((v1 as Label).Background, (v2 as Label).Background) = ((v2 as Label).Background, (v1 as Label).Background);
            }

            mainBoard.Rotate();

            SwapContents(WhitePlayerPanel.Children[0], BlackPlayerPanel.Children[0]);
            SwapContents(WhitePlayerPanel.Children[1], BlackPlayerPanel.Children[1]);
            (WhitePlayerPanel, BlackPlayerPanel) = (BlackPlayerPanel, WhitePlayerPanel);
        }

        private void SavePositionButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(mainGameManager.CurrentFEN);

            MessageBox.Show("Skopiowano pozycję do schowka!", "Zapis", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void EndGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainGameManager.GameRunning)
            {
                var res = MessageBox.Show("Napewno chcesz zakończyć grę?", "Zakończenie gry", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (res == MessageBoxResult.No) return;
            }

            mainGameManager.ForceGameOver();

            ReturnToLobby();
        }

        private void btn_ResetPosition_Click(object sender, RoutedEventArgs e)
        {
            tb_StartPosition.Text = defaultFENPosition;
        }
    }
}
