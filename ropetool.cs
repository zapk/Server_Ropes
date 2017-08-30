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
		%client.updateRopeToolBP();
		ServerPlay3D("BrickPlantSound", %hitPos);
		return;
	}

	%group = _getRopeGroup(getSimTime(), %client.getBLID(), "");
	%group.brickGroup = getBrickGroupFromObject(%client);

	createRope(%player.ropeToolPosA, %hitPos, %client.currentColor, %client.ropeToolDiameter, %client.ropeToolSlack, %group);
	%player.ropeToolPosA = "";
}

function GameConnection::updateRopeToolBP(%this)
{
	if (!isObject(%this.player))
		return;

	%colHex = rgbToHex(getColorIDTable(%this.currentColor));

	%tut = (%this.player.ropeToolPosA $= "") ? "Click somewhere to set the first point" : "Now click another point to create a rope";

	%msg = "<font:Arial:22>\c6Rope Tool\n";
	%msg = %msg @ "<font:Verdana:16>\c6Slack: \c3" @ %this.ropeToolSlack @ " \c6[Shift Fwd/Back]<just:right>\c6" @ %tut @ " \n<just:left>";
	%msg = %msg @ "\c6Diameter: \c3" @ %this.ropeToolDiameter @ " \c6[Shift Left/Right]\n";
	%msg = %msg @ "\c6Color: <font:impact:20><color:" @ %colHex @ ">|||||<font:Verdana:16> \c6[Paint Color]\n";

	commandToClient(%this, 'BottomPrint', %msg, 0, true);
}

function serverCmdRopeTool(%this)
{
	if ($Pref::Ropes::ToolAdminOnly && !%this.isAdmin)
	{
		messageClient(%this, '', "\c6The rope tool is admin only. Ask an admin for help.");
		return;
	}

	if (isObject(%this.minigame) && !%this.minigame.enablebuilding)
	{
		messageClient(%this, '', "\c6You cannot use the rope tool while building is disabled in your minigame.");
		return;
	}

	if (!isObject(%player = %this.player))
	{
		messageClient(%this, '', "\c6You must be spawned to equip the rope tool.");
		return;
	}

	%player.updateArm(ropeToolImage);
	%player.mountImage(ropeToolImage, 0);
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
		commandToClient(%obj.client, 'clearBottomPrint');
		%obj.ropeToolAuthed = false;
		Parent::onUnMount(%this, %obj, %slot);
	}

	function GameConnection::onDeath(%this, %sourceObject, %sourceClient, %damageType, %damLoc)
	{
		commandToClient(%this, 'clearBottomPrint');
		Parent::onDeath(%this, %sourceObject, %sourceClient, %damageType, %damLoc);
	}

	function serverCmdShiftBrick(%this, %x, %y, %z)
	{
		if (!isObject(%player = %this.player) || !%player.ropeToolAuthed)
		{
			return Parent::serverCmdShiftBrick(%this, %x, %y, %z);
		}

		if (%x > 0) {
			%this.ropeToolSlack = mClampF(%this.ropeToolSlack + 1, 0, 50);
		} else if (%x < 0) {
			%this.ropeToolSlack = mClampF(%this.ropeToolSlack - 1, 0, 50);
		} else if (%y > 0) {
			%this.ropeToolDiameter = mClampF(%this.ropeToolDiameter - 0.1, 0.1, 1.0);
		} else if (%y < 0) {
			%this.ropeToolDiameter = mClampF(%this.ropeToolDiameter + 0.1, 0.1, 1.0);
		}

		%this.updateRopeToolBP();
	}

	function serverCmdRopeToo(%this) { serverCmdRopeTool(%this); }
	function serverCmdRopeTo (%this) { serverCmdRopeTool(%this); }
	function serverCmdRopeT  (%this) { serverCmdRopeTool(%this); }
	function serverCmdRope   (%this) { serverCmdRopeTool(%this); }
	function serverCmdRop    (%this) { serverCmdRopeTool(%this); }
	function serverCmdRo     (%this) { serverCmdRopeTool(%this); }
	function serverCmdR      (%this) { serverCmdRopeTool(%this); }
};

activatePackage(RopeToolPackage);
