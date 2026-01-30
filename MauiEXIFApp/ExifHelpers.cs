// ExifHelpers.cs

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace MauiEXIFApp;

public static class ExifHelpers
{
	public static T? GetValue<T>(this ExifProfile profile, ExifTag<T> tag)
	{
		if (profile.TryGetValue(tag, out var value) && value is not null && value.Value is not null)
		{
			return value.Value;
		}
		return default(T);
	}

	public static double GetDMSValue(this ExifProfile profile, ExifTag<Rational[]> dmsTag, ExifTag<string> dmsRefTag, string posRef, string negRef)
	{
		if (!profile.TryGetValue(dmsTag, out var dms) || dms is null || dms.Value is null)
		{
			return double.NaN;
		}

		double d = dms.Value[0].ToDouble();
		double m = dms.Value[1].ToDouble();
		double s = dms.Value[2].ToDouble();
		double dd = d + (m / 60.0) + (s / 3600.0);

		if (profile.TryGetValue(dmsRefTag, out var dmsRef)
			&& dmsRef is not null
			&& dmsRef.Value is not null
			&& dmsRef.Value.Equals(negRef, StringComparison.OrdinalIgnoreCase))
		{
			dd = -dd;
		}

		return dd;
	}
}
