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

function solveRopeDrop(%posA, %vec, %dist, %iter, %slack, %diameter, %Iterations)
{
	%rawPos = vectorAdd( %posA, vectorScale( %vec, %dist * (%iter / %iterations) ) );
	%endPos = vectorSub( %rawPos, VectorScale( "0 0 1", mSin(($pi / %iterations) * %iter) * %slack ) );

	%ray = containerRaycast( %rawPos, VectorSub( %endPos, VectorScale( "0 0 1", %diameter / 2 ) ), $Typemasks::FxBrickObjectType | $Typemasks::TerrainObjectType );
	%hit = firstWord( %ray );

	if(isObject(%hit))
	{
		%endPos = VectorAdd( getWords(%ray, 1, 3), VectorScale( getWords(%ray, 4, 6), %diameter / 2 ) );
	}

	return %endPos;
}

function rgbToHex( %rgb )
{
	%r = _compToHex( 255 * getWord( %rgb, 0 ) );
	%g = _compToHex( 255 * getWord( %rgb, 1 ) );
	%b = _compToHex( 255 * getWord( %rgb, 2 ) );

	return %r @ %g @ %b;
}

function _compToHex( %comp )
{
	%left = mFloor( %comp / 16 );
	%comp = mFloor( %comp - %left * 16 );

	%left = getSubStr( "0123456789ABCDEF", %left, 1 );
	%comp = getSubStr( "0123456789ABCDEF", %comp, 1 );

	return %left @ %comp;
}
