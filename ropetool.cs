// the bottomprint uses the style of the New Duplicator for continuity

datablock ItemData(ropeToolItem : hammerItem)
{
	uiName = "Rope Tool";
	colorShiftColor = "0.298039 0.686275 0.313726 1.000000";
	image = "ropeToolImage";
};

datablock ShapeBaseImageData(ropeToolImage : hammerImage)
{
	Item = "ropeToolItem";
	//Projectile = ropeToolProjectile;
	colorShiftColor = "0.298039 0.686275 0.313726 1.000000";
};

function ropeToolImage::onPreFire(%this, %player, %slot) { return hammerImage::onPreFire(%this, %player, %slot); }
function ropeToolImage::onFire(%this, %player, %slot) { return hammerImage::onFire(%this, %player, %slot); }
function ropeToolImage::onStopFire(%this, %player, %slot) { return hammerImage::onStopFire(%this, %player, %slot); }

function ropeToolImage::onHitObject(%this, %player, %slot, %hitObj, %hitPos, %hitNormal)
{
	if (!%player.ropeToolAuthed)
	{
		ServerPlay3D("ErrorSound", %hitPos);
		return;
	}

	ServerPlay3D("BrickMoveSound", %hitPos);

	if (!isObject(%client = %player.client))
		return;

	if (%player.ropeToolPosA $= "")
	{
		%player.ropeToolPosA = %hitPos;
		%player.ropeToolPosANorm = %hitNormal;
		%client.updateRopeToolBP();
		ServerPlay3D("BrickRotateSound", %hitPos);
		%client.ropeToolGhostLoop();
		return;
	}

	ServerPlay3D("BrickPlantSound", %hitPos);

	%group = _getRopeGroup(getSimTime(), %client.getBLID(), "");
	%group.brickGroup = getBrickGroupFromObject(%client);

	%posA = VectorAdd( %player.ropeToolPosA, VectorScale( %player.ropeToolPosANorm, %client.ropeToolDiameter / 2 ) );
	%posB = VectorAdd( %hitPos, VectorScale( %hitNormal, %client.ropeToolDiameter / 2 ) );

	createRope(%posA, %posB, %client.currentColor, %client.ropeToolDiameter, %client.ropeToolSlack, %group);

	%group.madeFromTool = true;
	%group.savePosA = %posA;
	%group.savePosB = %posB;
	%group.saveColor = %client.currentColor;
	%group.saveDiameter = %client.ropeToolDiameter;
	%group.saveSlack = %client.ropeToolSlack;

	%player.ropeToolPosA = %hitPos;
	%player.ropeToolPosANorm = %hitNormal;
	%client.updateRopeToolBP();

	%client.undoStack.push(%group.getID() TAB "ROPETOOL");
}

function GameConnection::ropeToolGhostClear(%client)
{
	cancel(%client.ropeToolGhostLoop);

	if (!isObject(%player = %client.player))
		return;

	if (isObject(%player.ropeToolGhost))
		%player.ropeToolGhost.delete();

	for(%i = 0; %i < 4; %i++)
	{
		if (isObject(%player.ropeToolGhost[%i]))
			%player.ropeToolGhost[%i].delete();
	}
}

function GameConnection::ropeToolGhostLoop(%client)
{
	cancel(%client.ropeToolGhostLoop);

	if (!isObject(%player = %client.player) || %player.ropeToolPosA $= "")
		return;

	%posA = VectorAdd( %player.ropeToolPosA, VectorScale( %player.ropeToolPosANorm, %client.ropeToolDiameter / 2 ) );
	%posB = %player.getHackPosition();

	%vec = vectorNormalize( vectorSub(%posB, %posA) );
	%dist = vectorDist( %posB, %posA );

	for(%i = 0; %i < 4; %i++)
	{
		%j = %i + 1;

		%subPosA = solveRopeDrop( %posA, %vec, %dist, %i, %client.ropeToolSlack, %client.ropeToolDiameter, 4 );
		%subPosB = solveRopeDrop( %posA, %vec, %dist, %j, %client.ropeToolSlack, %client.ropeToolDiameter, 4 );

		if (!isObject(%player.ropeToolGhost[%i]))
			%player.ropeToolGhost[%i] = _getNewRope( %client.ropeToolDiameter, %client.currentColor, "", true );
		else
			%player.ropeToolGhost[%i].diameter = %client.ropeToolDiameter;

		_aimRope( %player.ropeToolGhost[%i], %subPosA, %subPosB );
	}

	%client.ropeToolGhostLoop = %client.schedule(16, "ropeToolGhostLoop");
}

function GameConnection::ropeToolCleanup(%client)
{
	commandToClient(%client, 'clearBottomPrint');
	%client.ropeToolGhostClear();

	if (!isObject(%player = %client.player))
		return;

	%player.ropeToolAuthed = false;
}

function GameConnection::updateRopeToolBP(%client)
{
	if (!isObject(%client.player))
		return;

	%colHex = rgbToHex(getColorIDTable(%client.currentColor));

	%tut = (%client.player.ropeToolPosA $= "") ? "Click somewhere to set the first point" : "Now click another point to create a rope";

	%msg = "<font:Arial:22>\c6Rope Tool\n";
	%msg = %msg @ "<font:Verdana:16>\c6Slack: \c3" @ %client.ropeToolSlack @ " \c6[Brick Fwd/Back]<just:right>\c6" @ %tut @ " \n<just:left>";
	%msg = %msg @ "\c6Diameter: \c3" @ %client.ropeToolDiameter @ " \c6[Brick Left/Right]\n";
	%msg = %msg @ "\c6Color: <font:impact:20><color:" @ %colHex @ ">|||||<font:Verdana:16> \c6[Paint Color]\n";

	commandToClient(%client, 'BottomPrint', %msg, 0, true);
}

if (isPackage(RopeToolPackage))
{
	deactivatePackage(RopeToolPackage);
}

package RopeToolPackage
{
	function ropeToolImage::onMount(%this, %obj, %slot)
	{
		Parent::onMount(%this, %obj, %slot);

		if (!isObject(%client = %obj.client))
			return;

		if ($Pref::Ropes::ToolAdminOnly && !%client.isAdmin)
		{
			commandToClient(%client, 'centerPrint', "<font:Verdana:20>\c6Oops! The rope tool is admin only.", 5);
			return;
		}

		%obj.ropeToolAuthed = true;
		%obj.ropeToolPosA = "";

		if (%client.ropeToolSlack $= "") %client.ropeToolSlack = 2;
		if (%client.ropeToolDiameter $= "") %client.ropeToolDiameter = 0.2;

		%client.updateRopeToolBP();
	}

	function ropeToolImage::onUnMount(%this, %obj, %slot)
	{
		if (%obj.client) %obj.client.ropeToolCleanup();
		Parent::onUnMount(%this, %obj, %slot);
	}

	function GameConnection::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc)
	{
		%client.ropeToolCleanup();
		Parent::onDeath(%client, %sourceObject, %sourceClient, %damageType, %damLoc);
	}

	function serverCmdShiftBrick(%client, %x, %y, %z)
	{
		if (!isObject(%player = %client.player) || !%player.ropeToolAuthed)
		{
			return Parent::serverCmdShiftBrick(%client, %x, %y, %z);
		}

		if (%x > 0) {
			%client.ropeToolSlack = mClampF(%client.ropeToolSlack + 0.5, 0, 50);
		} else if (%x < 0) {
			%client.ropeToolSlack = mClampF(%client.ropeToolSlack - 0.5, 0, 50);
		} else if (%y > 0) {
			%client.ropeToolDiameter = mClampF(%client.ropeToolDiameter - 0.05, 0.05, 2.0);
		} else if (%y < 0) {
			%client.ropeToolDiameter = mClampF(%client.ropeToolDiameter + 0.05, 0.05, 2.0);
		}

		%client.updateRopeToolBP();
	}

	function serverCmdSuperShiftBrick(%client, %x, %y, %z)
	{
		if (!isObject(%player = %client.player) || !%player.ropeToolAuthed)
		{
			return Parent::serverCmdSuperShiftBrick(%client, %x, %y, %z);
		}

		serverCmdShiftBrick(%client, %x, %y, %z);
	}

	function serverCmdRopeToo(%client) { serverCmdRopeTool(%client); }
	function serverCmdRopeTo (%client) { serverCmdRopeTool(%client); }
	function serverCmdRopeT  (%client) { serverCmdRopeTool(%client); }
	function serverCmdRope   (%client) { serverCmdRopeTool(%client); }
	function serverCmdRop    (%client) { serverCmdRopeTool(%client); }
	function serverCmdRo     (%client) { serverCmdRopeTool(%client); }
	function serverCmdR      (%client) { serverCmdRopeTool(%client); }

	function GameConnection::onClientLeaveGame(%client)
	{
		%client.ropeToolCleanup();
		Parent::onClientLeaveGame(%client);
	}

	function serverCmdLight(%client)
	{
		if (isObject(%player = %client.player) && %player.ropeToolAuthed)
		{
			%player.ropeToolPosA = "";
			%client.updateRopeToolBP();
			%client.ropeToolGhostClear();
			return;
		}
		Parent::serverCmdLight(%client);
	}

	function serverCmdUndoBrick(%client)
	{
		%state = %client.undoStack.pop();
		%type = getField(%state, 1);

		if (%type $= "ROPETOOL")
		{
			%group = getField(%state, 0);

			if (isObject(%group))
			{
				if (isObject(%player = %client.player))
				%player.playThread(3, "undo");

				for (%i = 0; %i < %group.getCount(); %i++)
				{
					serverPlay3D("BrickBreakSound", %group.getObject(%i).getPosition());
				}
				%group.delete();
			}

			return;
		}

		%client.undoStack.push(%state);
		Parent::serverCmdUndoBrick(%client);
	}
};

activatePackage(RopeToolPackage);
