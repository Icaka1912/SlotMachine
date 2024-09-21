using System;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using System.Collections.Generic;

namespace SlotGame
{

    public partial class MainWindow : Window
    {
        private SoundPlayer player;
        private SoundPlayer columnStopSound;
        private SoundPlayer winSound1;
        private SoundPlayer winSound2;
        private int balance;
        private int spinAttemptsWhenZeroBalance;
        private bool isSpinning;
        private Random _random;
        private Dictionary<string, int> _symbolWeights;
        private Dictionary<string, Dictionary<int, int>> _symbolPayouts;
        private decimal _balance;
        private List<int[]> winningLines;

        public MainWindow()
        {
            InitializeComponent();

            BalanceWindow balanceWindow = new BalanceWindow();
            bool? result = balanceWindow.ShowDialog();

            if (result == true)
            {
                _balance = balanceWindow.Balance;
                balance = (int)_balance;
                MessageBox.Show($"Opening balance: {_balance} BGN.", "Balance", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("You have not entered a balance!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                Application.Current.Shutdown();
            }

            player = new SoundPlayer(@"C:\\Users\\hrist\\source\\repos\\SlotGame\\SlotGame\\spinaudio.wav");
            player.Load();

            columnStopSound = new SoundPlayer(@"C:\Users\hrist\source\repos\SlotGame\SlotGame\reelstopsound.wav");
            columnStopSound.Load();

            winSound1 = new SoundPlayer(@"C:\Users\hrist\source\repos\SlotGame\SlotGame\casinowinsound.wav");
            winSound1.Load();

            winSound2 = new SoundPlayer(@"C:\Users\hrist\source\repos\SlotGame\SlotGame\casinowinsound2.wav");
            winSound2.Load();

            _random = new Random();
            _symbolWeights = new Dictionary<string, int>
                {
                {"cherries.png", 200},
                {"lemon.png", 200},
                {"watermelon.png", 110},
                {"orange.png", 200},
                {"grape.png", 110},
                {"plum.png", 110},
                {"scatter.png", 40},
                {"seven.png", 30}
            };

            _symbolPayouts = new Dictionary<string, Dictionary<int, int>>
{
    { "cherries.png", new Dictionary<int, int> { {3, 4}, {4, 10}, {5, 40} } }, // Череши
    { "lemon.png", new Dictionary<int, int> { {3, 4}, {4, 10}, {5, 40} } },   // Лимон
    { "orange.png", new Dictionary<int, int> { {3, 4}, {4, 10}, {5, 40} } },  // Портокал
    { "plum.png", new Dictionary<int, int> { {3, 4}, {4, 10}, {5, 40} } },    // Слива
    { "grape.png", new Dictionary<int, int> { {3, 10}, {4, 40}, {5, 100} } }, // Грозде
    { "watermelon.png", new Dictionary<int, int> { {3, 10}, {4, 40}, {5, 100} } }, // Диня
    { "seven.png", new Dictionary<int, int> { {3, 20}, {4, 200}, {5, 1000} } },    // Червено 7
    { "scatter.png", new Dictionary<int, int> { {3, 2}, {4, 10}, {5, 50} } }       // Scatter звезда
};

            winningLines = new List<int[]>
            {
                new int[] { 0, 1, 2,},
                new int[] { 0, 1, 2, 3},
                new int[] { 0, 1, 2, 3, 4 },
                new int[] { 5, 6, 7},
                new int[] { 5, 6, 7, 8},
                new int[] { 5, 6, 7, 8, 9 },
                new int[] { 10, 11, 12},
                new int[] { 10, 11, 12, 13},
                new int[] { 10, 11, 12, 13, 14 },
                new int[] { 10, 6, 2},
                new int[] { 10, 6, 2, 8},
                new int[] { 10, 6, 2, 8, 14},
                new int[] { 0, 6, 12},
                new int[] { 0, 6, 12, 8},
                new int[] { 0, 6, 12, 8, 4}
            };

            spinAttemptsWhenZeroBalance = 0;
            UpdateBalanceText();
            InitializeReels();
        }

        private void UpdateBalanceText()
        {
            BalanceBox.Text = $"Balance :  {balance} BGN";
        }
        private void InitializeReels()
        {
            foreach (UIElement element in ReelsGrid.Children)
            {
                if (element is Image)
                {
                    (element as Image).Source = new BitmapImage(new Uri($"pack://application:,,,/images/{GetRandomSymbol()}"));
                }
            }
        }

        private string GetRandomSymbol()
        {
            int totalWeight = _symbolWeights.Values.Sum();
            int randomValue = _random.Next(totalWeight);

            foreach (var symbol in _symbolWeights)
            {
                if (randomValue < symbol.Value)
                {
                    return symbol.Key;
                }
                randomValue -= symbol.Value;
            }

            return _symbolWeights.Keys.Last();
        }

        private async Task SpinReelColumn(int column)
        {
            Random tempRandom = new Random();
            int spinTime = 700;
            int interval = 200;


            var spinEndTime = DateTime.Now.AddMilliseconds(spinTime + (column * 500));
            while (DateTime.Now < spinEndTime)
            {

                for (int row = 0; row < 3; row++)
                {
                    int index = row * 5 + column;

                    if (ReelsGrid.Children[index] is Image image)
                    {

                        string randomSymbol = _symbolWeights.Keys.ElementAt(tempRandom.Next(_symbolWeights.Count));
                        image.Source = new BitmapImage(new Uri($"pack://application:,,,/images/{randomSymbol}"));
                    }
                }

                await Task.Delay(interval);
            }


            for (int row = 0; row < 3; row++)
            {
                int index = row * 5 + column;

                if (ReelsGrid.Children[index] is Image image)
                {

                    string finalSymbol = GetRandomSymbol();
                    image.Source = new BitmapImage(new Uri($"pack://application:,,,/images/{finalSymbol}"));

                    DoubleAnimation fadeAnimation = new DoubleAnimation
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromMilliseconds(500),
                    };

                    image.BeginAnimation(Image.OpacityProperty, fadeAnimation);

                    ScaleTransform scaleTransform = new ScaleTransform();
                    image.RenderTransform = scaleTransform;

                    DoubleAnimation zoomAnimation = new DoubleAnimation
                    {
                        From = 0.9,
                        To = 1.0,
                        Duration = TimeSpan.FromMilliseconds(500),
                        EasingFunction = new BackEase { Amplitude = 1.5, EasingMode = EasingMode.EaseOut }
                    };

                    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, zoomAnimation);
                    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, zoomAnimation);
                }
            }
            columnStopSound.Play();
        }

        private bool CheckLineForWin(int[] line, out int matchCount)
        {
            matchCount = 1;

            if (ReelsGrid.Children[line[0]] is Image firstImage)
            {
                var firstSymbol = ((BitmapImage)firstImage.Source).UriSource.OriginalString;

                for (int i = 1; i < line.Length; i++)
                {
                    if (ReelsGrid.Children[line[i]] is Image nextImage)
                    {
                        var nextSymbol = ((BitmapImage)nextImage.Source).UriSource.OriginalString;

                        if (firstSymbol == nextSymbol)
                        {
                            matchCount++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return matchCount >= 3; // Връща true ако има поне 3 съвпадения
            }

            return false;
        }


        private int CalculateTotalWin()
        {
            int totalWin = 0;
            foreach (var line in winningLines)
            {
                int winForLine = CalculateWinForLine(line);
                totalWin += winForLine;
            }

            balance += totalWin;
            UpdateBalanceText();

            return totalWin;
        }

        private int CalculateWinForLine(int[] line)
        {
            if (ReelsGrid.Children[line[0]] is Image firstImage)
            {
                var firstSymbol = ((BitmapImage)firstImage.Source).UriSource.OriginalString;
                int matchCount = 1;

                for (int i = 1; i < line.Length; i++)
                {
                    if (ReelsGrid.Children[line[i]] is Image nextImage)
                    {
                        var nextSymbol = ((BitmapImage)nextImage.Source).UriSource.OriginalString;
                        if (firstSymbol == nextSymbol)
                        {
                            matchCount++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                string symbolName = System.IO.Path.GetFileName(firstSymbol);
                if (_symbolPayouts.ContainsKey(symbolName) && _symbolPayouts[symbolName].ContainsKey(matchCount))
                {
                    return _symbolPayouts[symbolName][matchCount];
                }
            }

            return 0;
        }

        private void HighlightWinningLine(int[] line, int matchCount)
        {
            // Ограничаваме подчертаването до броя на съвпаденията (напр. 3, 4 или 5)
            for (int i = 0; i < matchCount; i++)
            {
                if (ReelsGrid.Children[line[i]] is Image image)
                {
                    DoubleAnimation animation = new DoubleAnimation
                    {
                        From = 1,
                        To = 0,
                        Duration = TimeSpan.FromMilliseconds(500),
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever
                    };

                    image.BeginAnimation(UIElement.OpacityProperty, animation);
                }
            }
        }

        private void StopAllAnimations()
        {
            foreach (UIElement element in ReelsGrid.Children)
            {
                if (element is Image image)
                {
                    // Спиране на всяка текуща анимация върху изображението
                    image.BeginAnimation(UIElement.OpacityProperty, null);
                }
            }
        }

        private async void SpinButton_Click(object sender, RoutedEventArgs e)
        {
            if (isSpinning) return;

            StopAllAnimations();

            if (balance <= 0)
            {
                spinAttemptsWhenZeroBalance++;

                if (spinAttemptsWhenZeroBalance > 1)
                {
                    BalanceWindow balanceWindow = new BalanceWindow();
                    bool? result = balanceWindow.ShowDialog();

                    if (result == true)
                    {
                        _balance = balanceWindow.Balance;
                        balance += (int)_balance;
                        MessageBox.Show($"New balance: {_balance} BGN.", "Balance", MessageBoxButton.OK, MessageBoxImage.Information);
                        spinAttemptsWhenZeroBalance = 0;
                        UpdateBalanceText();
                    }
                    else
                    {
                        MessageBox.Show("You have not entered a new balance!", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    MessageBox.Show($"You have no balance! If you want to continue your game, press the spin button to enter a new amount!", "No balance", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                return;
            }

            balance -= 1;
            UpdateBalanceText();

            isSpinning = true;
            player.Play();

            List<Task> spinTasks = new List<Task>();

            for (int column = 0; column < 5; column++)
            {
                spinTasks.Add(SpinReelColumn(column));
            }

            await Task.WhenAll(spinTasks);

            isSpinning = false;

            int totalWin = CalculateTotalWin();
            LastWinBox.Text = $"Last Win : {totalWin} BGN";

            // Пускане на звуците при печеливша линия
            bool hasWinningLine = false;
            foreach (var line in winningLines)
            {
                int matchCount;
                if (CheckLineForWin(line, out matchCount))
                {
                    hasWinningLine = true;
                    HighlightWinningLine(line, matchCount);
                }
            }

            // Пускане на звуците само ако има печеливша линия
            if (hasWinningLine)
            {
                winSound1.Play();
                winSound2.Play();
            }
        }
    }
}
