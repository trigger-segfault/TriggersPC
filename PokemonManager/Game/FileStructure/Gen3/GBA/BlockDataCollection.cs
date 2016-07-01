using PokemonManager.Items;
using PokemonManager.PokemonStructures;
using PokemonManager.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GBA {
	public class BlockDataCollection : List<IBlockData> {

		private Inventory inventory;
		private GBAPokePC pokePC;
		private Mailbox mailbox;
		private GBAGameSave gameSave;
		private SecretBaseManager secretBaseManager;

		public BlockDataCollection(GBAGameSave gameSave, byte[] data) {
			if (data.Length != 57344)
				throw new Exception("Save data size must be 57344 bytes");

			this.inventory = new Inventory(gameSave);
			this.pokePC = new GBAPokePC(gameSave);
			this.mailbox = new Mailbox(gameSave, 10);
			this.gameSave = gameSave;

			// Find the trainer info block data first to get vital information
			// AKA: GameCode and Security Key
			for (int i = 0; i < 14; i++) {
				if (ReadSectionID(data, i) == SectionTypes.TrainerInfo) {
					Add(new TrainerInfoBlockData(gameSave, ByteHelper.SubByteArray(i * 4096, data, 4096), this));
					break;
				}
			}

			// Now that we have the trainer info we can get the rest of the blocks
			for (int i = 0; i < 14; i++) {
				SectionTypes sectionID = ReadSectionID(data, i);
				if (sectionID == SectionTypes.TeamAndItems)
					Add(new TeamAndItemsBlockData(gameSave, ByteHelper.SubByteArray(i * 4096, data, 4096), this));
				else if (sectionID == SectionTypes.Unknown1)
					Add(new NationalPokedexBAndCBlockData(gameSave, ByteHelper.SubByteArray(i * 4096, data, 4096), this));
				else if (sectionID == SectionTypes.Unknown2)
					Add(new DecorationBlockData(gameSave, ByteHelper.SubByteArray(i * 4096, data, 4096), this));
				else if (sectionID == SectionTypes.RivalInfo)
					Add(new RivalInfoBlockData(gameSave, ByteHelper.SubByteArray(i * 4096, data, 4096), this));
				else if (sectionID >= SectionTypes.PCBufferA && sectionID <= SectionTypes.PCBufferI)
					Add(new PokeBoxBlockData(gameSave, ByteHelper.SubByteArray(i * 4096, data, 4096), this));
				else if (sectionID != SectionTypes.TrainerInfo)
					Add(new BlockData(gameSave, ByteHelper.SubByteArray(i * 4096, data, 4096), this));
			}

			// Sort the block data list, why not
			List<IBlockData> list = Enumerable.ToList<IBlockData>((IEnumerable<IBlockData>)Enumerable.OrderBy<IBlockData, SectionTypes>(this, (Func<IBlockData, SectionTypes>)(u => u.SectionID)));
			Clear();
			AddRange((IEnumerable<IBlockData>)list);

			// Gather Secret Base Data
			if (GameCode == GameCodes.Emerald || GameCode == GameCodes.RubySapphire) {
				List<byte> sbData = new List<byte>(19 * 160);
				if (GameCode == GameCodes.Emerald) {
					sbData.AddRange(ByteHelper.SubByteArray(3004, NationalPokedexBAndC.Raw, 964));
					sbData.AddRange(ByteHelper.SubByteArray(0, DecorationData.Raw, 2076));
				}
				else if (GameCode == GameCodes.RubySapphire) {
					sbData.AddRange(ByteHelper.SubByteArray(2856, NationalPokedexBAndC.Raw, 1112));
					sbData.AddRange(ByteHelper.SubByteArray(0, DecorationData.Raw, 1928));
				}
				secretBaseManager = new SecretBaseManager(gameSave, sbData.ToArray());
			}

			// Gather the PC data
			List<byte> pcData = new List<byte>(33730);
			List<IBlockData> pcBlockList = this.Where<IBlockData>((IBlockData u) => {
				if (u.SectionID < SectionTypes.PCBufferA)
					return false;
				return u.SectionID <= SectionTypes.PCBufferI;
			}).ToList<IBlockData>();
			foreach (IBlockData blockData in pcBlockList)
				pcData.AddRange((blockData as PokeBoxBlockData).GetBoxData());
			this.pokePC.Load(pcData.ToArray());
		}

		public BlockData this[SectionTypes sectionType] {
			get { return (BlockData)GetBlockData(sectionType); }
		}

		public SectionTypes ReadSectionID(byte[] data, int index) {
			return (SectionTypes)LittleEndian.ToUInt16(data, index * 4096 + 4084);
		}

		public TrainerInfoBlockData TrainerInfo {
			get { return Enumerable.FirstOrDefault<IBlockData>(Enumerable.Where<IBlockData>(this, (Func<IBlockData, bool>)(u => u.SectionID == SectionTypes.TrainerInfo))) as TrainerInfoBlockData; }
		}

		public TeamAndItemsBlockData TeamAndItems {
			get { return Enumerable.FirstOrDefault<IBlockData>(Enumerable.Where<IBlockData>(this, (Func<IBlockData, bool>)(u => u.SectionID == SectionTypes.TeamAndItems))) as TeamAndItemsBlockData; }
		}

		public NationalPokedexBAndCBlockData NationalPokedexBAndC {
			get { return Enumerable.FirstOrDefault<IBlockData>(Enumerable.Where<IBlockData>(this, (Func<IBlockData, bool>)(u => u.SectionID == SectionTypes.Unknown1))) as NationalPokedexBAndCBlockData; }
		}

		public DecorationBlockData DecorationData {
			get { return Enumerable.FirstOrDefault<IBlockData>(Enumerable.Where<IBlockData>(this, (Func<IBlockData, bool>)(u => u.SectionID == SectionTypes.Unknown2))) as DecorationBlockData; }
		}
		public RivalInfoBlockData RivalInfo {
			get { return Enumerable.FirstOrDefault<IBlockData>(Enumerable.Where<IBlockData>(this, (Func<IBlockData, bool>)(u => u.SectionID == SectionTypes.RivalInfo))) as RivalInfoBlockData; }
		}

		public IBlockData GetBlockData(SectionTypes sectionID) {
			return Enumerable.FirstOrDefault<IBlockData>(Enumerable.Where<IBlockData>(this, (Func<IBlockData, bool>)(u => u.SectionID == sectionID)));
		}

		public GBAPokePC PokePC {
			get { return pokePC; }
		}
		public Inventory Inventory {
			get { return inventory; }
		}
		public Mailbox Mailbox {
			get { return mailbox; }
		}
		public SecretBaseManager SecretBaseManager {
			get { return secretBaseManager; }
		}

		public bool GetGameFlag(int index) {
			if (GameCode == GameCodes.FireRedLeafGreen && index < 0x500)
				return TeamAndItems.GetGameFlag(index);
			else
				return NationalPokedexBAndC.GetGameFlag(index);
		}
		public void SetGameFlag(int index, bool flag) {
			if (GameCode == GameCodes.FireRedLeafGreen && index < 0x500)
				TeamAndItems.SetGameFlag(index, flag);
			else
				NationalPokedexBAndC.SetGameFlag(index, flag);
		}
		public void ClearDaycareEgg() {
			if (GameCode == GameCodes.Emerald) {
				SetGameFlag(0x86, false);
				ByteHelper.ReplaceBytes(RivalInfo.Raw, 712, new byte[5]);
			}
			else if (GameCode == GameCodes.RubySapphire) {
				SetGameFlag(0x86, false);
				ByteHelper.ReplaceBytes(RivalInfo.Raw, 564, new byte[3]);
			}
			else if (GameCode == GameCodes.FireRedLeafGreen) {
				SetGameFlag(0x266, false);
				ByteHelper.ReplaceBytes(RivalInfo.Raw, 536, new byte[3]);
			}
		}

		public bool[] Badges {
			get {
				bool[] badges = new bool[8];
				if (GameCode == GameCodes.RubySapphire) {
					badges[0] = GetGameFlag((int)RubySapphireGameFlags.HasBadge1);
					badges[1] = GetGameFlag((int)RubySapphireGameFlags.HasBadge2);
					badges[2] = GetGameFlag((int)RubySapphireGameFlags.HasBadge3);
					badges[3] = GetGameFlag((int)RubySapphireGameFlags.HasBadge4);
					badges[4] = GetGameFlag((int)RubySapphireGameFlags.HasBadge5);
					badges[5] = GetGameFlag((int)RubySapphireGameFlags.HasBadge6);
					badges[6] = GetGameFlag((int)RubySapphireGameFlags.HasBadge7);
					badges[7] = GetGameFlag((int)RubySapphireGameFlags.HasBadge8);
				}
				else if (GameCode == GameCodes.Emerald) {
					badges[0] = GetGameFlag((int)EmeraldGameFlags.HasBadge1);
					badges[1] = GetGameFlag((int)EmeraldGameFlags.HasBadge2);
					badges[2] = GetGameFlag((int)EmeraldGameFlags.HasBadge3);
					badges[3] = GetGameFlag((int)EmeraldGameFlags.HasBadge4);
					badges[4] = GetGameFlag((int)EmeraldGameFlags.HasBadge5);
					badges[5] = GetGameFlag((int)EmeraldGameFlags.HasBadge6);
					badges[6] = GetGameFlag((int)EmeraldGameFlags.HasBadge7);
					badges[7] = GetGameFlag((int)EmeraldGameFlags.HasBadge8);
				}
				else if (GameCode == GameCodes.FireRedLeafGreen) {
					badges[0] = GetGameFlag((int)FireRedLeafGreenGameFlags.HasBadge1);
					badges[1] = GetGameFlag((int)FireRedLeafGreenGameFlags.HasBadge2);
					badges[2] = GetGameFlag((int)FireRedLeafGreenGameFlags.HasBadge3);
					badges[3] = GetGameFlag((int)FireRedLeafGreenGameFlags.HasBadge4);
					badges[4] = GetGameFlag((int)FireRedLeafGreenGameFlags.HasBadge5);
					badges[5] = GetGameFlag((int)FireRedLeafGreenGameFlags.HasBadge6);
					badges[6] = GetGameFlag((int)FireRedLeafGreenGameFlags.HasBadge7);
					badges[7] = GetGameFlag((int)FireRedLeafGreenGameFlags.HasBadge8);
				}
				return badges;
			}
		}

		public bool HasBattledStevenEmerald {
			get { return GetGameFlag((int)EmeraldGameFlags.HasBeatenSteven); }
			set {
				gameSave.IsChanged = true;
				SetGameFlag((int)EmeraldGameFlags.HasBeatenSteven, value);
			}
		}

		public GameCodes GameCode {
			get { return TrainerInfo.GameCode; }
		}
		public uint SecurityKey {
			get { return TrainerInfo.SecurityKey; }
		}
		public ushort UInt16SecurityKey {
			get { return BitConverter.ToUInt16(BitConverter.GetBytes(SecurityKey), 0); }
		}

		public byte[] GetFinalData() {
			if (GameCode == GameCodes.RubySapphire || GameCode == GameCodes.Emerald)
				RecalculateSecretBaseBuffer();
			RecalculatePCBuffer();
			List<byte> list = new List<byte>(57344);
			foreach (IBlockData blockData in this)
				list.AddRange(blockData.GetFinalData());
			return list.ToArray();
		}
		public void RecalculateSecretBaseBuffer() {
			byte[] finalData = secretBaseManager.GetFinalData();
			if (GameCode == GameCodes.Emerald) {
				ByteHelper.ReplaceBytes(NationalPokedexBAndC.Raw, 3004, ByteHelper.SubByteArray(0, finalData, 964));
				ByteHelper.ReplaceBytes(DecorationData.Raw, 0, ByteHelper.SubByteArray(964, finalData, 2076));
			}
			else if (GameCode == GameCodes.RubySapphire) {
				ByteHelper.ReplaceBytes(NationalPokedexBAndC.Raw, 2856, ByteHelper.SubByteArray(0, finalData, 1112));
				ByteHelper.ReplaceBytes(DecorationData.Raw, 0, ByteHelper.SubByteArray(1112, finalData, 1928));
			}
		}
		public void RecalculatePCBuffer() {
			List<IBlockData> list = this.Where<IBlockData>((IBlockData u) => {
				if (u.SectionID < SectionTypes.PCBufferA) {
					return false;
				}
				return u.SectionID <= SectionTypes.PCBufferI;
			}).OrderBy<IBlockData, SectionTypes>((IBlockData u) => u.SectionID).ToList<IBlockData>();
			byte[] finalData = pokePC.GetFinalData();
			int num = 0;
			foreach (IBlockData blockDatum in list) {
				PokeBoxBlockData pokeBoxBlockDatum = blockDatum as PokeBoxBlockData;
				if (blockDatum.SectionID != SectionTypes.PCBufferI) {
					pokeBoxBlockDatum.OverrideBoxData(ByteHelper.SubByteArray(num * 3968, finalData, 3968));
				}
				else {
					pokeBoxBlockDatum.OverrideBoxData(ByteHelper.SubByteArray(num * 3968, finalData, 2000));
				}
				num++;
			}
		}
	}
}
