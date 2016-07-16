using PokemonManager.PokemonStructures;
using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PokemonManager.Windows {
	/// <summary>
	/// Interaction logic for EvolutionWindow.xaml
	/// </summary>
	public partial class EvolutionWindow : Window {
		private readonly double[] frameArray = new double[] { 25, 25, 25, 24, 24, 24, 23, 23, 22, 21, 19, 16, 15, 13, 12, 11, 11, 10, 9, 9, 9, 8, 7, 7, 7, 7, 7, 6, 6, 6, 6, 6, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 1 };
		//private readonly double[] frameArrayREAL = new double[] { 25, 21, 19, 16, 15, 13, 12, 11, 11, 10, 9, 9, 9, 8, 7, 7, 7, 7, 7, 6, 6, 6, 6, 6, 6, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 1 };

		// Animation
		private DispatcherTimer timer;
		private Storyboard storyboard;
		private MediaPlayer playerCry;
		private MediaPlayer playerEvolutionCry;
		private MediaPlayer playerMusic;
		private int evolutionState;

		// Evolution
		private IPokemon pokemon;
		private ushort evolutionDexID;
		private bool evolved;
		private bool shedinjaAdded;

		public EvolutionWindow(IPokemon pokemon, ushort evolutionDexID) {
			InitializeComponent();

			this.pokemon = pokemon;
			this.evolutionDexID = evolutionDexID;
			this.evolved = false;

			this.evolutionState = 0;

			this.timer = new DispatcherTimer();
			this.timer.Tick += OnTick;
			this.timer.Interval = TimeSpan.FromSeconds(1.5);
			this.timer.Start();

			this.storyboard = new Storyboard();
			this.storyboard.Completed += OnStoryboardCompleted;
			this.imagePrevolution.Source = PokemonDatabase.GetPokemonImageFromDexID(pokemon.DexID, pokemon.IsShiny);
			this.imageEvolution.Source = PokemonDatabase.GetPokemonImageFromDexID(evolutionDexID, pokemon.IsShiny);
			this.rectMaskPrevolution.OpacityMask = new ImageBrush(PokemonDatabase.GetPokemonImageFromDexID(pokemon.DexID, pokemon.IsShiny));
			this.rectMaskEvolution.OpacityMask = new ImageBrush(PokemonDatabase.GetPokemonImageFromDexID(evolutionDexID, pokemon.IsShiny));
			this.rectMaskPrevolution.Opacity = 0;
			this.textBlockMessage.Text = "What?\nYour " + pokemon.Nickname + " is evolving!";
			playerCry = new MediaPlayer();
			playerCry.MediaEnded += OnMediaEnded;
			playerCry.Volume = PokeManager.Settings.MutedVolume;
			playerEvolutionCry = new MediaPlayer();
			playerEvolutionCry.MediaEnded += OnMediaEnded;
			playerEvolutionCry.Volume = PokeManager.Settings.MutedVolume;
			playerMusic = new MediaPlayer();
			playerMusic.MediaEnded += OnMediaEnded;
			playerMusic.Volume = PokeManager.Settings.MutedVolume;

			if (PokeManager.Settings.IsMuted)
				imageVolume.Source = ResourceDatabase.GetImageFromName("IconVolumeMute");
			else
				imageVolume.Source = ResourceDatabase.GetImageFromName("IconVolumeOn");

			CreateAnimation();
			string cryFile = PokemonDatabase.FindCryFile(pokemon.DexID);
			try {
				if (cryFile != null)
					playerCry.Open(new Uri(cryFile));
				else
					playerCry = null;
			} catch (Exception) {
				playerCry = null;
			}
			cryFile = PokemonDatabase.FindCryFile(evolutionDexID);
			try {
				if (cryFile != null)
					playerEvolutionCry.Open(new Uri(cryFile));
				else
					playerEvolutionCry = null;
			} catch (Exception) {
				playerEvolutionCry = null;
			}
			playerMusic.Open(new Uri(System.IO.Path.Combine(PokeManager.ApplicationDirectory, "Resources", "Audio", "Evolution.wav")));
		}

		public static bool ShowDialog(Window window, IPokemon pokemon, ushort evolutionDexID) {
			EvolutionWindow form = new EvolutionWindow(pokemon, evolutionDexID);
			form.Owner = window;
			form.ShowDialog();
			return form.evolved;
		}

		private void FinishEvolution() {
			evolved = true;

			storyboard.Stop(this);
			if (playerCry != null)
				playerCry.Stop();
			playerMusic.Stop();
			evolutionState = 4;

			// Make sure everything looks correct
			imagePrevolution.Visibility = Visibility.Hidden;
			rectMaskPrevolution.Visibility = Visibility.Hidden;
			rectMaskEvolution.Visibility = Visibility.Hidden;
			rectGlow.Visibility = Visibility.Hidden;
			rectBlackCover.Visibility = Visibility.Hidden;
			((imageEvolution.RenderTransform as TransformGroup).Children[1] as ScaleTransform).ScaleX = 1;
			((imageEvolution.RenderTransform as TransformGroup).Children[1] as ScaleTransform).ScaleY = 1;

			// Setup correct controls
			this.textBlockMessage.Text = "Congratulations!\nYour " + pokemon.Nickname + " evolved into " + EvolutionData.Name + "!";
			this.gridControlButtons.Visibility = Visibility.Hidden;
			this.buttonClose.Visibility = Visibility.Visible;
			if (!pokemon.HasNickname)
				pokemon.Nickname = EvolutionData.Name.ToUpper();
			pokemon.DexID = evolutionDexID;
			pokemon.RecalculateStats();

			if (evolutionDexID == 291) {
				IPokemon shedinja = pokemon.Clone();
				shedinja.DexID = 292;
				shedinja.RemoveNickname();
				shedinja.RecalculateStats();
				if (pokemon.PokePC.HasRoomForPokemon(1)) {
					shedinjaAdded = true;
					pokemon.PokePC.PlacePokemonInNextAvailableSlot(pokemon.PokeContainer is IPokeBox ? (int)(pokemon.PokeContainer as IPokeBox).BoxNumber : -1, pokemon.ContainerIndex, shedinja);
				}
			}
			pokemon.GameSave.IsChanged = true;
			PokeManager.RefreshUI();
		}

		private PokemonData EvolutionData {
			get { return PokemonDatabase.GetPokemonFromDexID(evolutionDexID); }
		}

		private void OnVolumeClicked(object sender, RoutedEventArgs e) {
			PokeManager.Settings.IsMuted = !PokeManager.Settings.IsMuted;
			if (PokeManager.Settings.IsMuted)
				imageVolume.Source = ResourceDatabase.GetImageFromName("IconVolumeMute");
			else
				imageVolume.Source = ResourceDatabase.GetImageFromName("IconVolumeOn");
			if (playerCry != null)
				playerCry.Volume = PokeManager.Settings.MutedVolume;
			if (playerEvolutionCry != null)
				playerEvolutionCry.Volume = PokeManager.Settings.MutedVolume;
			playerMusic.Volume = PokeManager.Settings.MutedVolume;
		}

		private void OnSkipClicked(object sender, RoutedEventArgs e) {
			FinishEvolution();
		}
		private void OnCancelClicked(object sender, RoutedEventArgs e) {
			evolutionState = 4;
			if (playerCry != null)
				playerCry.Stop();
			playerMusic.Stop();
			storyboard.Stop(this);

			// Make sure everything looks right
			imageEvolution.Visibility = Visibility.Hidden;
			rectMaskPrevolution.Visibility = Visibility.Hidden;
			rectMaskEvolution.Visibility = Visibility.Hidden;
			rectGlow.Visibility = Visibility.Hidden;
			rectBlackCover.Visibility = Visibility.Hidden;
			((imagePrevolution.RenderTransform as TransformGroup).Children[1] as ScaleTransform).ScaleX = 1;
			((imagePrevolution.RenderTransform as TransformGroup).Children[1] as ScaleTransform).ScaleY = 1;

			// Setup correct controls
			this.textBlockMessage.Text = "What?\n" + pokemon.Nickname + " stopped evolving!";
			this.gridControlButtons.Visibility = Visibility.Hidden;
			this.buttonClose.Visibility = Visibility.Visible;
			
			// Let us here the cries of sorrow from our pokemon again
			if (playerCry != null)
				playerCry.Play();
		}
		private void OnCloseClicked(object sender, RoutedEventArgs e) {
			Close();
		}

		#region Progression

		private void HandleEvolutionState() {
			if (evolutionState == 0) {
				if (playerCry != null) {
					playerCry.Play();
					timer.Interval = TimeSpan.FromSeconds(2);
					timer.Start();
				}
				else {
					evolutionState = 1;
				}
			}
			if (evolutionState == 1) {
				timer.Stop();
				storyboard.Begin(this, true);
				playerMusic.Play();
			}
			if (evolutionState == 2) {
				if (playerEvolutionCry != null) {
					playerEvolutionCry.Play();
				}
				else {
					timer.Interval = TimeSpan.FromSeconds(1);
					timer.Start();
				}
			}
			if (evolutionState == 3) {
				// Play congrats sound once we have one
				FinishEvolution();
			}

			evolutionState++;
		}
		private void OnTick(object sender, EventArgs e) {
			timer.Stop();
			HandleEvolutionState();
		}
		private void OnStoryboardCompleted(object sender, EventArgs e) {
			HandleEvolutionState();
		}

		private void OnMediaEnded(object sender, EventArgs e) {
			HandleEvolutionState();
		}

		#endregion

		#region Making Animation

		private void CreateAnimation() {
			double divisor = 32.5;// 45.0 * 100.0 / 180.0 * 130.0 / 100.0;
			double beginTime = 0;
			double seconds = 0;
			double radialDuration = 0;
			double radialBegin = 0;

			beginTime += 1.6;

			seconds = 38.0 / divisor;
			AddAnimation(beginTime, rectBlackCover, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(seconds)), UIElement.OpacityProperty);
			beginTime += seconds;

			seconds = (72.0 + 20) / divisor;
			AddAnimation(beginTime, rectMaskPrevolution, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(seconds)), UIElement.OpacityProperty);

			radialBegin = beginTime + 40.0 / divisor;
			beginTime += seconds;

			beginTime += (170.0 - 30) / divisor;

			for (int animationIndex = 0; animationIndex < frameArray.Length; animationIndex++) {
				seconds = frameArray[animationIndex] / divisor;
				double small = 0.0;
				double a = (animationIndex % 2 == 0 ? 1 : small);
				double b = (animationIndex % 2 == 0 ? small : 1);
				MakeScaleAnimation(beginTime, a, (animationIndex == frameArray.Length - 1 ? 0 : b), seconds, "X", imagePrevolution);
				MakeScaleAnimation(beginTime, a, (animationIndex == frameArray.Length - 1 ? 0 : b), seconds, "Y", imagePrevolution);
				MakeScaleAnimation(beginTime, a, (animationIndex == frameArray.Length - 1 ? 0 : b), seconds, "X", rectMaskPrevolution);
				MakeScaleAnimation(beginTime, a, (animationIndex == frameArray.Length - 1 ? 0 : b), seconds, "Y", rectMaskPrevolution);
				MakeScaleAnimation(beginTime, (animationIndex == 0 ? 0 : b), a, seconds, "X", imageEvolution);
				MakeScaleAnimation(beginTime, (animationIndex == 0 ? 0 : b), a, seconds, "Y", imageEvolution);
				MakeScaleAnimation(beginTime, (animationIndex == 0 ? 0 : b), a, seconds, "X", rectMaskEvolution);
				MakeScaleAnimation(beginTime, (animationIndex == 0 ? 0 : b), a, seconds, "Y", rectMaskEvolution);
				beginTime += seconds;
			}

			radialDuration = beginTime - radialBegin + 20.0 / divisor;
			MakeRadialLoopAnimation(0, 0, 1, radialDuration, radialBegin);
			MakeRadialLoopAnimation(1, 0, 1, radialDuration, radialBegin);
			MakeRadialLoopAnimation(2, 0, 1, radialDuration, radialBegin);
			MakeRadialLoopAnimation(3, 0, 1, radialDuration, radialBegin);
			MakeRadialLoopAnimation(4, 0, 1, radialDuration, radialBegin);
			MakeRadialLoopAnimation(5, 0, 1, radialDuration, radialBegin);

			seconds = 15.0 / divisor;
			AddAnimation(beginTime, rectGlow, new DoubleAnimation(1, 0, TimeSpan.FromSeconds(seconds)), UIElement.OpacityProperty);

			beginTime += 60.0 / divisor;

			seconds = 15.0 / divisor;
			AddAnimation(beginTime, rectFlash, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(seconds)), UIElement.OpacityProperty);
			beginTime += seconds + 34.0 / divisor;
			seconds = 90.0 / divisor;
			AddAnimation(beginTime, rectFlash, new DoubleAnimation(1, 0, TimeSpan.FromSeconds(seconds)), UIElement.OpacityProperty);
			beginTime += 10.0 / divisor;
			seconds = 25.0 / divisor;
			AddAnimation(beginTime, rectMaskEvolution, new DoubleAnimation(1, 0, TimeSpan.FromSeconds(seconds)), UIElement.OpacityProperty);

			beginTime += 10.0 / divisor;
			seconds = 16.0 / divisor;
			AddAnimation(beginTime, rectBlackCover, new DoubleAnimation(1, 0, TimeSpan.FromSeconds(seconds)), UIElement.OpacityProperty);
		}
		private void AddAnimation(double beginTime, UIElement element, AnimationTimeline anim, DependencyProperty property) {
			anim.BeginTime = TimeSpan.FromSeconds(beginTime);
			Storyboard.SetTarget(anim, element);
			Storyboard.SetTargetProperty(anim, new PropertyPath(property));
			storyboard.Children.Add(anim);
		}
		private void MakeScaleAnimation(double beginTime, double start, double end, double seconds, string xy, UIElement element) {
			DoubleAnimation anim = new DoubleAnimation(start, end, TimeSpan.FromSeconds(seconds));
			anim.BeginTime = TimeSpan.FromSeconds(beginTime);
			Storyboard.SetTarget(anim, element);
			Storyboard.SetTargetProperty(anim, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(ScaleTransform.Scale" + xy + ")"));
			storyboard.Children.Add(anim);
		}
		private void MakeRadialLoopAnimation(int index, double start, double stop, double duration, double begin) {
			double expand = 0.02;
			double[] scales1 = new double[] { 0, 0.0025 + expand / 2, 0.00425 + expand, 0.00525 + expand, 0.0070 + expand * 1.5, 0.0095 + expand * 3 };
			double[] scales2 = new double[] { 0, 0.0025 + expand / 2, 0.00425 + expand, 0.00525 + expand, 0.0070 + expand * 1.5, 0.0095 + expand * 3 };
			DoubleAnimation anim = new DoubleAnimation(Math.Max(0, 0 + scales1[index] * 100 - scales1[5] * 100), (1 + scales2[index] * 100) / 2, TimeSpan.FromSeconds(120.0 / 60.0));
			anim.RepeatBehavior = new RepeatBehavior(TimeSpan.FromSeconds(duration));
			anim.BeginTime = TimeSpan.FromSeconds(begin);
			anim.AccelerationRatio = 0.3;
			Storyboard.SetTarget(anim, rectGlow);
			Storyboard.SetTargetProperty(anim, new PropertyPath("(Rectangle.Fill).(GradientBrush.GradientStops)[" + index + "].(GradientStop.Offset)"));
			storyboard.Children.Add(anim);
		}

		#endregion

		private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
			if (playerCry != null)
				playerCry.Close();
			if (playerEvolutionCry != null)
				playerEvolutionCry.Close();
			playerMusic.Close();

			ushort[] moves = PokemonDatabase.GetMovesLearnedAtLevel(pokemon);
			foreach (ushort moveID in moves) {
				Move move = new Move(moveID);
				if (pokemon.NumMoves == 4) {
					var result = LearnMoveWindow.ShowDialog(Window.GetWindow(this), pokemon, move.ID);
					if (result.HasValue && result.Value) {
						TriggerMessageBox.Show(this, pokemon.Nickname + " learned " + move.MoveData.Name + "!", "Move Learned");
						PokeManager.RefreshUI();
					}
				}
				else {
					pokemon.SetMoveAt(pokemon.NumMoves, move);
					TriggerMessageBox.Show(this, pokemon.Nickname + " learned " + move.MoveData.Name + "!", "Move Learned");
					PokeManager.RefreshUI();
				}
			}

			if (shedinjaAdded) {
				TriggerMessageBox.Show(PokeManager.ManagerWindow, "A Shedinja has appeared from the shed exoskeleton of " + pokemon.Nickname, "New Pokémon");
			}
		}
	}
}
