using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for NumericUpDown.xaml
	/// </summary>
	public partial class NumericUpDown : UserControl {

		int errorValue;
		int number;
		int maximum;
		int minimum;
		int increment;

		public NumericUpDown() {
			InitializeComponent();
			this.errorValue = 0;
			this.number = 1;
			this.minimum = 0;
			this.maximum = 10000;
			this.increment = 1;

			UpdateSpinner();
			UpdateTextBox();

			textBox.Focusable = true;
		}
		public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumericUpDown));

		public event RoutedEventHandler ValueChanged;

		public int ErrorValue {
			get { return errorValue; }
			set {
				if (value < minimum || value > maximum)
					throw new IndexOutOfRangeException("NumericUpDown ErrorValue outside of Minimum or Maximum range.");
				errorValue = value;
			}
		}
		public int Value {
			get { return number; }
			set {
				if (value < minimum || value > maximum)
					throw new IndexOutOfRangeException("NumericUpDown Value outside of Minimum or Maximum range.");
				if (number != value) {
					number = value;
					UpdateSpinner();
					UpdateTextBox();
					RaiseEvent(new RoutedEventArgs(NumericUpDown.ValueChangedEvent));
				}
			}
		}
		public int Minimum {
			get { return minimum; }
			set {
				if (value > maximum)
					throw new IndexOutOfRangeException("NumericUpDown Minimum is greater than Maximum range.");
				minimum = value;
				if (number < minimum)
					Value = minimum;
				else
					UpdateSpinner();
				if (errorValue < minimum)
					errorValue = minimum;
			}
		}
		public int Maximum {
			get { return maximum; }
			set {
				if (value < minimum)
					throw new IndexOutOfRangeException("NumericUpDown Maximum is less than Minimum range.");
				maximum = value;
				if (number > maximum)
					Value = maximum;
				else
					UpdateSpinner();
				if (errorValue > maximum)
					errorValue = maximum;
			}
		}
		public int Increment {
			get { return increment; }
			set { increment = value; }
		}
		public string Text {
			get { return textBox.Text; }
		}
		private void UpdateSpinner() {
			spinner.ValidSpinDirection = ValidSpinDirections.None;
			spinner.ValidSpinDirection |= (number != maximum ? ValidSpinDirections.Increase : ValidSpinDirections.None);
			spinner.ValidSpinDirection |= (number != minimum ? ValidSpinDirections.Decrease : ValidSpinDirections.None);
		}
		private void UpdateTextBox() {
			int caretIndex = textBox.CaretIndex;
			textBox.Text = number.ToString();
			textBox.CaretIndex = Math.Min(textBox.Text.Length, caretIndex);
			UpdateTextBoxError();
		}
		private void UpdateTextBox(string newText, int newCaretIndex) {
			int caretIndex = textBox.CaretIndex;
			textBox.Text = newText;
			textBox.CaretIndex = Math.Min(textBox.Text.Length, newCaretIndex);
			UpdateTextBoxError();
		}
		private void UpdateTextBoxError() {
			bool error = false;
			try {
				int newNum = int.Parse(Text);
				if (newNum > maximum || newNum < minimum) {
					error = true;
				}
			}
			catch (OverflowException) {
				error = true;
			}
			catch (FormatException) {
				error = true;
			}
			if (error)
				textBox.Foreground = new SolidColorBrush(Color.FromRgb(220, 0, 0));
			else
				textBox.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
		}

		public void SelectAll() {
			textBox.Focusable = true;
			textBox.Focus();
			textBox.SelectAll();
		}

		private void OnTextInput(object sender, TextCompositionEventArgs e) {
			int oldValue = number;
			TextBox textBox = sender as TextBox;
			e.Handled = true;
			bool invalidChar = false;
			for (int i = 0; i < e.Text.Length && !invalidChar; i++) {
				if ((e.Text[i] < '0' || e.Text[i] > '9') && (e.Text[i] != '-' || textBox.CaretIndex != 0 || i > 0 || minimum >= 0))
					invalidChar = true;
			}
			if (!invalidChar) {
				string newText = "";
				if (textBox.SelectionLength != 0)
					newText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength).Insert(textBox.SelectionStart, e.Text);
				else
					newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
				try {
					number = int.Parse(newText);
					if (number > maximum) {
						number = maximum;
						UpdateTextBox();
						textBox.CaretIndex += e.Text.Length;
					}
					else if (number < minimum) {
						number = minimum;
						UpdateTextBox(newText, textBox.CaretIndex + e.Text.Length);
					}
					else {
						UpdateTextBox(newText, textBox.CaretIndex + e.Text.Length);
					}
				}
				catch (OverflowException) {
					if (textBox.Text.Length > 0 && textBox.Text[0] == '-') {
						//Underflow
						number = minimum;
						UpdateTextBox();
						textBox.CaretIndex = textBox.Text.Length;
					}
					else {
						number = maximum;
						UpdateTextBox();
						textBox.CaretIndex = textBox.Text.Length;
					}
				}
				catch (FormatException) {
					if (newText == "-" || newText == "") {
						// Don't worry, the user is just writing a negative number or typing a new number
						number = errorValue;
						UpdateTextBox(newText, textBox.CaretIndex + e.Text.Length);
					}
					else {
						// Shouldn't happen?
						number = errorValue;
						UpdateTextBox();
						textBox.CaretIndex = textBox.Text.Length;
					}
				}
				if (number != oldValue) {
					UpdateSpinner();
					RaiseEvent(new RoutedEventArgs(NumericUpDown.ValueChangedEvent));
				}
			}
			UpdateTextBoxError();
		}

		private void OnTextChanged(object sender, TextChangedEventArgs e) {
			int oldValue = number;
			try {
				number = int.Parse(textBox.Text);
				if (number > maximum) {
					number = maximum;
					UpdateTextBox();
					textBox.CaretIndex = textBox.Text.Length;
				}
				else if (number < minimum) {
					number = minimum;
				}
			}
			catch (OverflowException) {
				if (textBox.Text.Length > 0 && textBox.Text[0] == '-') {
					//Underflow
					number = minimum;
					UpdateTextBox();
					textBox.CaretIndex = textBox.Text.Length;
				}
				else {
					number = maximum;
					UpdateTextBox();
					textBox.CaretIndex = textBox.Text.Length;
				}
			}
			catch (FormatException) {
				if (textBox.Text == "-" || textBox.Text == "") {
					// Don't worry, the user is just writing a negative number or typing a new number
					number = errorValue;
				}
				else {
					number = errorValue;
					UpdateTextBox();
					textBox.CaretIndex = textBox.Text.Length;
				}
			}
			if (number != oldValue) {
				UpdateSpinner();
				RaiseEvent(new RoutedEventArgs(NumericUpDown.ValueChangedEvent));
			}
			
			UpdateTextBoxError();
		}

		private void OnSpinnerSpin(object sender, SpinEventArgs e) {
			int oldValue = number;
			if (e.Direction == SpinDirection.Increase)
				number = Math.Min(maximum, number + increment);
			else if (e.Direction == SpinDirection.Decrease)
				number = Math.Max(minimum, number - increment);
			if (number != oldValue) {
				UpdateSpinner();
				UpdateTextBox();
				textBox.CaretIndex = textBox.Text.Length;
				RaiseEvent(new RoutedEventArgs(NumericUpDown.ValueChangedEvent));
			}
		}

		private void OnFocusLost(object sender, RoutedEventArgs e) {
			UpdateTextBox();
		}
	}
}
