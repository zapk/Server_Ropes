function saveTooledRopes(%filePath)
{
	%file = new fileObject();

	if (!%file.openForWrite(%filePath))
	{
		%file.delete();
		return "Cannot write to file.";
	}

	%file.writeLine("VERSION\t1");
	%file.writeLine("DATE" TAB getDateTime());

	%nSaved = 0;
	for(%i = 0; %i < MainRopeGroup.getCount(); %i++)
	{
		%rg = MainRopeGroup.getObject(%i);

		if (%rg.madeFromTool)
		{
			%nSaved++;
			%file.writeLine(%rg.bl_id TAB %rg.savePosA TAB %rg.savePosB TAB %rg.saveColor TAB %rg.saveDiameter TAB %rg.saveSlack);
		}
	}

	%file.close();
	%file.delete();

	if (!%nSaved)
	{
		fileDelete(%filePath);
		return "No ropes to save.";
	}

	return "";
}

function loadTooledRopes(%filePath)
{
	%file = new fileObject();

	if (!%file.openForRead(%filePath))
	{
		%file.delete();
		return "Cannot read file.";
	}

	clearRopes();

	%i = 0;
	while (!%file.isEOF())
	{
		%line = %file.readLine();

		switch$(getField(%line, 0))
		{
			case "VERSION":
				%version = getField(%line, 1);

			case "DATE":
				%date = getField(%line, 1);

			default:
				%bl_id = getField(%line, 0);
				%posA = getField(%line, 1);
				%posB = getField(%line, 2);
				%color = getField(%line, 3);
				%diameter = getField(%line, 4);
				%slack = getField(%line, 5);

				%group = _getRopeGroup("load_" @ %i, %bl_id, "");

				createRope(%posA, %posB, %color, %diameter, %slack, %group);

				%group.madeFromTool = true;
				%group.loadedFromFile = true;
				%group.savePosA = %posA;
				%group.savePosB = %posB;
				%group.saveColor = %color;
				%group.saveDiameter = %diameter;
				%group.saveSlack = %slack;

				%i++;
		}
	}

	%file.close();
	%file.delete();

	return "";
}
