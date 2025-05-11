using System;
using System.Windows;
using System.Windows.Controls;
using GalaxyGame; // Assuming this is the namespace for your Game class

namespace GalacticBusinessSimulator
{
    public partial class MainWindow : Window
    {
        private readonly Game _game;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize the game with default settings (e.g., 1 player)
            _game = new Game(playerCount: 1);
            _game.GameMessageLogged += Game_GameMessageLogged;
            _game.GameStateChanged += Game_GameStateChanged;

            // Start the game and update UI
            _game.StartGame();
            UpdateUI();
        }

        // Event handler for game messages (logs to txtGameLog)
        private void Game_GameMessageLogged(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtGameLog.AppendText($"{message}\n");
                txtGameLog.ScrollToEnd();
            });
        }

        // Event handler for game state changes (updates player info, planets, cards, etc.)
        private void Game_GameStateChanged()
        {
            Dispatcher.Invoke(UpdateUI);
        }

        // Updates the UI with current game state
        private void UpdateUI()
        {
            var currentPlayer = _game.CurrentPlayer;

            // Update player information
            txtPlayerName.Text = $"Player: {currentPlayer.Name}";
            txtPlayerCredits.Text = $"Credits: {currentPlayer.Credits}";
            txtPlayerPosition.Text = $"Position: {currentPlayer.Position}";
            txtTurnInfo.Text = $"Turn: {_game.CurrentTurn} (Player: {currentPlayer.Name})";

            // Update owned planets
            lstOwnedPlanets.ItemsSource = currentPlayer.OwnedPlanets.Select(p => p.Name);

            // Update player cards
            lstPlayerCards.ItemsSource = currentPlayer.Cards.Select(c => c.Type.ToString());

            // Update location information
            txtLocationTitle.Text = $"Location: Position {_game.CurrentPlayer.Position}";
            pnlLocationInfo.Children.Clear();
            if (_game.Galaxy.IsSolarSystemAt(currentPlayer.Position))
            {
                var system = _game.Galaxy.GetSolarSystemAt(currentPlayer.Position);
                var systemLabel = new TextBlock { Text = $"Solar System: {system.Name}", FontWeight = FontWeights.Bold };
                pnlLocationInfo.Children.Add(systemLabel);

                foreach (var planet in system.Planets)
                {
                    var planetInfo = new TextBlock
                    {
                        Text = $"{planet.Name} - Owner: {(planet.Owner?.Name ?? "None")}",
                        Margin = new Thickness(0, 2, 0, 2)
                    };
                    pnlLocationInfo.Children.Add(planetInfo);
                }
            }
            else if (_game.Galaxy.IsRailwayStationAt(currentPlayer.Position))
            {
                pnlLocationInfo.Children.Add(new TextBlock { Text = "Railway Station" });
            }
            else if (_game.Galaxy.IsAnomalyAt(currentPlayer.Position))
            {
                pnlLocationInfo.Children.Add(new TextBlock { Text = "Anomaly" });
            }
            else if (_game.Galaxy.IsPirateAt(currentPlayer.Position))
            {
                pnlLocationInfo.Children.Add(new TextBlock { Text = "Pirate Territory" });
            }
        }

        // Event handler for Roll Dice button
        private void btnRollDice_Click(object sender, RoutedEventArgs e)
        {
            _game.MovePlayer();
        }

        // Event handler for Skip Turn button
        private void btnSkipTurn_Click(object sender, RoutedEventArgs e)
        {
            _game.SkipTurn();
        }

        // Event handler for Build/Upgrade button
        private void btnBuild_Click(object sender, RoutedEventArgs e)
        {
            if (!_game.Galaxy.IsSolarSystemAt(_game.CurrentPlayer.Position))
            {
                ShowModal("Build Error", new TextBlock { Text = "You must be in a solar system to build or upgrade." });
                return;
            }

            var planets = _game.GetBuildablePlanets();
            if (planets.Count == 0)
            {
                ShowModal("Build Error", new TextBlock { Text = "No planets available to build or upgrade." });
                return;
            }

            // Create a modal to select a planet
            var comboBox = new ComboBox
            {
                ItemsSource = planets.Select(p => p.DisplayText),
                Margin = new Thickness(0, 10, 0, 10)
            };
            var buildButton = new Button
            {
                Content = "Select Planet",
                Width = 100,
                Margin = new Thickness(0, 10, 0, 0)
            };

            buildButton.Click += (s, args) =>
            {
                if (comboBox.SelectedIndex >= 0)
                {
                    var selectedPlanet = planets[comboBox.SelectedIndex].Planet;
                    ShowBuildOptionsModal(selectedPlanet);
                    CloseModal();
                }
            };

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock { Text = "Select a planet to build or upgrade:" });
            stackPanel.Children.Add(comboBox);
            stackPanel.Children.Add(buildButton);

            ShowModal("Build or Upgrade", stackPanel);
        }

        // Event handler for Use Card button
        private void btnUseCard_Click(object sender, RoutedEventArgs e)
        {
            if (lstPlayerCards.SelectedItem == null)
            {
                ShowModal("Card Error", new TextBlock { Text = "No card selected." });
                return;
            }

            var selectedCard = _game.CurrentPlayer.Cards[lstPlayerCards.SelectedIndex];
            _game.ProcessCard(selectedCard);
        }

        // Event handler for View Map button
        private void btnViewMap_Click(object sender, RoutedEventArgs e)
        {
            // Placeholder for map view (implement as needed)
            var mapInfo = new TextBlock
            {
                Text = "Map View (Not Implemented)\n" +
                       $"Total Positions: {_game.Galaxy.TotalPositions}\n" +
                       $"Current Position: {_game.CurrentPlayer.Position}"
            };
            ShowModal("Galaxy Map", mapInfo);
        }

        // Event handler for ListBox selection changed
        private void lstPlayerCards_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: Update UI or enable/disable btnUseCard based on selection
            btnUseCard.IsEnabled = lstPlayerCards.SelectedItem != null;
        }

        // Event handler for modal close button
        private void btnModalClose_Click(object sender, RoutedEventArgs e)
        {
            CloseModal();
        }

        // Helper method to show modal
        private void ShowModal(string title, UIElement content)
        {
            modalTitle.Text = title;
            modalBody.Content = content;
            modalOverlay.Visibility = Visibility.Visible;
        }

        // Helper method to close modal
        private void CloseModal()
        {
            modalOverlay.Visibility = Visibility.Collapsed;
            modalBody.Content = null;
        }

        // Helper method to show build options for a selected planet
        private void ShowBuildOptionsModal(Planet planet)
        {
            var stackPanel = new StackPanel();

            // New structure options
            var availableStructures = _game.GetAvailableStructures(planet);
            if (availableStructures.Any())
            {
                stackPanel.Children.Add(new TextBlock { Text = "Build New Structure:", FontWeight = FontWeights.Bold });
                var structureComboBox = new ComboBox
                {
                    ItemsSource = availableStructures.Select(s => $"{s.Type} (Cost: {s.Cost})"),
                    Margin = new Thickness(0, 5, 0, 5)
                };
                var buildButton = new Button
                {
                    Content = "Build",
                    Width = 100,
                    Margin = new Thickness(0, 5, 0, 5)
                };
                buildButton.Click += (s, args) =>
                {
                    if (structureComboBox.SelectedIndex >= 0)
                    {
                        var selectedStructure = availableStructures[structureComboBox.SelectedIndex].Type;
                        _game.BuildOrUpgradeStructure(planet, true, selectedStructure);
                        CloseModal();
                    }
                };
                stackPanel.Children.Add(structureComboBox);
                stackPanel.Children.Add(buildButton);
            }

            // Upgrade structure options
            var upgradableStructures = _game.GetUpgradableStructures(planet);
            if (upgradableStructures.Any())
            {
                stackPanel.Children.Add(new TextBlock
                {
                    Text = "Upgrade Structure:",
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(0, 10, 0, 0)
                });
                var upgradeComboBox = new ComboBox
                {
                    ItemsSource = upgradableStructures.Select(s => $"{s.Type} (Level {s.Level})"),
                    Margin = new Thickness(0, 5, 0, 5)
                };
                var upgradeButton = new Button
                {
                    Content = "Upgrade",
                    Width = 100,
                    Margin = new Thickness(0, 5, 0, 5)
                };
                upgradeButton.Click += (s, args) =>
                {
                    if (upgradeComboBox.SelectedIndex >= 0)
                    {
                        var selectedStructure = upgradableStructures[upgradeComboBox.SelectedIndex];
                        _game.BuildOrUpgradeStructure(planet, false, null, selectedStructure);
                        CloseModal();
                    }
                };
                stackPanel.Children.Add(upgradeComboBox);
                stackPanel.Children.Add(upgradeButton);
            }

            if (!availableStructures.Any() && !upgradableStructures.Any())
            {
                stackPanel.Children.Add(new TextBlock { Text = "No build or upgrade options available." });
            }

            ShowModal($"Build/Upgrade on {planet.Name}", stackPanel);
        }

        // Override Window Closing to save game
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _game.QuitGame();
            base.OnClosing(e);
        }
    }
}