function serverCmdRopeHelp(%client)
{
	messageClient(%client, '', "\c6/clearAllRopes, /clearRopes, /ropeTool, /saveRopes, /loadRopes, /listRopeSaves");
	messageClient(%client, '', "\c6README: <a:github.com/zapk/Server_Ropes>github.com/zapk/Server_Ropes</a>\c6.");
}

function serverCmdClearAllRopes(%client)
{
	if(!%client.isAdmin)
		return;

	%ropes = clearRopes();

	messageAll('MsgClearBricks', '\c3%1 \c0cleared all ropes. (%2)', %client.getPlayerName(), %ropes);
}

function serverCmdClearRopes(%client)
{
	%ropes = clearRopes(%client.getBLID());

	if(%ropes)
		messageAll('MsgClearBricks', '\c3%1 \c2cleared \c3%1\c2\'s ropes (%2)', %client.getPlayerName(), %ropes);
}

function serverCmdRopeTool(%client)
{
	if ($Pref::Server::Ropes::ToolAdminOnly && !%client.isAdmin)
	{
		messageClient(%client, '', "\c6The rope tool is admin only. Ask an admin for help.");
		return;
	}

	if (isObject(%client.minigame) && !%client.minigame.enablebuilding)
	{
		messageClient(%client, '', "\c6You cannot use the rope tool while building is disabled in your minigame.");
		return;
	}

	if (!isObject(%player = %client.player))
	{
		messageClient(%client, '', "\c6You must be spawned to equip the rope tool.");
		return;
	}

	%player.updateArm(ropeToolImage);
	%player.mountImage(ropeToolImage, 0);
}

function serverCmdSaveRopes(%client, %f0, %f1, %f2, %f3, %f4, %f5, %f6, %f7)
{
	if (!%client.isAdmin)
		return;

	%fileName = trim(%f0 SPC %f1 SPC %f2 SPC %f3 SPC %f4 SPC %f5 SPC %f6 SPC %f7);

	if (strlen(%fileName) < 1)
		return;

	%filePath = "config/server/RopeSaves/" @ %fileName @ ".blr";

	%error = saveTooledRopes(%filePath);
	if (%error !$= "")
		messageClient(%client, '', %error);
	else
		messageAll('MsgAdminForce', '\c3%1 \c2has saved all ropes to "\c3%2\c2"', %client.getPlayerName(), %fileName);
}

function serverCmdLoadRopes(%client, %f0, %f1, %f2, %f3, %f4, %f5, %f6, %f7)
{
	if (!%client.isAdmin)
		return;

	%fileName = trim(%f0 SPC %f1 SPC %f2 SPC %f3 SPC %f4 SPC %f5 SPC %f6 SPC %f7);

	if (strlen(%fileName) < 1)
		return;

	%filePath = "config/server/RopeSaves/" @ %fileName @ ".blr";

	%error = loadTooledRopes(%filePath);
	if (%error !$= "")
		messageClient(%client, '', %error);
	else
		messageAll('MsgAdminForce', '\c3%1 \c2has loaded ropes from "\c3%2\c2"', %client.getPlayerName(), %fileName);
}

function serverCmdListRopeSaves(%client)
{
	if (!%client.isAdmin)
		return;

	messageClient(%client, '', "\c6Available files:");
	%pattern = "config/server/RopeSaves/*.blr";
	for(%i = findFirstFile(%pattern); isFile(%i); %i = findNextFile(%pattern))
	{
		messageClient(%client, '', "\c6" @ fileBase(%i));
	}
}
