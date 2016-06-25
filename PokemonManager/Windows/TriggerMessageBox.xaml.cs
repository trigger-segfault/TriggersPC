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
	/// <summary>
	/// Interaction logic for MyMessageBox.xaml
	/// </summary>
	public partial class TriggerMessageBox : Window {

		private MessageBoxResult result;
		private int minWidth;
		private MessageBoxButton buttons;

		public TriggerMessageBox(string title, string message, MessageBoxButton buttons, string buttonName1 = null, string buttonName2 = null, string buttonName3 = null) {
			InitializeComponent();

			this.buttons = buttons;
			minWidth = 280;
			switch (buttons) {
			case MessageBoxButton.OK:
				button1.Visibility = Visibility.Hidden;
				button2.Visibility = Visibility.Hidden;
				button3.IsDefault = true;
				button3.Content = "OK";
				button3.Tag = MessageBoxResult.OK;
				minWidth -= 85 * 2;
				result = MessageBoxResult.OK;
				if (buttonName1 != null)
					button3.Content = buttonName1;
				break;
			case MessageBoxButton.OKCancel:
				button1.Visibility = Visibility.Hidden;
				button2.IsDefault = true;
				button2.Content = "OK";
				button2.Tag = MessageBoxResult.OK;
				button3.IsCancel = true;
				button3.Content = "Cancel";
				button3.Tag = MessageBoxResult.Cancel;
				minWidth -= 85;
				result = MessageBoxResult.Cancel;
				if (buttonName1 != null)
					button2.Content = buttonName1;
				if (buttonName2 != null)
					button3.Content = buttonName2;
				break;
			case MessageBoxButton.YesNo:
				button1.Visibility = Visibility.Hidden;
				button2.IsDefault = true;
				button2.Content = "Yes";
				button2.Tag = MessageBoxResult.Yes;
				button3.IsCancel = true;
				button3.Content = "No";
				button3.Tag = MessageBoxResult.No;
				minWidth -= 85;
				result = MessageBoxResult.No;
				if (buttonName1 != null)
					button2.Content = buttonName1;
				if (buttonName2 != null)
					button3.Content = buttonName2;
				break;
			case MessageBoxButton.YesNoCancel:
				button1.IsDefault = true;
				button1.Content = "Yes";
				button1.Tag = MessageBoxResult.Yes;
				button2.Content = "No";
				button2.Tag = MessageBoxResult.No;
				button3.IsCancel = true;
				button3.Content = "Cancel";
				button3.Tag = MessageBoxResult.Cancel;
				result = MessageBoxResult.Cancel;
				if (buttonName1 != null)
					button1.Content = buttonName1;
				if (buttonName2 != null)
					button2.Content = buttonName2;
				if (buttonName3 != null)
					button3.Content = buttonName3;
				break;
			}


			this.Title = title;
			this.textBlockMessage.Text = message;
		}

		public static MessageBoxResult Show(Window window, string message) {
			return Show(window, message, "", MessageBoxButton.OK);
		}
		public static MessageBoxResult Show(Window window, string message, string title) {
			return Show(window, message, title, MessageBoxButton.OK);
		}
		public static MessageBoxResult Show(Window window, string message, MessageBoxButton buttons) {
			return Show(window, message, "", buttons);
		}
		public static MessageBoxResult Show(Window window, string message, string title, MessageBoxButton buttons, string buttonName1 = null, string buttonName2 = null, string buttonName3 = null) {
			TriggerMessageBox messageBox = new TriggerMessageBox(title, message, buttons, buttonName1, buttonName2, buttonName3);
			if (window == null || window.Visibility != Visibility.Visible)
				messageBox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			else
				messageBox.Owner = window;
			messageBox.ShowDialog();
			return messageBox.result;
		}

		private void OnButtonClicked(object sender, RoutedEventArgs e) {
			result = (MessageBoxResult)((Button)sender).Tag;
			Close();
		}
		private void OnWindowLoaded(object sender, RoutedEventArgs e) {
			this.Width = Math.Max(minWidth, textBlockMessage.ActualWidth + 60);
			this.Height += textBlockMessage.ActualHeight - 16;
		}
	}
}
