using CommunityToolkit.Mvvm.ComponentModel;

namespace Surveying.Models
{
    /// <summary>
    /// UNIFIED PHOTO CLASS
    /// Replaces: PhotoItem (from PhotoItem.cs) + FlexiblePhotoItem (from PhotoUploadViewModel.cs)
    /// Combines: Basic photo properties + segmented upload features + file info
    /// </summary>
    public partial class Photo : ObservableObject, IDisposable
    {
        // ===== CORE PHOTO PROPERTIES (from PhotoItem) =====
        public FileResult FileResult { get; set; }
        public ImageSource ImageSource { get; set; }

        // ===== SEGMENTED UPLOAD PROPERTIES (from FlexiblePhotoItem) =====
        public string Segment { get; set; } = "General";       // For cleaning segments like "Top Outside", "Front Upper Half", etc.
        public DateTime CaptureDate { get; set; } = DateTime.Now;
        public string OriginalFileName { get; set; } = string.Empty;

        // ===== FILE INFO PROPERTIES (from FlexiblePhotoItem) =====
        public long FileSize { get; set; }
        public bool IsCompressed { get; set; }

        // ===== DESCRIPTION PROPERTIES =====
        public string Description { get; set; } = string.Empty;    // User can add notes about the photo

        // ===== COMPUTED PROPERTIES FOR UI =====
        public string FileSizeFormatted => FormatFileSize(FileSize);

        public string SegmentDisplay => string.IsNullOrEmpty(Segment) ? "General" : Segment;

        public string CaptureInfo => $"Taken: {CaptureDate:dd/MM/yyyy HH:mm}";

        public string FileInfo => !string.IsNullOrEmpty(OriginalFileName)
            ? $"{OriginalFileName} ({FileSizeFormatted})"
            : FileSizeFormatted;

        // ===== HELPER METHODS =====
        private string FormatFileSize(long bytes) => bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024:F1} KB",
            _ => $"{bytes / (1024 * 1024):F1} MB"
        };

        // ===== RESOURCE CLEANUP (from FlexiblePhotoItem) =====
        public void Dispose()
        {
            // Clean up any unmanaged resources if needed
            // FileResult and ImageSource will be garbage collected automatically
        }

        // ===== CONSTRUCTORS =====
        public Photo()
        {
        }

        public Photo(FileResult fileResult, ImageSource imageSource, string segment = "General")
        {
            FileResult = fileResult;
            ImageSource = imageSource;
            Segment = segment;
            OriginalFileName = fileResult?.FileName ?? string.Empty;
            CaptureDate = DateTime.Now;
        }

        // ===== CONVENIENCE METHODS =====
        public async Task<long> CalculateFileSizeAsync()
        {
            if (FileResult == null) return 0;

            try
            {
                using var stream = await FileResult.OpenReadAsync();
                FileSize = stream.Length;
                return FileSize;
            }
            catch
            {
                FileSize = 0;
                return 0;
            }
        }

        public bool BelongsToSegment(string segmentName)
        {
            return string.Equals(Segment, segmentName, StringComparison.OrdinalIgnoreCase);
        }
    }
}