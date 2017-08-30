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
	talk("ropeToolImage hit object " @ %hitObj @ " at " @ %hitPos);
	ServerPlay3D("hammerHitSound", %hitPos);
}
