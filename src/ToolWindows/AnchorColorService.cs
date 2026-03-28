using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using CommentsVS.Classification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace CommentsVS.ToolWindows
{
    /// <summary>
    /// Service that provides colors for anchor types from the classification format map.
    /// </summary>
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    internal class AnchorColorService
    {
        private readonly IClassificationFormatMapService _formatMapService;
        private readonly IClassificationTypeRegistryService _classificationTypeRegistry;
        private readonly Dictionary<AnchorType, Color> _cachedColors = new();
        private IClassificationFormatMap _formatMap;

        [ImportingConstructor]
        public AnchorColorService(
            IClassificationFormatMapService formatMapService,
            IClassificationTypeRegistryService classificationTypeRegistry)
        {
            _formatMapService = formatMapService;
            _classificationTypeRegistry = classificationTypeRegistry;
        }

        /// <summary>
        /// Gets the color for an anchor type from the classification format map.
        /// </summary>
        public Color GetColor(AnchorType anchorType)
        {
            // Initialize format map on first use
            if (_formatMap == null)
            {
                _formatMap = _formatMapService.GetClassificationFormatMap("text");
                _formatMap.ClassificationFormatMappingChanged += OnFormatMappingChanged;
            }

            // Check cache first
            if (_cachedColors.TryGetValue(anchorType, out Color cachedColor))
            {
                return cachedColor;
            }

            // Get color from classification format map
            var color = GetColorFromFormatMap(anchorType);
            _cachedColors[anchorType] = color;
            return color;
        }

        private void OnFormatMappingChanged(object sender, System.EventArgs e)
        {
            // Clear cache when colors change
            _cachedColors.Clear();
        }

        private Color GetColorFromFormatMap(AnchorType anchorType)
        {
            var classificationTypeName = GetClassificationTypeName(anchorType);
            if (string.IsNullOrEmpty(classificationTypeName))
            {
                return GetFallbackColor(anchorType);
            }

            var classificationType = _classificationTypeRegistry.GetClassificationType(classificationTypeName);
            if (classificationType == null)
            {
                return GetFallbackColor(anchorType);
            }

            var properties = _formatMap.GetTextProperties(classificationType);
            if (properties != null && !properties.ForegroundBrushEmpty && properties.ForegroundBrush is SolidColorBrush brush)
            {
                return brush.Color;
            }

            return GetFallbackColor(anchorType);
        }

        private static string GetClassificationTypeName(AnchorType anchorType)
        {
            return anchorType switch
            {
                AnchorType.Todo => CommentTagClassificationTypes.Todo,
                AnchorType.Hack => CommentTagClassificationTypes.Hack,
                AnchorType.Note => CommentTagClassificationTypes.Note,
                AnchorType.Bug => CommentTagClassificationTypes.Bug,
                AnchorType.Fixme => CommentTagClassificationTypes.Fixme,
                AnchorType.Undone => CommentTagClassificationTypes.Undone,
                AnchorType.Review => CommentTagClassificationTypes.Review,
                AnchorType.Anchor => CommentTagClassificationTypes.Anchor,
                AnchorType.Custom => CommentTagClassificationTypes.Custom,
                _ => null
            };
        }

        private static Color GetFallbackColor(AnchorType anchorType)
        {
            return anchorType switch
            {
                AnchorType.Todo => Colors.Orange,
                AnchorType.Hack => Colors.Crimson,
                AnchorType.Note => Colors.LimeGreen,
                AnchorType.Bug => Colors.Red,
                AnchorType.Fixme => Colors.OrangeRed,
                AnchorType.Undone => Colors.MediumPurple,
                AnchorType.Review => Colors.DodgerBlue,
                AnchorType.Anchor => Colors.Teal,
                AnchorType.Custom => Colors.Goldenrod,
                _ => Colors.Gray
            };
        }
    }
}