using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
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
using WPF.JoshSmith.ServiceProviders.UI;
using Xceed.Wpf.Toolkit;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for SendItemToWindow.xaml
	/// </summary>
	public partial class ManageBoxesWindow : Window {

		private ManagerPokePC pokePC;
		private ListViewDragDropManager<ListViewItem> dropManager;
		private ObservableCollection<ListViewItem> boxes;
		private int selectedIndex;
		private IPokeBox selectedBox;
		private ContextMenu contextMenu;
		private int rowIndex;

		public ManageBoxesWindow() {
			InitializeComponent();
			this.pokePC = PokeManager.ManagerGameSave.PokePC as ManagerPokePC;

			contextMenu = new ContextMenu();
			MenuItem edit = new MenuItem();
			edit.Header = "Edit Box";
			edit.Click += OnEditBox;
			MenuItem insert = new MenuItem();
			insert.Header = "Insert Box";
			insert.Click += OnInsertBox;
			MenuItem delete = new MenuItem();
			delete.Header = "Remove Box";
			delete.Click += OnDeleteBox;
			delete.IsEnabled = pokePC.NumBoxes > 1;
			contextMenu.Items.Add(edit);
			contextMenu.Items.Add(insert);
			contextMenu.Items.Add(delete);

			selectedIndex = -1;

			boxes = new ObservableCollection<ListViewItem>();
			PopulateBoxes();

			listViewBoxes.ItemsSource = boxes;
			dropManager = new ListViewDragDropManager<ListViewItem>(listViewBoxes);
			dropManager.ProcessDrop += OnProcessDrop;

			pokeBoxControl.UnloadBox();
			pokeBoxControl.Mode = PokeBoxControlModes.ViewOnly;
			pokeBoxControl.PokemonViewer = pokemonViewer;

			buttonRemoveRow.IsEnabled = PokeManager.ManagerGameSave.NumPokePCRows > 1;
		}

		private void PopulateBoxes() {
			boxes.Clear();
			for (int i = 0; i < pokePC.NumBoxes; i++) {
				ListViewItem listViewItem = new ListViewItem();
				MakeBox(i, listViewItem);
				boxes.Add(listViewItem);
			}
		}

		private void MakeBox(int index, ListViewItem listViewItem) {
			IPokeBox pokeBox = pokePC[index];
			StackPanel stackPanel = new StackPanel();
			stackPanel.Orientation = Orientation.Horizontal;

			double scale = 0.3;
			int xOffset = 2;
			int yOffset = 2;
			Grid boxGrid = new Grid();
			boxGrid.Width = xOffset * 2 + 156 * scale;
			boxGrid.Height  = yOffset * 2 + 141 * scale;
			Image wallpaperImage = new Image();
			wallpaperImage.Source = pokeBox.WallpaperImage;
			wallpaperImage.Width = 156 * scale;
			wallpaperImage.Height = 141 * scale;
			wallpaperImage.Margin = new Thickness(xOffset, yOffset, 0, 0);
			boxGrid.Children.Add(wallpaperImage);
			for (int y = 0; y < 5; y++) {
				for (int x = 0; x < 6; x++) {
					IPokemon pokemon = pokeBox[x + y * 6];
					if (pokemon != null) {
						// Dont bother with the shadow mask. It's not a big deal here
						Image boxImage = new Image();
						boxImage.Width = 32 * scale;
						boxImage.Height = 32 * scale;
						boxImage.Margin = new Thickness(xOffset + (2 + x * 24) * scale, yOffset + (13 + y * 24) * scale, 0, 0);
						boxImage.Source = pokemon.BoxSprite;
						boxImage.HorizontalAlignment = HorizontalAlignment.Left;
						boxImage.VerticalAlignment = VerticalAlignment.Top;
						boxGrid.Children.Add(boxImage);
					}
				}
			}
			stackPanel.Children.Add(boxGrid);


			Label name = new Label();
			name.Content = pokeBox.Name;
			name.Margin = new Thickness(3, 0, 0, 0);
			name.FontWeight = FontWeights.Bold;
			name.VerticalAlignment = VerticalAlignment.Center;
			stackPanel.Children.Add(name);

			listViewItem.ContextMenu = contextMenu;
			listViewItem.ContextMenuOpening += OnContextMenuOpening;
			listViewItem.Content = stackPanel;
			listViewItem.Tag = pokeBox;
		}
		private void UpdateBoxNames() {
			foreach (ListViewItem listViewItem in boxes) {
				IPokeBox box = listViewItem.Tag as IPokeBox;
				((Label)((StackPanel)listViewItem.Content).Children[1]).Content = box.Name;
			}
		}
		private void OnContextMenuOpening(object sender, ContextMenuEventArgs e) {
			if (selectedIndex != -1) {
				((MenuItem)contextMenu.Items[0]).IsEnabled = true;
				((MenuItem)contextMenu.Items[1]).IsEnabled = true;
				((MenuItem)contextMenu.Items[2]).IsEnabled = true;
			}
			else {
				((MenuItem)contextMenu.Items[0]).IsEnabled = false;
				((MenuItem)contextMenu.Items[1]).IsEnabled = false;
				((MenuItem)contextMenu.Items[2]).IsEnabled = false;
			}
		}

		public static bool? ShowDialog(Window owner) {
			ManageBoxesWindow window = new ManageBoxesWindow();
			window.Owner = owner;
			return window.ShowDialog();
		}

		private void OnAddBox(object sender, RoutedEventArgs e) {
			pokePC.AddBox();
			//pokePC[(int)pokePC.NumBoxes - 1].Name = "NEW BOX";
			ListViewItem listViewItem = new ListViewItem();
			MakeBox((int)pokePC.NumBoxes - 1, listViewItem);
			boxes.Add(listViewItem);
			((MenuItem)boxes[0].ContextMenu.Items[2]).IsEnabled = pokePC.NumBoxes > 1;
			selectedIndex = (int)pokePC.NumBoxes - 1;
			listViewBoxes.SelectedIndex = selectedIndex;

			// Hackish thing to make sure the list view is always scrolled at the bottom when adding a new box
			//http://stackoverflow.com/questions/211971/scroll-wpf-listview-to-specific-line
			/*VirtualizingStackPanel vsp =  
				(VirtualizingStackPanel)typeof(ItemsControl).InvokeMember("_itemsHost",
				BindingFlags.Instance | BindingFlags.GetField | BindingFlags.NonPublic, null,
				listViewBoxes, null);
			double scrollHeight = vsp.ScrollOwner.ScrollableHeight;
			vsp.SetVerticalOffset(vsp.ScrollOwner.ScrollableHeight * 2);*/

			listViewBoxes.ScrollIntoView(listViewBoxes.SelectedItem);
			((Control)listViewBoxes.SelectedItem).Focus();
		}
		private void OnEditBox(object sender, RoutedEventArgs e) {
			if (selectedBox != null) {
				var result = EditBoxWindow.ShowDialog(this, selectedBox);
				if (result.HasValue && result.Value) {
					((Label)((StackPanel)boxes[selectedIndex].Content).Children[1]).Content = selectedBox.Name;
					pokeBoxControl.LoadBox(selectedBox, PokeManager.GetIndexOfGame(selectedBox.PokePC.GameSave));
				}
			}
		}
		private void OnInsertBox(object sender, RoutedEventArgs e) {
			pokePC.InsertBox(selectedIndex);
			//pokePC[selectedIndex].Name = "NEW BOX";
			ListViewItem listViewItem = new ListViewItem();
			MakeBox(selectedIndex, listViewItem);
			boxes.Insert(selectedIndex, listViewItem);
			((MenuItem)boxes[0].ContextMenu.Items[2]).IsEnabled = pokePC.NumBoxes > 1;
			selectedIndex = (int)pokePC.NumBoxes - 1;
			listViewBoxes.SelectedIndex = selectedIndex;
			UpdateBoxNames();
			pokeBoxControl.LoadBox(selectedBox, PokeManager.GetIndexOfGame(selectedBox.PokePC.GameSave));
		}

		private void OnDeleteBox(object sender, RoutedEventArgs e) {
			if (selectedBox.NumPokemon > 0) {
				TriggerMessageBox.Show(this, "Cannot remove a box containing Pokémon. Release them first.", "Remove Box");
			}
			else if (pokePC.NumBoxes > 1 && selectedIndex < pokePC.NumBoxes && selectedIndex != -1) {
				pokePC.RemoveBoxAt(selectedIndex);
				boxes.RemoveAt(selectedIndex);
				((MenuItem)boxes[0].ContextMenu.Items[2]).IsEnabled = pokePC.NumBoxes > 1;
				UpdateBoxNames();
				pokeBoxControl.UnloadBox();
				selectedIndex = -1;
			}
		}

		private void OnBoxSelectionChanged(object sender, SelectionChangedEventArgs e) {
			int newSelection = listViewBoxes.SelectedIndex;
			if (newSelection != -1) {
				selectedIndex = newSelection;
				selectedBox = boxes[selectedIndex].Tag as IPokeBox;
				pokeBoxControl.LoadBox(selectedBox, PokeManager.GetIndexOfGame(selectedBox.PokePC.GameSave));
			}
		}
		public void OnProcessDrop(object sender, ProcessDropEventArgs<ListViewItem> e) {
			if (e.OldIndex > -1) {
				pokePC.MoveBox(e.OldIndex, e.NewIndex);
				boxes.Move(e.OldIndex, e.NewIndex);
				UpdateBoxNames();
				pokeBoxControl.LoadBox(selectedBox, PokeManager.GetIndexOfGame(selectedBox.PokePC.GameSave));
			}

			e.Effects = DragDropEffects.Move;
		}

		private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
			PokeManager.ReloadRowComboBoxes();
		}

		private void OnEditRow(object sender, RoutedEventArgs e) {
			if (EditRowWindow.ShowDialog(this, PokeManager.ManagerGameSave.GetPokePCRow(rowIndex))) {
				comboBoxRows.RefreshRows();
			}
		}

		private void OnAddRow(object sender, RoutedEventArgs e) {
			if (EditRowWindow.ShowDialog(this, null)) {
				comboBoxRows.ReloadRows();
				comboBoxRows.SelectedIndex = PokeManager.ManagerGameSave.NumPokePCRows - 1;
			}
		}

		private void OnRemoveRow(object sender, RoutedEventArgs e) {
			int numPokemon = 0;
			foreach (IPokemon pokmeon in PokeManager.ManagerGameSave.GetPokePCRow(rowIndex)) {
				numPokemon++;
			}
			if (numPokemon > 0) {
				TriggerMessageBox.Show(this, "Cannot remove a row containing boxes with Pokémon in them. Release them first.", "Remove Row");
			}
			else if (PokeManager.ManagerGameSave.NumPokePCRows > 1 && rowIndex < PokeManager.ManagerGameSave.NumPokePCRows) {
				PokeManager.ManagerGameSave.RemovePokePCRow(rowIndex);
				comboBoxRows.ReloadRows();
				buttonRemoveRow.IsEnabled = PokeManager.ManagerGameSave.NumPokePCRows > 1;
			}
			buttonMoveRowDown.IsEnabled = rowIndex != 0;
			buttonMoveRowUp.IsEnabled = rowIndex + 1 < PokeManager.ManagerGameSave.NumPokePCRows;
		}

		private void OnRowSelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (comboBoxRows.SelectedIndex != -1) {
				rowIndex = comboBoxRows.SelectedIndex;
				pokePC = PokeManager.ManagerGameSave.GetPokePCRow(rowIndex);
				PopulateBoxes();
				buttonMoveRowUp.IsEnabled = rowIndex != 0;
				buttonMoveRowDown.IsEnabled = rowIndex + 1 < PokeManager.ManagerGameSave.NumPokePCRows;
			}
		}

		private void OnMoveRowDown(object sender, RoutedEventArgs e) {
			if (rowIndex + 1 < PokeManager.ManagerGameSave.NumPokePCRows) {
				PokeManager.ManagerGameSave.MoveRowDown(rowIndex);
				comboBoxRows.RefreshRows();
				comboBoxRows.SelectedIndex++;
				buttonMoveRowUp.IsEnabled = rowIndex != 0;
				buttonMoveRowDown.IsEnabled = rowIndex + 1 < PokeManager.ManagerGameSave.NumPokePCRows;
			}
		}

		private void OnMoveRowUp(object sender, RoutedEventArgs e) {
			if (rowIndex > 0) {
				PokeManager.ManagerGameSave.MoveRowUp(rowIndex);
				comboBoxRows.RefreshRows();
				comboBoxRows.SelectedIndex--;
				buttonMoveRowUp.IsEnabled = rowIndex != 0;
				buttonMoveRowDown.IsEnabled = rowIndex + 1 < PokeManager.ManagerGameSave.NumPokePCRows;
			}
		}
	}
}
