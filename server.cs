//          _____                   _______                   _____                    _____                    _____
//         /\    \                 /::\    \                 /\    \                  /\    \                  /\    \
//        /::\    \               /::::\    \               /::\    \                /::\    \                /::\    \
//       /::::\    \             /::::::\    \             /::::\    \              /::::\    \              /::::\    \
//      /::::::\    \           /::::::::\    \           /::::::\    \            /::::::\    \            /::::::\    \
//     /:::/\:::\    \         /:::/~~\:::\    \         /:::/\:::\    \          /:::/\:::\    \          /:::/\:::\    \
//    /:::/__\:::\    \       /:::/    \:::\    \       /:::/__\:::\    \        /:::/__\:::\    \        /:::/__\:::\    \
//   /::::\   \:::\    \     /:::/    / \:::\    \     /::::\   \:::\    \      /::::\   \:::\    \       \:::\   \:::\    \
//  /::::::\   \:::\    \   /:::/____/   \:::\____\   /::::::\   \:::\    \    /::::::\   \:::\    \    ___\:::\   \:::\    \
// /:::/\:::\   \:::\____\ |:::|    |     |:::|    | /:::/\:::\   \:::\____\  /:::/\:::\   \:::\    \  /\   \:::\   \:::\    \
///:::/  \:::\   \:::|    ||:::|____|     |:::|    |/:::/  \:::\   \:::|    |/:::/__\:::\   \:::\____\/::\   \:::\   \:::\____\
//\::/   |::::\  /:::|____| \:::\    \   /:::/    / \::/    \:::\  /:::|____|\:::\   \:::\   \::/    /\:::\   \:::\   \::/    /
// \/____|:::::\/:::/    /   \:::\    \ /:::/    /   \/_____/\:::\/:::/    /  \:::\   \:::\   \/____/  \:::\   \:::\   \/____/
//       |:::::::::/    /     \:::\    /:::/    /             \::::::/    /    \:::\   \:::\    \       \:::\   \:::\    \
//       |::|\::::/    /       \:::\__/:::/    /               \::::/    /      \:::\   \:::\____\       \:::\   \:::\____\
//       |::| \::/____/         \::::::::/    /                 \::/____/        \:::\   \::/    /        \:::\  /:::/    /
//       |::|  ~|                \::::::/    /                   ~~               \:::\   \/____/          \:::\/:::/    /
//       |::|   |                 \::::/    /                                      \:::\    \               \::::::/    /
//       \::|   |                  \::/____/                                        \:::\____\               \::::/    /
//        \:|   |                   ~~                                               \::/    /                \::/    /
//         \|___|                                                                     \/____/                  \/____/
//
// Author: Zapk
// Version: 2.0.0 (November 2015)

// Events:
// - ropeClearAll
// - ropeToBricks [brick names] [color] [diameter] [slack]
//   Brick names are separated by spaces.

// Commands:
// - /clearropes - clears your own ropes
// - /clearallropes - clears all ropes on the server (Admin)

// Changes:
// 1.0.0 to 2.0.0:
// -  Ropes now fall onto bricks instead of clipping through them.
// -  "Shapes Per Rope" pref renamed to "Slacked Rope Shapes", maximum increased to 40.
// -  /clearallropes now displays the message even if no ropes existed.

if(!isObject(MainRopeGroup))
{
	new ScriptGroup(MainRopeGroup);
}

if(isFile("Add-Ons/System_ReturnToBlockland/server.cs") && !$RTB::Hooks::ServerControl)
{
	exec("Add-Ons/System_ReturnToBlockland/hooks/serverControl.cs");
}

RTB_registerPref("Slacked Rope Shapes", "Ropes", "Pref::Ropes::Iterations", "int 1 40", "Server_Ropes", 20, 0, 0);

datablock StaticShapeData(RopeCylinder)
{
	shapeFile = "./rope.dts";
};

function eulerToAxis(%euler)
{
	%euler = VectorScale( %euler, $pi / 180 );
	%matrix = MatrixCreateFromEuler( %euler );
	return getWords( %matrix, 3, 6 );
}

function axisToEuler(%axis)
{
	%angleOver2 = getWord( %axis, 3 ) * 0.5;
	%angleOver2 = -%angleOver2;
	%sinThetaOver2 = mSin( %angleOver2 );
	%cosThetaOver2 = mCos( %angleOver2 );
	%q0 = %cosThetaOver2;
	%q1 = getWord( %axis, 0 ) * %sinThetaOver2;
	%q2 = getWord( %axis, 1 ) * %sinThetaOver2;
	%q3 = getWord( %axis, 2 ) * %sinThetaOver2;
	%q0q0 = %q0 * %q0;
	%q1q2 = %q1 * %q2;
	%q0q3 = %q0 * %q3;
	%q1q3 = %q1 * %q3;
	%q0q2 = %q0 * %q2;
	%q2q2 = %q2 * %q2;
	%q2q3 = %q2 * %q3;
	%q0q1 = %q0 * %q1;
	%q3q3 = %q3 * %q3;
	%m13 = 2.0 * ( %q1q3 - %q0q2 );
	%m21 = 2.0 * ( %q1q2 - %q0q3 );
	%m22 = 2.0 * %q0q0 - 1.0 + 2.0 * %q2q2;
	%m23 = 2.0 * ( %q2q3 + %q0q1 );
	%m33 = 2.0 * %q0q0 - 1.0 + 2.0 * %q3q3;
	return mRadToDeg( mAsin( %m23 ) ) SPC mRadToDeg( mAtan( -%m13, %m33 ) ) SPC mRadToDeg( mAtan( -%m21, %m22 ) );
}

function _aimRope(%rope, %posA, %posB)
{
	if(!isObject(%rope) || %posA $= "" || %posB $= "")
	{
		return;
  	}

	%rope.setTransform( %posA );

	%fv = %rope.getForwardVector();

	%xA = getWord( %fv, 0 );
	%yA = getWord( %fv, 1 );

	%nv = vectorNormalize( vectorSub( %posB, %rope.getPosition() ) );

	%xB = getWord( %nv, 0 );
	%yB = getWord( %nv, 1 );

	%rad = mATan( %xB, %yB ) - mATan( %xA, %yA );
	%deg = ( 180 / $pi ) * %rad;

	%rot = axisToEuler( getWords( %rope.getTransform(), 3, 7 ) );

	%radB = mATan( getWord( %fv, 2 ), mSqrt( %xA * %xA + %yA * %yA ) ) - mATan( getWord( %nv, 2 ), mSqrt( %xB * %xB + %yB * %yB ) );
	%degB = ( 180 / $pi ) * %radB;

	%newRot = eulerToAxis( vectorAdd( %rot, -%degB + %offset SPC "0" SPC -%deg + %offset ) );

	%midPos = vectorAdd( vectorScale( vectorSub( %posB, %rope.getPosition() ), 0.5 ), %rope.getPosition() );

	%rope.setTransform( %midPos SPC %newRot );

	%dist = vectorDist( %posB, %posA ) + 0.1;

	%rope.setScale( %rope.diameter SPC %dist SPC %rope.diameter );
}

function clearRopes(%bl_id)
{
	%c = 0;

	%useID = (%bl_id !$= "");

	if(isObject(MainRopeGroup) && MainRopeGroup.getCount())
	{
		for(%i = MainRopeGroup.getCount() - 1; %i >= 0; %i--)
		{
			%rg = MainRopeGroup.getObject(%i);

			if(!%useID || %rg.bl_id $= %bl_id)
			{
				%c++;
				%rg.delete();
			}
		}
	}

	return mFloor(%c);
}

function _removeRopeGroup(%creationData)
{
	for(%i = MainRopeGroup.getCount() - 1; %i >= 0; %i--)
	{
		%sg = MainRopeGroup.getObject(%i);

		if(%sg.creationData $= %creationData)
		{
			%sg.delete();
		}
	}
}

function _getRopeGroup(%group, %bl_id, %creationData)
{
	for(%i = 0; %i < MainRopeGroup.getCount(); %i++)
	{
		%sg = MainRopeGroup.getObject(%i);

		if(%sg.gn $= %group)
		{
			return %sg.getID();
		}
	}

	%new = new ScriptGroup()
	{
		gn = %group;
		bl_id = %bl_id;
		creationData = %creationData;
	};

	MainRopeGroup.add(%new);

	return %new;
}

function getNewRope(%diameter, %color, %group)
{
	%rope = new StaticShape()
	{
		position = "0 0 0";
		rotation = "0 0 0";
		scale = %diameter SPC %diameter SPC %diameter;
		dataBlock = RopeCylinder;
		canSetIFLs = false;
		diameter = %diameter;
		isRope = true;
	};

	%rope.setNodeColor("ALL", getColorIDTable(%color));

	%group.add(%rope);

	return %rope;
}

function solveRopeDrop(%posA, %vec, %dist, %iter, %slack, %diameter)
{
	%rawPos = vectorAdd( %posA, vectorScale( %vec, %dist * (%iter / $Pref::Ropes::Iterations) ) );
	%endPos = vectorSub( %rawPos, 0 SPC 0 SPC ( mSin(($pi / $Pref::Ropes::Iterations) * %iter) * %slack ) );

	%ray = containerRaycast( %rawPos, %endPos, $Typemasks::FxBrickObjectType | $Typemasks::TerrainObjectType );
	%hit = firstWord( %ray );

	if(isObject(%hit))
	{
		%endPos = VectorAdd( getWords(%ray, 1, 3), "0 0 " @ %diameter / 2 );
	}

	return %endPos;
}

function createRope(%posA, %posB, %color, %diameter, %slack, %group)
{
	%diameter = mClampF( %diameter, 0.1, 1 );
	%slack = mClampF( %slack, 0, 50 );

	if(%slack < 0.01 && %slack > -0.01)
	{
		%rope = getNewRope( mClampF( %diameter, 0, 1 ), %color, %group );

		_aimRope( %rope, %posA, %posB );
		return;
	}

	%vec = vectorNormalize( vectorSub(%posB, %posA) );
	%dist = vectorDist( %posB, %posA );

	for(%i = 0; %i < $Pref::Ropes::Iterations; %i++)
	{
		%j = %i + 1;

		%subPosA = solveRopeDrop( %posA, %vec, %dist, %i, %slack, %diameter );
		%subPosB = solveRopeDrop( %posA, %vec, %dist, %j, %slack, %diameter );

		%rope = getNewRope( %diameter, %color, %group );

		_aimRope( %rope, %subPosA, %subPosB );
	}
}

function serverCmdClearAllRopes(%this)
{
	if(!%this.isAdmin)
		return;

	%ropes = clearRopes();

	messageAll('MsgClearBricks', '\c3%1 \c0cleared all ropes. (%2)', %this.getPlayerName(), %ropes);
}

function serverCmdClearRopes(%this)
{
	%ropes = clearRopes(%this.bl_id);

	if(%ropes)
	{
		messageAll('MsgClearBricks', '\c3%1 \c2cleared \c3%1\c2\'s ropes (%2)', %this.getPlayerName(), %ropes);
	}
}

registerOutputEvent("fxDTSBrick", "ropeClearAll");

function fxDTSBrick::ropeClearAll(%this)
{
	for(%i = 0; %i < getWordCount(%this.ropeGroups); %i++)
	{
		%g = getWord(%this.ropeGroups, %i);
		if(isObject(%g))
		{
			%g.delete();
		}
	}

	%this.ropeGroups = "";
}

function hReturnNamedBrick(%brickGroup,%name)
{
	%name = "_" @ %name;
	for(%a = 0; %a < %brickGroup.NTNameCount; %a++)
	{
		if(%brickGroup.NTName[%a] !$= %name || %brickGroup.NTObjectCount[%name] < 0)
			continue;

		%n = %brickGroup.NTObjectCount[%name];

		return %brickGroup.NTObject[%name,getRandom(0,%n-1)];
	}
	return 0;
}

registerOutputEvent("fxDTSBrick", "ropeToBricks", "string 200 100" TAB "paintColor 0" TAB "float 0.1 1 0.01 0.2" TAB "float 0 50 0.01 2");

function fxDTSBrick::ropeToBricks(%this, %toNames, %color, %diameter, %slack)
{
	for(%i = 0; %i < getWordCount(%toNames); %i++)
	{
		%this.schedule((%i + 1) * 50, _ropeToBrick, getWord(%toNames, %i), %color, %diameter, %slack);
	}
}

function fxDTSBrick::_ropeToBrick(%this, %toName, %color, %diameter, %slack)
{
	%toBrick = hReturnNamedBrick(%this.getGroup(), %toName);

	if(!isObject(%toBrick)) return;

	%creationData = %this SPC %toBrick SPC %color SPC %diameter SPC %slack;

	_removeRopeGroup(%creationData);

	%group = _getRopeGroup(getSimTime(), %this.getGroup().bl_id, %creationData);

	createRope(%this.getPosition(), %toBrick.getPosition(), %color, %diameter, %slack, %group);

	%this.ropeGroups = trim(%this.ropeGroups SPC %group);
	%toBrick.ropeGroups = trim(%toBrick.ropeGroups SPC %group);
}

if(isPackage(RopePackage))
{
	deactivatePackage(RopePackage);
}

package RopePackage
{
	function fxDTSBrick::onRemove(%this)
	{
		%this.ropeClearAll();
		Parent::onRemove(%this);
	}
};

activatePackage(RopePackage);
