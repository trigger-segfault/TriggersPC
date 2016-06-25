using PokemonManager.PokemonStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PokemonManager.Windows {

	public partial class SpriteCustomizationWindow : Window {

		struct SpriteSelecionTag {
			public ushort DexID;
			public FrontSpriteSelectionTypes Type;
		}

		private bool shinyMode;
		private Brush spriteDisabled;
		private Brush spriteHighlight;
		private Brush spriteHighlightChecked;
		private Brush spriteUnhighlight;
		private Brush spriteUnhighlightChecked;

		public SpriteCustomizationWindow() {
			InitializeComponent();

			this.shinyMode = false;

			this.spriteDisabled				= new SolidColorBrush(Color.FromRgb(170, 170, 170));
			this.spriteUnhighlight			= new SolidColorBrush(Color.FromRgb(255, 255, 255));
			this.spriteHighlight			= new SolidColorBrush(Color.FromRgb(173, 210, 255));
			this.spriteUnhighlightChecked	= new SolidColorBrush(Color.FromRgb(23, 108, 211));
			this.spriteHighlightChecked		= new SolidColorBrush(Color.FromRgb(79, 149, 234));

			this.checkBoxDifferentShiny.IsChecked = PokeManager.Settings.UseDifferentShinyFrontSprites;
			this.checkBoxShowShiny.IsEnabled = PokeManager.Settings.UseDifferentShinyFrontSprites;

			PopulateList();
		}

		public static void Show(Window window) {
			SpriteCustomizationWindow form = new SpriteCustomizationWindow();
			form.Owner = window;
			form.ShowDialog();
		}

		private void PopulateList() {
			for (int i = 0; i < 387; i++) {
				Grid grid = new Grid();
				stackPanelList.Children.Add(grid);
				FillGridItem((ushort)(i + 1));
			}
		}

		private void UpdateList(bool recreate = false) {
			for (int i = 0; i < 387; i++) {
				if (recreate)
					FillGridItem((ushort)(i + 1));
				else
					UpdateGridItem((ushort)(i + 1));
			}
		}

		private void UpdateGridItem(ushort dexID) {
			Grid grid = stackPanelList.Children[dexID - 1] as Grid;
			StackPanel stackPanel = grid.Children[0] as StackPanel;
			int shinyID = shinyMode ? 1 : 0;
			(stackPanel.Children[0] as Grid).Background = (
				PokeManager.Settings.FrontSpriteSelections[shinyID, dexID - 1] == FrontSpriteSelectionTypes.RSE ?
				spriteUnhighlightChecked : spriteUnhighlight
			);
			(stackPanel.Children[1] as Grid).Background = (
				PokeManager.Settings.FrontSpriteSelections[shinyID, dexID - 1] == FrontSpriteSelectionTypes.FRLG ?
				spriteUnhighlightChecked : spriteUnhighlight
			);
			(stackPanel.Children[2] as Grid).Background = (
				PokeManager.Settings.FrontSpriteSelections[shinyID, dexID - 1] == FrontSpriteSelectionTypes.Custom ?
				spriteUnhighlightChecked : spriteUnhighlight
			);
			bool hasFRLG = PokemonDatabase.HasPokemonImageType(dexID, FrontSpriteSelectionTypes.FRLG, shinyMode);
			bool hasCustom = PokemonDatabase.HasPokemonImageType(dexID, FrontSpriteSelectionTypes.Custom, shinyMode);
			grid.Visibility = ((!hasFRLG && !hasCustom) ? Visibility.Collapsed : Visibility.Visible);
		}

		private void FillGridItem(ushort dexID) {
			Grid grid = stackPanelList.Children[dexID - 1] as Grid;
			grid.Width = double.NaN;
			grid.HorizontalAlignment = HorizontalAlignment.Stretch;
			grid.Height = 64 + 18;
			grid.IsHitTestVisible = true;
			grid.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
			grid.Children.Clear();

			Label labelName = new Label();
			if (dexID == 387)
				labelName.Content = "Egg - Pokémon Egg";
			else
				labelName.Content = "No" + dexID.ToString("000") + " - " + PokemonDatabase.GetPokemonFromDexID(dexID).Name;
			labelName.Padding = new Thickness(5, 1, 5, 1);
			labelName.FontWeight = FontWeights.Bold;
			labelName.VerticalAlignment = VerticalAlignment.Top;
			labelName.IsHitTestVisible = false;

			StackPanel stackPanel = new StackPanel();
			stackPanel.Margin = new Thickness(0, 18, 0, 0);
			stackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
			stackPanel.VerticalAlignment = VerticalAlignment.Stretch;
			stackPanel.Orientation = Orientation.Horizontal;
			stackPanel.Height = 64;
			stackPanel.Width = double.NaN;

			bool hasFRLG = PokemonDatabase.HasPokemonImageType(dexID, FrontSpriteSelectionTypes.FRLG, shinyMode);
			bool hasCustom = PokemonDatabase.HasPokemonImageType(dexID, FrontSpriteSelectionTypes.Custom, shinyMode);
			stackPanel.Children.Add(CreateSpriteSelection(dexID, FrontSpriteSelectionTypes.RSE));
			if (hasFRLG)
				stackPanel.Children.Add(CreateSpriteSelection(dexID, FrontSpriteSelectionTypes.FRLG));
			else
				stackPanel.Children.Add(CreateBlankSpriteSelection(dexID, FrontSpriteSelectionTypes.FRLG));
			if (hasCustom)
				stackPanel.Children.Add(CreateSpriteSelection(dexID, FrontSpriteSelectionTypes.Custom));
			else
				stackPanel.Children.Add(CreateBlankSpriteSelection(dexID, FrontSpriteSelectionTypes.Custom));

			grid.Children.Add(stackPanel);
			grid.Children.Add(labelName);

			grid.Visibility = ((!hasFRLG && !hasCustom) ? Visibility.Collapsed : Visibility.Visible);
			grid.Tag = dexID;
		}

		private UIElement CreateSpriteSelection(ushort dexID, FrontSpriteSelectionTypes type) {
			Grid grid = new Grid();
			// Spinda has spot drawing code so we're not allowed to change him
			if (dexID != 327) {
				grid.PreviewMouseDown += OnSpriteSelectionClicked;
				grid.MouseEnter += OnSpriteSelectionEnter;
				grid.MouseLeave += OnSpriteSelectionLeave;
				if (PokeManager.Settings.FrontSpriteSelections[shinyMode ? 1 : 0, dexID - 1] == type)
					grid.Background = spriteUnhighlightChecked;
				else
					grid.Background = spriteUnhighlight;
			}
			else {
				grid.Background = spriteDisabled;
			}
			grid.Width = 64;
			grid.Height = 64;

			Image sprite = new Image();
			sprite.Width = 64;
			sprite.Height = 64;
			sprite.IsHitTestVisible = false;
			if (type == FrontSpriteSelectionTypes.RSE) {
				if (shinyMode)
					sprite.Source = PokemonDatabase.GetPokemonImageTypes(dexID).ShinyImage;
				else
					sprite.Source = PokemonDatabase.GetPokemonImageTypes(dexID).Image;
			}
			else if (type == FrontSpriteSelectionTypes.FRLG) {
				if (shinyMode)
					sprite.Source = PokemonDatabase.GetPokemonImageTypes(dexID).FRLGShinyImage;
				else
					sprite.Source = PokemonDatabase.GetPokemonImageTypes(dexID).FRLGImage;
			}
			else {
				if (shinyMode)
					sprite.Source = PokemonDatabase.GetPokemonImageTypes(dexID).CustomShinyImage;
				else
					sprite.Source = PokemonDatabase.GetPokemonImageTypes(dexID).CustomImage;
			}

			grid.Children.Add(sprite);

			grid.Tag = new SpriteSelecionTag{ DexID = dexID, Type = type };
			return grid;
		}

		private UIElement CreateBlankSpriteSelection(ushort dexID, FrontSpriteSelectionTypes type) {
			Grid grid = new Grid();
			grid.Width = 64;
			grid.Height = 64;
			if (dexID == 327)
				grid.Background = spriteDisabled;
			else if (PokeManager.Settings.FrontSpriteSelections[shinyMode ? 1 : 0, dexID - 1] == type)
				grid.Background = spriteUnhighlightChecked;
			else
				grid.Background = spriteUnhighlight;

			Label label = new Label();
			label.HorizontalAlignment = HorizontalAlignment.Center;
			label.VerticalAlignment = VerticalAlignment.Center;
			label.Padding = new Thickness(1, 1, 1, 1);
			label.Content = "No " + type.ToString();

			grid.Children.Add(label);
			return grid;
		}

		private void OnSpriteSelectionClicked(object sender, MouseButtonEventArgs e) {
			Grid grid = sender as Grid;
			SpriteSelecionTag tag = (SpriteSelecionTag)grid.Tag;
			if (e.ChangedButton == MouseButton.Left && PokeManager.Settings.FrontSpriteSelections[shinyMode ? 1 : 0, tag.DexID - 1] != tag.Type) {
				PokeManager.Settings.FrontSpriteSelections[shinyMode ? 1 : 0, tag.DexID - 1] = tag.Type;
				UpdateGridItem(tag.DexID);
				grid.Background = spriteHighlightChecked;
				if (!PokeManager.Settings.UseDifferentShinyFrontSprites)
					PokeManager.Settings.FrontSpriteSelections[1, tag.DexID - 1] = tag.Type;
			}
		}

		private void OnSpriteSelectionEnter(object sender, MouseEventArgs e) {
			Grid grid = sender as Grid;
			SpriteSelecionTag tag = (SpriteSelecionTag)grid.Tag;
			if (PokeManager.Settings.FrontSpriteSelections[shinyMode ? 1 : 0, tag.DexID - 1] == tag.Type)
				grid.Background = spriteHighlightChecked;
			else
				grid.Background = spriteHighlight;
		}
		private void OnSpriteSelectionLeave(object sender, MouseEventArgs e) {
			Grid grid = sender as Grid;
			SpriteSelecionTag tag = (SpriteSelecionTag)grid.Tag;
			if (PokeManager.Settings.FrontSpriteSelections[shinyMode ? 1 : 0, tag.DexID - 1] == tag.Type)
				grid.Background = spriteUnhighlightChecked;
			else
				grid.Background = spriteUnhighlight;
		}

		private void OnRefreshClicked(object sender, RoutedEventArgs e) {
			PokemonDatabase.ReloadCustomPokemonSprites();
			UpdateList(true);
		}

		private void OnSetRSEClicked(object sender, RoutedEventArgs e) {
			for (int i = 0; i < 387; i++) {
				if (i + 1 == 327)
					continue;
				if (!shinyMode)
					PokeManager.Settings.FrontSpriteSelections[0, i] = FrontSpriteSelectionTypes.RSE;
				if (shinyMode)
					PokeManager.Settings.FrontSpriteSelections[1, i] = FrontSpriteSelectionTypes.RSE;
				UpdateGridItem((ushort)(i + 1));
			}
		}

		private void OnSetFRLGClicked(object sender, RoutedEventArgs e) {
			for (int i = 0; i < 387; i++) {
				if (i + 1 == 327)
					continue;
				if (!shinyMode &&
					PokemonDatabase.HasPokemonImageType((ushort)(i + 1), FrontSpriteSelectionTypes.FRLG, false))
					PokeManager.Settings.FrontSpriteSelections[0, i] = FrontSpriteSelectionTypes.FRLG;
				if (shinyMode &&
					PokemonDatabase.HasPokemonImageType((ushort)(i + 1), FrontSpriteSelectionTypes.FRLG, true))
					PokeManager.Settings.FrontSpriteSelections[1, i] = FrontSpriteSelectionTypes.FRLG;
				UpdateGridItem((ushort)(i + 1));
			}
		}

		private void OnSetCustomClicked(object sender, RoutedEventArgs e) {
			for (int i = 0; i < 387; i++) {
				if (i + 1 == 327)
					continue;
				if (!shinyMode &&
					PokemonDatabase.HasPokemonImageType((ushort)(i + 1), FrontSpriteSelectionTypes.Custom, false))
					PokeManager.Settings.FrontSpriteSelections[0, i] = FrontSpriteSelectionTypes.Custom;
				if (shinyMode &&
					PokemonDatabase.HasPokemonImageType((ushort)(i + 1), FrontSpriteSelectionTypes.Custom, true))
					PokeManager.Settings.FrontSpriteSelections[1, i] = FrontSpriteSelectionTypes.Custom;
				UpdateGridItem((ushort)(i + 1));
			}
		}

		private void OnShowShinyChecked(object sender, RoutedEventArgs e) {
			shinyMode = checkBoxShowShiny.IsChecked.Value;
			UpdateList(true);
		}

		private void OnDifferentShinyChecked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.UseDifferentShinyFrontSprites = checkBoxDifferentShiny.IsChecked.Value;
			if (!PokeManager.Settings.UseDifferentShinyFrontSprites) {
				checkBoxShowShiny.IsChecked = false;
				checkBoxShowShiny.IsEnabled = false;

				if (shinyMode) {
					shinyMode = false;
					UpdateList(true);
				}
			}
			else {
				checkBoxShowShiny.IsEnabled = true;
			}
		}

		private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
			PokeManager.SaveSettings();
			PokeManager.RefreshUI();
		}
	}
}
