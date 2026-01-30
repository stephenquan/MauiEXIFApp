// AccessMediaLocation.cs

namespace MauiEXIFApp;

public class AccessMediaLocation : Permissions.BasePlatformPermission
{
#if ANDROID
	public override (string androidPermission, bool isRuntime)[] RequiredPermissions
		=> new (string, bool)[] { (Android.Manifest.Permission.AccessMediaLocation, true) };
#endif
}
