using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Text.Projection;

namespace ProjectionViewer
{
    /// <summary>
    /// TextAdornment1 places red boxes behind all the "a"s in the editor window
    /// </summary>
    internal sealed class ProjectionHighlightAdornment
    {
        /// <summary>
        /// The layer of the adornment.
        /// </summary>
        private readonly IAdornmentLayer _layer;

        /// <summary>
        /// Text view where the adornment is created.
        /// </summary>
        private readonly IWpfTextView _view;

        private readonly IProjectionBuffer _projectionBuffer;

        /// <summary>
        /// Adornment brush.
        /// </summary>
        private readonly Brush[] _brushes;

        /// <summary>
        /// Adornment pen.
        /// </summary>
        private readonly Pen[] _pens;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionHighlightAdornment"/> class.
        /// </summary>
        /// <param name="view">Text view to create the adornment for</param>
        public ProjectionHighlightAdornment(IWpfTextView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            _layer = view.GetAdornmentLayer(ProjectionAdornmentTextViewCreationListener.LayerName);

            _view = view;
            _projectionBuffer = view.TextBuffer as IProjectionBuffer;
            _view.LayoutChanged += OnLayoutChanged;

            // Create the pen and brush to color the box behind the a's
            _brushes = new Brush[5];
            _pens = new Pen[5];
            _brushes[0] = new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0x00, 0xff));
            _brushes[0].Freeze();
            _pens[0] = new Pen(new SolidColorBrush(Color.FromRgb(0x00, 0x00, 0xff)), 0.5);
            _pens[0].Freeze();

            _brushes[1] = new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0xff, 0x00));
            _brushes[1].Freeze();
            _pens[1] = new Pen(new SolidColorBrush(Color.FromRgb(0x00, 0xff, 0x00)), 0.5);
            _pens[1].Freeze();

            _brushes[2] = new SolidColorBrush(Color.FromArgb(0x20, 0xff, 0x00, 0x00));
            _brushes[2].Freeze();
            _pens[2] = new Pen(new SolidColorBrush(Color.FromRgb(0xff, 0x00, 0x00)), 0.5);
            _pens[2].Freeze();

            _brushes[3] = new SolidColorBrush(Color.FromArgb(0x20, 0xff, 0x00, 0xff));
            _brushes[3].Freeze();
            _pens[3] = new Pen(new SolidColorBrush(Color.FromRgb(0xff, 0x00, 0xff)), 0.5);
            _pens[3].Freeze();

            _brushes[4] = new SolidColorBrush(Color.FromArgb(0x20, 0x00, 0xff, 0xff));
            _brushes[4].Freeze();
            _pens[4] = new Pen(new SolidColorBrush(Color.FromRgb(0x00, 0xff, 0xff)), 0.5);
            _pens[4].Freeze();
        }

        /// <summary>
        /// Handles whenever the text displayed in the view changes by adding the adornment to any reformatted lines
        /// </summary>
        /// <remarks><para>This event is raised whenever the rendered text displayed in the <see cref="ITextView"/> changes.</para>
        /// <para>It is raised whenever the view does a layout (which happens when DisplayTextLineContainingBufferPosition is called or in response to text or classification changes).</para>
        /// <para>It is also raised whenever the view scrolls horizontally or when its size changes.</para>
        /// </remarks>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        internal void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            foreach(var span in e.NewOrReformattedSpans)
            {
                CreateVisuals(span);
            }
        }

        /// <summary>
        /// Adds the scarlet box behind the 'a' characters within the given line
        /// </summary>
        /// <param name="line">Line to add the adornments</param>
        private void CreateVisuals(SnapshotSpan span)
        {
            var sourceSpans = _projectionBuffer.CurrentSnapshot.MapToSourceSnapshots(span.Span);
            var allSourceSpans = _projectionBuffer.CurrentSnapshot.GetSourceSpans();

            int allSpansIndex = 0;
            for(int i = 0; i < sourceSpans.Count; i++)
            {
                for(int j = allSpansIndex; j < allSourceSpans.Count; j++)
                {
                    if(allSourceSpans[j].Snapshot == sourceSpans[i].Snapshot)
                    {
                        if(allSourceSpans[j].IntersectsWith(sourceSpans[i]))
                        {
                            var viewSpan = _projectionBuffer.CurrentSnapshot.MapFromSourceSnapshot(allSourceSpans[j]).Single();
                            var viewSnapshotSpan = new SnapshotSpan(_projectionBuffer.CurrentSnapshot, viewSpan);
                            // remove any existing adornments for this span before applying a new one5
                            _layer.RemoveAdornmentsByVisualSpan(viewSnapshotSpan);

                            int colorIndex = GetColorIndex(allSourceSpans[j]);
                            Image image = GetAdornmentImage(viewSnapshotSpan, _brushes[colorIndex], _pens[colorIndex]);
                            _layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, viewSnapshotSpan, null, image, null);

                            allSpansIndex++;
                            break;
                        }
                    }
                }
            }
        }

        private int GetColorIndex(SnapshotSpan sourceSpan)
        {
            int sourceBufferIndex = 0;
            for (int i = 0; i < _projectionBuffer.SourceBuffers.Count && i < _brushes.Length; i++)
            {
                if (sourceSpan.Snapshot.TextBuffer == _projectionBuffer.SourceBuffers[i])
                {
                    sourceBufferIndex = i;
                    break;
                }
            }

            return sourceBufferIndex;
        }

        private Image GetAdornmentImage(SnapshotSpan adornmentSpan, Brush brush, Pen pen)
        {
            Image image = null;
            Geometry geometry = _view.TextViewLines.GetMarkerGeometry(adornmentSpan);
            if (geometry != null)
            {
                var drawing = new GeometryDrawing(brush, pen, geometry);
                drawing.Freeze();

                var drawingImage = new DrawingImage(drawing);
                drawingImage.Freeze();

                image = new Image
                {
                    Source = drawingImage,
                };

                // Align the image with the top of the bounds of the text geometry
                Canvas.SetLeft(image, geometry.Bounds.Left);
                Canvas.SetTop(image, geometry.Bounds.Top);
            }

            return image;
        }
    }
}
