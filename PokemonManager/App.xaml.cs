using PokemonManager.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PokemonManager {
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {

		Mutex m;
		public App() {

			bool isnew;
			m = new Mutex(true, "Global\\" + appGuid, out isnew);
			if (!isnew) {
				TriggerMessageBox.Show(null, "Cannot run more than one instance of Trigger's PC at a time.");
				Environment.Exit(0);
			}
		}

		private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
			if (ErrorMessageBox.Show(e.Exception)) {
				e.Handled = true;
				Environment.Exit(0);
			}
		}

		private static string appGuid = "d527114e-3888-417f-807f-e08fef41029c";
	}
}
