using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Game.FileStructure.Gen3.GBA {

	public enum RubySapphireGameFlags : ushort {

		HasBadge1 = 0x807,
		HasBadge2 = 0x808,
		HasBadge3 = 0x809,
		HasBadge4 = 0x80A,
		HasBadge5 = 0x80B,
		HasBadge6 = 0x80C,
		HasBadge7 = 0x80D,
		HasBadge8 = 0x80E,

		HasBeatenEliteFour = 0x804,

		EonTicketActivated = 0x853
	}

	public enum EmeraldGameFlags : ushort {

		HasUsedSwaggerMoveTutor = 0x1B1,
		HasUsedRolloutMoveTutor = 0x1B2,
		HasUsedFuryCutterMoveTutor = 0x1B3,
		HasUsedMimicMoveTutor = 0x1B4,
		HasUsedMetronomeMoveTutor = 0x1B5,
		HasUsedSleepTalkMoveTutor = 0x1B6,
		HasUsedSubstituteMoveTutor = 0x1B7,
		HasUsedDynamicPunchMoveTutor = 0x1B8,
		HasUsedDoubleEdgeMoveTutor = 0x1B9,
		HasUsedExplosionMoveTutor = 0x1BA,

		HasBeatenSteven = 0x4F8,

		HasBeatenEliteFour = 0x864,

		HasBadge1 = 0x867,
		HasBadge2 = 0x868,
		HasBadge3 = 0x869,
		HasBadge4 = 0x86A,
		HasBadge5 = 0x86B,
		HasBadge6 = 0x86C,
		HasBadge7 = 0x86D,
		HasBadge8 = 0x86E,

		EonTicketActivated = 0x8B3,
		HasReceivedFrontierPass = 0x8D2,
		AuroraTicketActivated = 0x8D5,
		OldSeaMapActivated = 0x8D6,
		MysticTicketActivated = 0x8E0
	}

	public enum FireRedLeafGreenGameFlags : ushort {

		HasUsedDoubleEdgeTutor = 0x2C0,
		HasUsedThunderWaveTutor = 0x2C1,
		HasUsedRockSlideTutor = 0x2C2,
		HasUsedExplosionTutor = 0x2C3,
		HasUsedMegaPunchTutor = 0x2C4,
		HasUsedMegaKickTutor = 0x2C5,
		HasUsedDreamEaterTutor = 0x2C6,
		HasUsedSoftboiledTutor = 0x2C7,
		HasUsedSubstituteTutor = 0x2C8,
		HasUsedSwordsDanceTutor = 0x2C9,
		HasUsedSeismicTossTutor = 0x2CA,
		HasUsedCounterTutor = 0x2CB,
		HasUsedMetronomeTutor = 0x2CC,
		HasUsedMimicTutor = 0x2CD,
		HasUsedBodySlamTutor = 0x2CE,

		HasUsedStarterPowerMoveTutor = 0x2E1,
		
		RubySapphireSubplotFinished = 0x05C,

		HasBadge1 = 0x820,
		HasBadge2 = 0x821,
		HasBadge3 = 0x822,
		HasBadge4 = 0x823,
		HasBadge5 = 0x824,
		HasBadge6 = 0x825,
		HasBadge7 = 0x826,
		HasBadge8 = 0x827,

		HasRunningShoes = 0x82F,

		HasBeatenEliteFour = 0x82C,

		MysticTicketActivated = 0x84A,
		AuroraTicketActivated = 0x84B

	}
}
