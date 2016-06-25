using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonManager.Items {
	public enum SecretBaseRoomTypes {
		RedCave,
		BrownCave,
		BlueCave,
		YellowCave,
		Tree,
		Shrub
	}
	public enum SecretBaseRoomLayouts {
		A,
		B,
		C,
		D
	}
	public enum Sides {
		None,
		Left,
		Right
	}
	public enum SecretBasePlacementTypes {
		// Room
		BedroomFloor,
		Blocked, // [B] Nothing Allowed
		Floor, // [F] Solid/Passable/Mat/Table/Chair Only
		Wall, // [W] Passable/Poster Only
		Rock, // [R] BigDollBack Only
		Hole, // [H] Board Only
		Reserved, // [S] Passable Only
		
		// Decoration
		Empty, // Every Type
		Solid, // Floor Only
		Back, // Floor/Wall/Reserved Only
		BackNoWall, //Floor/Reserved Only
		Poster, // Wall Only
		Mat, // Floor Only
		MatCenter, // Floor Only
		Doll, // (Also Cushion) Mat/MatCenter/DollBack/DollSide Only
		DollSide, // Mat/Doll/DollBack/DollSide Only
		DollBack, // Everything but Blocked
		Board // Hole Only
	}
	public enum DecorationDataTypes {
		Unknown,
		SmallSolid,
		WideDesk,
		LargeDesk,
		SmallStepable,
		SmallStatue,
		LargeStatue,
		Brick,
		Stepable,
		Tent,
		Board,
		Slide,
		Tire,
		Stand,
		Door,
		NoteMat,
		LargeMat,
		SmallPoster,
		LargePoster,
		SmallDoll,
		LargeDoll
	}
}
