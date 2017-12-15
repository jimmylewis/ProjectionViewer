﻿using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;

namespace ProjectionViewer
{
    /// <summary>
    /// Margin's canvas and visual definition including both size and content
    /// </summary>
    internal class ProjectionViewerMargin : Canvas, IWpfTextViewMargin
    {
        /// <summary>
        /// Margin name.
        /// </summary>
        public const string MarginName = "ProjectionViewMargin";

        /// <summary>
        /// A value indicating whether the object is disposed.
        /// </summary>
        private bool isDisposed;

        private ITextView _textView;
        private IProjectionBuffer _projectionBuffer;

        #region UI controls
        private Label _label;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorMargin1"/> class for a given <paramref name="textView"/>.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView"/> to attach the margin to.</param>
        public ProjectionViewerMargin(IWpfTextView textView)
        {
            _textView = textView;

            _projectionBuffer = _textView.TextBuffer as IProjectionBuffer;
            Debug.Assert(_projectionBuffer != null);

            this.Height = 20; // Margin height sufficient to have the label
            this.ClipToBounds = true;
            this.Background = new SolidColorBrush(Colors.LightGreen);

            // Add a green colored label that says "Hello EditorMargin1"
            _label = new Label
            {
                Background = new SolidColorBrush(Colors.LightGreen),
                Content = $"This text view contains {_projectionBuffer.SourceBuffers.Count} projection buffers.",
            };

            this.Children.Add(_label);

            _projectionBuffer.SourceBuffersChanged += OnSourceBuffersChanged;
        }

        private void OnSourceBuffersChanged(object sender, ProjectionSourceBuffersChangedEventArgs e)
        {
            _label.Content = $"This text view contains {_projectionBuffer.SourceBuffers.Count} projection buffers.";
        }

        #region IWpfTextViewMargin

        /// <summary>
        /// Gets the <see cref="Sytem.Windows.FrameworkElement"/> that implements the visual representation of the margin.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
        public FrameworkElement VisualElement
        {
            // Since this margin implements Canvas, this is the object which renders
            // the margin.
            get
            {
                this.ThrowIfDisposed();
                return this;
            }
        }

        #endregion

        #region ITextViewMargin

        /// <summary>
        /// Gets the size of the margin.
        /// </summary>
        /// <remarks>
        /// For a horizontal margin this is the height of the margin,
        /// since the width will be determined by the <see cref="ITextView"/>.
        /// For a vertical margin this is the width of the margin,
        /// since the height will be determined by the <see cref="ITextView"/>.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
        public double MarginSize
        {
            get
            {
                this.ThrowIfDisposed();

                // Since this is a horizontal margin, its width will be bound to the width of the text view.
                // Therefore, its size is its height.
                return this.ActualHeight;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the margin is enabled.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
        public bool Enabled
        {
            get
            {
                this.ThrowIfDisposed();

                // The margin should always be enabled
                return true;
            }
        }

        /// <summary>
        /// Gets the <see cref="ITextViewMargin"/> with the given <paramref name="marginName"/> or null if no match is found
        /// </summary>
        /// <param name="marginName">The name of the <see cref="ITextViewMargin"/></param>
        /// <returns>The <see cref="ITextViewMargin"/> named <paramref name="marginName"/>, or null if no match is found.</returns>
        /// <remarks>
        /// A margin returns itself if it is passed its own name. If the name does not match and it is a container margin, it
        /// forwards the call to its children. Margin name comparisons are case-insensitive.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="marginName"/> is null.</exception>
        public ITextViewMargin GetTextViewMargin(string marginName)
        {
            return string.Equals(marginName, ProjectionViewerMargin.MarginName, StringComparison.OrdinalIgnoreCase) ? this : null;
        }

        /// <summary>
        /// Disposes an instance of <see cref="EditorMargin1"/> class.
        /// </summary>
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                if(_projectionBuffer != null)
                {
                    _projectionBuffer.SourceBuffersChanged -= OnSourceBuffersChanged;
                }

                GC.SuppressFinalize(this);
                this.isDisposed = true;
            }
        }

        #endregion

        /// <summary>
        /// Checks and throws <see cref="ObjectDisposedException"/> if the object is disposed.
        /// </summary>
        private void ThrowIfDisposed()
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException(MarginName);
            }
        }
    }
}
