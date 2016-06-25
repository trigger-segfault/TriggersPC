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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for MyMessageBox.xaml
	/// </summary>
	public partial class ErrorMessageBox : Window {

		private Exception exception;
		private bool viewingFull;

		public ErrorMessageBox(Exception exception) {
			InitializeComponent();

			this.textBlockMessage.Text = "Exception:\n" + exception.Message;
			this.exception = exception;
			this.viewingFull = false;
		}

		public static bool Show(Exception exception) {
			ErrorMessageBox messageBox = new ErrorMessageBox(exception);
			var result = messageBox.ShowDialog();
			return result.HasValue && result.Value;
		}

		private void OnWindowLoaded(object sender, RoutedEventArgs e) {
			this.Height += textBlockMessage.ActualHeight - 16;
		}

		private void OnContinue(object sender, RoutedEventArgs e) {
			Close();
		}

		private void OnCopyToClipboard(object sender, RoutedEventArgs e) {
			Clipboard.SetText(exception.ToString());
		}

		private void OnSeeFullException(object sender, RoutedEventArgs e) {
			if (viewingFull) {
				buttonException.Content = "See Full Exception";
				textBlockMessage.Text = "Exception:\n" + exception.Message;
				Height = 250;
			}
			else {
				buttonException.Content = "Hide Full Exception";
				textBlockMessage.Text = "Exception:\n" + exception.ToString();
				Height = 500;
			}
			viewingFull = !viewingFull;
		}

		private void OnRequestNavigate(object sender, RequestNavigateEventArgs e) {
			System.Diagnostics.Process.Start((sender as Hyperlink).NavigateUri.ToString());
		}

		private void OnClose(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}
	}
}
