using PokemonManager.Game;
using PokemonManager.Game.FileStructure;
using PokemonManager.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonManager.PokemonStructures.Events {
	public class PokemonEventDistribution : EventDistribution {

		protected IPokemon reward;

		public ushort DexID { get; set; }
		public bool IsEgg { get; set; }
		public byte Level { get; set; }
		public bool? IsSecondAbility { get; set; }
		public string Nickname { get; set; }

		public string TrainerName { get; set; }
		public Genders? TrainerGender { get; set; }
		public ushort? TrainerID { get; set; }
		public ushort? SecretID { get; set; }

		// Personality
		public uint? Personality { get; set; }
		public bool? IsShiny { get; set; }
		public Genders? Gender { get; set; }
		public byte? NatureID { get; set; }

		public ushort[] HeldItem { get; set; }
		public byte[] Conditions { get; set; }
		public byte[] IVs { get; set; }
		public byte? HiddenPowerDamage { get; set; }
		public PokemonTypes? HiddenPowerType { get; set; }

		public ushort? Move1ID { get; set; }
		public ushort? Move2ID { get; set; }
		public ushort? Move3ID { get; set; }
		public ushort? Move4ID { get; set; }

		public CheckEventRequirementsDelegate CheckRequirements { get; set; }

		public PokemonEventDistribution(string id) : base(id) {
			this.DexID = 1;
			this.Level = 5;
		}

		public override EventRewardTypes RewardType {
			get { return EventRewardTypes.Pokemon; }
		}
		public override void GenerateReward(IGameSave gameSave) {
			Random random = new Random((int)DateTime.Now.Ticks);
			PokemonData pokemonData = PokemonDatabase.GetPokemonFromDexID(DexID);
			GBAPokemon pkm = new GBAPokemon();

			pkm.DexID = DexID;
			pkm.Personality = (Personality.HasValue ? Personality.Value : (uint)random.Next());
			pkm.Experience = PokemonDatabase.GetExperienceFromLevel(pokemonData.ExperienceGroup, (IsEgg ? (byte)5 : Level));
			pkm.IsSecondAbility2 = (IsSecondAbility.HasValue ? IsSecondAbility.Value : (pokemonData.HasTwoAbilities && random.Next(2) == 1)); // TODO: TESTING
			pkm.Nickname = (Nickname != null ? Nickname : pokemonData.Name.ToUpper());
			pkm.BallCaughtID = 4;
			pkm.MetLocationID = 255;
			if (DexID == 151 || DexID == 386)
				pkm.IsFatefulEncounter = true;
			pkm.LevelMet = (IsEgg ? (byte)0 : Level);
			pkm.Language = Languages.English;
			pkm.Friendship = (byte)(IsEgg ? 10 : 70);

			// Ownership
			pkm.TrainerName = (TrainerName != null ? TrainerName : PokeManager.ManagerGameSave.TrainerName);
			pkm.TrainerGender = (TrainerGender.HasValue ? TrainerGender.Value : PokeManager.ManagerGameSave.TrainerGender);
			pkm.TrainerID = (TrainerID.HasValue ? TrainerID.Value : PokeManager.ManagerGameSave.TrainerID);
			pkm.SecretID = (SecretID.HasValue ? SecretID.Value : PokeManager.ManagerGameSave.SecretID);
			if (TrainerGender.HasValue && TrainerGender.Value == Genders.Genderless)
				pkm.TrainerGender = (random.Next(2) == 1 ? Genders.Female : Genders.Male);

			if (!Personality.HasValue)
				GeneratePID(pkm, random);

			if (HeldItem != null) pkm.HeldItemID = HeldItem[random.Next(HeldItem.Length)];

			if (Conditions != null) {
				pkm.Coolness	= Conditions[0];
				pkm.Beauty		= Conditions[1];
				pkm.Cuteness	= Conditions[2];
				pkm.Smartness	= Conditions[3];
				pkm.Toughness	= Conditions[4];
				pkm.Feel		= Conditions[5];
			}

			pkm.HPIV		= (IVs != null ? IVs[0] : (byte)random.Next(32));
			pkm.AttackIV	= (IVs != null ? IVs[1] : (byte)random.Next(32));
			pkm.DefenseIV	= (IVs != null ? IVs[2] : (byte)random.Next(32));
			pkm.SpAttackIV	= (IVs != null ? IVs[3] : (byte)random.Next(32));
			pkm.SpDefenseIV	= (IVs != null ? IVs[4] : (byte)random.Next(32));
			pkm.SpeedIV		= (IVs != null ? IVs[5] : (byte)random.Next(32));

			if (HiddenPowerDamage.HasValue)	pkm.SetHiddenPowerDamage(HiddenPowerDamage.Value);
			if (HiddenPowerType.HasValue) pkm.SetHiddenPowerType(HiddenPowerType.Value);

			// Teach the Pokemon all of it's default moves
			ushort[] moves = PokemonDatabase.GetMovesLearnedAtLevelRange(pkm, 1, (IsEgg ? (byte)5 : Level));
			foreach (ushort moveID in moves) {
				if (!PokemonDatabase.PokemonHasMove(pkm, moveID)) {
					if (pkm.NumMoves < 4) {
						pkm.SetMoveAt(pkm.NumMoves, new Move(moveID));
					}
					else {
						for (int i = 0; i < 3; i++) {
							pkm.SetMoveAt(i, pkm.GetMoveAt(i + 1));
						}
						pkm.SetMoveAt(3, new Move(moveID));
					}
				}
			}
			if (Move1ID.HasValue) pkm.SetMoveAt(0, new Move(Move1ID.Value));
			if (Move2ID.HasValue) pkm.SetMoveAt(1, new Move(Move2ID.Value));
			if (Move3ID.HasValue) pkm.SetMoveAt(2, new Move(Move3ID.Value));
			if (Move4ID.HasValue) pkm.SetMoveAt(3, new Move(Move4ID.Value));

			GameTypes gameType = gameSave.GameType;
			if (gameType == GameTypes.Ruby)				pkm.GameOrigin = GameOrigins.Ruby;
			else if (gameType == GameTypes.Sapphire)	pkm.GameOrigin = GameOrigins.Sapphire;
			else if (gameType == GameTypes.Emerald)		pkm.GameOrigin = GameOrigins.Emerald;
			else if (gameType == GameTypes.FireRed)		pkm.GameOrigin = GameOrigins.FireRed;
			else if (gameType == GameTypes.LeafGreen)	pkm.GameOrigin = GameOrigins.LeafGreen;
			else if (gameType == GameTypes.Colosseum ||
					 gameType == GameTypes.XD)			pkm.GameOrigin = GameOrigins.ColosseumXD;
			else										pkm.GameOrigin = GameOrigins.Emerald;

			pkm.GameType = gameType;
			pkm.Checksum = pkm.CalculateChecksum();
			pkm.RecalculateStats();
			reward = pkm;
		}

		private void GeneratePID(GBAPokemon pkm, Random random) {
			PokemonData pokemonData = PokemonDatabase.GetPokemonFromDexID(DexID);
			byte genderByte = pokemonData.GenderRatio;

			BitArray bitsID = ByteHelper.GetBits((ushort)(pkm.TrainerID ^ pkm.SecretID));
			BitArray bitsPID1 = new BitArray(16);
			BitArray bitsPID2 = new BitArray(16);
			uint pid = 0;
			int tryCount = 0;
			while (true) {
				if (IsShiny.HasValue && !IsEgg) {
					if (IsShiny.Value) {
						for (int i = 0; i < 3; i++) {
							bitsPID1[i] = random.Next(2) == 1;
							bitsPID2[i] = random.Next(2) == 1;
						}
						for (int i = 3; i < 16; i++) {
							bool bit = random.Next(2) == 1;
							bitsPID1[i] = (bitsID[i] ? !bit : bit);
							bitsPID2[i] = bit;
						}
						pid = 0;
						pid = ByteHelper.SetBits(pid, 16, bitsPID1);
						pid = ByteHelper.SetBits(pid, 0, bitsPID2);
						pkm.Personality = pid;
					}
					else {
						// The chances of this are astronamically low so we can be lazy and not use an algorithm
						while (pkm.IsShiny) {
							pkm.Personality = (uint)random.Next();
						}
					}
				}
				else {
					pkm.Personality = (uint)random.Next();
				}

				if ((!Gender.HasValue || pkm.Gender == Gender || genderByte == 255 || genderByte == 254 || genderByte == 0) &&
					(!NatureID.HasValue || pkm.NatureID == NatureID)) {
					break;
				}

				tryCount++;
				// Let's give up (I haven't done the math to see if it's always possible to get all 3 combinations. Plus RNG is a bitch so... we need a failsafe)
				if (tryCount > 5000) {
					break;
				}
			}
		}

		// Can only be used after generate reward.
		public override BitmapSource RewardSprite {
			get { return reward.Sprite; }
		}
		public override void GiveReward(IGameSave gameSave) {
			gameSave.PokePC.PlacePokemonInNextAvailableSlot(0, 0, reward);
			PokeManager.ManagerWindow.GotoPokemon(reward);
		}
		public override bool IsCompleted(IGameSave gameSave) {
			return false;
		}
		public override bool IsRequirementsFulfilled(IGameSave gameSave) {
			return CheckRequirements(gameSave);
		}
		public override bool HasRoomForReward(IGameSave gameSave) {
			return gameSave.PokePC.HasRoomForPokemon(1);
		}

	}
}
