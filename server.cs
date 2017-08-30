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
// Author:		Zapk
// Version:		2.0.0 (November 2015)
// URL:				https://github.com/zapk/Server_Ropes

if(!isObject(MainRopeGroup))
{
	new ScriptGroup(MainRopeGroup);
}

if(isFile("Add-Ons/System_ReturnToBlockland/server.cs") && !$RTB::Hooks::ServerControl)
{
	exec("Add-Ons/System_ReturnToBlockland/hooks/serverControl.cs");
}

RTB_registerPref("Slacked Rope Shapes", "Ropes", "Pref::Ropes::Iterations", "int 1 40", "Server_Ropes", 20, 0, 0);
RTB_registerPref("Rope Tool Admin Only", "Ropes", "Pref::Ropes::ToolAdminOnly", "bool", "Server_Ropes", true, 0, 0);

datablock StaticShapeData(RopeCylinder)
{
	shapeFile = "./rope.dts";
};

exec("./math.cs");
exec("./ropetool.cs");

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
	%group.brickGroup = %this.getGroup();

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

	function WandImage::onHitObject(%this, %player, %slot, %hitObj, %hitPos, %hitNormal)
	{
		Parent::onHitObject(%this, %player, %slot, %hitObj, %hitPos, %hitNormal);

		if (!isObject(%player.client) || !isObject(%hitObj) || !%hitObj.isRope)
			return;

		%brickGroup = %hitObj.getGroup().brickGroup;

		if (!isObject(%brickGroup))
			return;

		if (getTrustLevel(%player, %brickGroup) < $TrustLevel::Wand)
		{
			commandToClient(%player.client, 'CenterPrint', %brickGroup.name @ " does not trust you enough to do that", 1);
		}
		else
		{
			serverPlay3D("BrickBreakSound", %hitPos);
			%hitObj.getGroup().delete();
		}
	}

	function AdminWandImage::onHitObject(%this, %player, %slot, %hitObj, %hitPos, %hitNormal)
	{
		Parent::onHitObject(%this, %player, %slot, %hitObj, %hitPos, %hitNormal);

		if (!isObject(%player.client) || !isObject(%hitObj) || !%hitObj.isRope)
			return;

		serverPlay3D("BrickBreakSound", %hitPos);
		%hitObj.getGroup().delete();
	}
};

activatePackage(RopePackage);
