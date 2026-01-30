// MainViewModel.cs

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiEXIFApp;

public partial class MainViewModel : ObservableObject
{
	[ObservableProperty]
	public partial string MauiVersion { get; set; }

	[ObservableProperty]
	public partial string FileName { get; set; } = "No photo selected";

	[ObservableProperty]
	public partial string FilePath { get; set; }

	[ObservableProperty]
	public partial string ExifText { get; set; } = "No EXIF data available";

	[ObservableProperty]
	public partial string MessageText { get; set; } = string.Empty;

	public void ReadMauiVersionFromAssembly(System.Reflection.Assembly assembly)
	{
		var attrs = System.Reflection.CustomAttributeExtensions.GetCustomAttributes<System.Reflection.AssemblyMetadataAttribute>(assembly);
		MauiVersion = attrs.FirstOrDefault(a => a.Key == "ExtraAppInfo.MauiVersion")?.Value ?? "Unknown";
	}

	[RelayCommand]
	public async Task PickPhotoAndShowExifAsync()
	{
		try
		{
			MessageText = string.Empty;
			FileName = "No photo selected";
			FilePath = string.Empty;
			ExifText = "No EXIF data available";

			var status = await Permissions.RequestAsync<AccessMediaLocation>();
			if (status != PermissionStatus.Granted)
			{
				MessageText = "Permission to access media location was denied.";
				ExifText = string.Empty;
				return;
			}

			var options = new MediaPickerOptions
			{
				Title = "Please pick a photo",
			};

#if MAUI_10_0_OR_GREATER
			var results = await MediaPicker.PickPhotosAsync(options);
			ArgumentNullException.ThrowIfNull(results, nameof(results));
			if (results.Count < 1)
			{
				MessageText = "Error: No photo was selected.";
				return;
			}
			var result = results[0];
#else
			var result = await MediaPicker.PickPhotoAsync(options);
			ArgumentException.ThrowIfNullOrEmpty(result?.FullPath, nameof(result));
#endif

			FileName = result.FileName;
			FilePath = result.FullPath;

			var stream = await result.OpenReadAsync();
			SixLabors.ImageSharp.ImageInfo? imageInfo = SixLabors.ImageSharp.Image.Identify(stream);
			SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifProfile? profile = imageInfo?.Metadata?.ExifProfile;

			if (imageInfo == null)
			{
				MessageText = "Error: No image info available.";
				return;
			}

			ExifText =
				$$"""
				Latitude: {{profile?.GetDMSValue(
								SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.GPSLatitude,
								SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.GPSLatitudeRef, "N", "S").ToString("F6") ?? "N/A"}}
				Longitude: {{profile?.GetDMSValue(
								SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.GPSLongitude,
								SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.GPSLongitudeRef, "E", "W").ToString("F6") ?? "N/A"}}
				Image Width: {{profile?.GetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.ImageWidth)}}
				Image Length: {{profile?.GetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.ImageLength)}}
				Focal Length: {{profile?.GetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.FocalLength)}}
				Date Taken: {{profile?.GetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.DateTimeOriginal)}}
				Camera Make: {{profile?.GetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Make)}}
				Camera Model: {{profile?.GetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Model)}}
				Orientation: {{profile?.GetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Orientation)}}
				User Comment: {{profile?.GetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.UserComment)}}
				Artist: {{profile?.GetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Artist)}}
				""";
		}
		catch (Exception ex)
		{
			MessageText = $"Error: {ex.Message}";
		}
	}
}
