using PokemonManager.Items;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for BuySellInfoControl.xaml
	/// </summary>
	public partial class BuySellInfoControl : UserControl {
		public BuySellInfoControl() {
			InitializeComponent();

			this.FontWeight = FontWeights.Bold;
			this.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));

			if (DesignerProperties.GetIsInDesignMode(this)) {
				AddPrice(false, true, "$9,999");
				AddPrice(false, false, "100 Coins");
				AddPrice(false, false, "1,000 BP");
				AddSellPrice(false, "$4,499");
			}
		}

		public void LoadBuySellInfo(ItemData item) {
			this.stackPanelContents.Children.Clear();

			bool buyLabelSet = false;
			if (item.Price != 0) {
				AddPrice(false, !buyLabelSet, "$" + item.Price.ToString("#,0"));
				buyLabelSet = true;
			}
			if (item.CoinsPrice != 0) {
				AddPrice(false, !buyLabelSet, item.CoinsPrice.ToString("#,0") + " Coins");
				buyLabelSet = true;
			}
			if (item.BattlePointsPrice != 0) {
				AddPrice(false, !buyLabelSet, item.BattlePointsPrice.ToString("#,0") + " BP");
				buyLabelSet = true;
			}
			if (item.PokeCouponsPrice != 0) {
				AddPrice(false, !buyLabelSet, item.PokeCouponsPrice.ToString("#,0") + " PC");
				buyLabelSet = true;
			}
			if (item.VolcanicAshPrice != 0) {
				AddPrice(false, !buyLabelSet, item.VolcanicAshPrice.ToString("#,0") + " Soot");
				buyLabelSet = true;
			}
			if (!buyLabelSet)
				AddPrice(true);
			if (item.SellPrice != 0)
				AddSellPrice(false, "$" + item.SellPrice.ToString("#,0"));
			else
				AddSellPrice(true);
		}

		public void LoadBuySellInfo(DecorationData decoration) {
			this.stackPanelContents.Children.Clear();

			bool buyLabelSet = false;
			if (decoration.Price != 0) {
				AddPrice(false, !buyLabelSet, "$" + decoration.Price.ToString("#,0"), decoration.IsOnlyPurchasableDuringSale);
				buyLabelSet = true;
			}
			if (decoration.CoinsPrice != 0) {
				AddPrice(false, !buyLabelSet, decoration.CoinsPrice.ToString("#,0") + " Coins");
				buyLabelSet = true;
			}
			if (decoration.BattlePointsPrice != 0) {
				AddPrice(false, !buyLabelSet, decoration.BattlePointsPrice.ToString("#,0") + " BP");
				buyLabelSet = true;
			}
			if (decoration.VolcanicAshPrice != 0) {
				AddPrice(false, !buyLabelSet, decoration.VolcanicAshPrice.ToString("#,0") + " Soot");
				buyLabelSet = true;
			}
			if (!buyLabelSet)
				AddPrice(true);

			AddSellPrice(true);
		}


		public void UnloadBuySellInfo() {
			this.stackPanelContents.Children.Clear();
		}

		public void AddPrice(bool failed, bool buy = true, string price = "", bool sale = false) {
			Grid grid = new Grid();
			grid.Height = 20;
			if (buy) {
				Label labelBuy = new Label();
				labelBuy.Content = (failed ? "Can't be Bought" : "Buy");
				labelBuy.Padding = new Thickness(5, 2, 5, 2);
				//labelBuy.Foreground = this.Foreground;
				//labelBuy.FontWeight = this.FontWeight;
				grid.Children.Add(labelBuy);
			}
			if (price != "") {
				Label labelPrice = new Label();
				labelPrice.Content = price + (sale ? " (Sale)" : "");
				labelPrice.Padding = new Thickness(5, 2, 5, 2);
				//labelPrice.Foreground = this.Foreground;
				//labelPrice.FontWeight = this.FontWeight;
				labelPrice.HorizontalContentAlignment = HorizontalAlignment.Right;
				grid.Children.Add(labelPrice);
			}
			stackPanelContents.Children.Add(grid);
		}
		public void AddSellPrice(bool failed, string price = "") {
			Separator separator = new Separator();
			separator.Height = 7;
			separator.Margin = new Thickness(4, 0, 4, 0);
			separator.Background = this.Foreground;
			stackPanelContents.Children.Add(separator);

			Grid grid = new Grid();
			grid.Height = 20;
			Label labelSell = new Label();
			labelSell.Content = (failed ? "Can't be Sold" : "Sell");
			labelSell.Padding = new Thickness(5, 2, 5, 2);
			//labelSell.Foreground = this.Foreground;
			//labelSell.FontWeight = this.FontWeight;
			grid.Children.Add(labelSell);
			if (price != "") {
				Label labelPrice = new Label();
				labelPrice.Content = price;
				labelPrice.Padding = new Thickness(5, 2, 5, 2);
				//labelPrice.Foreground = this.Foreground;
				//labelPrice.FontWeight = this.FontWeight;
				labelPrice.HorizontalContentAlignment = HorizontalAlignment.Right;
				grid.Children.Add(labelPrice);
			}
			stackPanelContents.Children.Add(grid);
		}
	}
}
