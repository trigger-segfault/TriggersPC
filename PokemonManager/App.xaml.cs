using PokemonManager.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Resources;
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
			if (ErrorMessageBox.Show(e.Exception))
				Environment.Exit(0);
			e.Handled = true;
		}

		private void OnApplicationStartup(object sender, StartupEventArgs e) {
			SplashScreen screen;
			string[] pokeSplashes = {
				"Cubone",
				"Electabuzz",
				"Gengar",
				"Kecleon",
				"Magikarp",
				"Rayquaza",
				"Ursaring",
				"Voltorb"
			};
			Random random = new Random((int)DateTime.Now.Ticks);
			if ((DateTime.Now.Month == 4 && DateTime.Now.Day == 1))
				screen = new SplashScreen("Resources/Splash/SplashMagikarp.png");
			else if (random.Next(10) == 0)
				screen = new SplashScreen("Resources/Splash/Splash" + pokeSplashes[random.Next(pokeSplashes.Length)] + ".png");
			else
				screen = new SplashScreen("Resources/Splash/Splash.png");
			screen.Show(true);
		}

		private static string appGuid = "d527114e-3888-417f-807f-e08fef41029c";
	}
}
