﻿using System;
using System.Numerics;
using ImGuiNET;
using SCVE.Editor.Editing.Editing;
using SCVE.Editor.Imaging;
using SCVE.Editor.Services;

namespace SCVE.Editor.ImGuiUi
{
    public class SequencePanel : IImGuiRenderable
    {
        private ClipImGuiRenderer _clipRenderer;

        private readonly EditingService _editingService;
        private readonly PreviewService _previewService;

        public SequencePanel(EditingService editingService, PreviewService previewService)
        {
            _editingService = editingService;
            _previewService = previewService;
            _clipRenderer = new ClipImGuiRenderer();
        }

        private bool _isDraggingClip;
        private bool _isDraggingCursor;

        private readonly GhostClip _ghostClip = GhostClip.CreateNew(0, 1);

        private int _cursorDragFrames = 0;

        private static readonly Vector2 _cursorSize = new(10, 20);

        // clockwise order
        private readonly Vector2[] _cursorShapePoints =
        {
            // top-left
            new(),
            // top-right
            new(_cursorSize.X, 0),
            // right mid
            new(_cursorSize.X, _cursorSize.Y / 2f),
            // bottom mid
            new(_cursorSize.X / 2, _cursorSize.Y),
            // left mid
            new(0, _cursorSize.Y / 2f),
        };

        private readonly Vector2[] _cursorCurrentPoints = new Vector2[5];

        public void OnImGuiRender()
        {
            if (!ImGui.Begin("Sequence Panel"))
            {
                goto END;
            }

            if (_editingService.OpenedSequence is null)
            {
                ImGui.Text("No sequence is opened");
                goto END;
            }

            var windowPos = ImGui.GetWindowPos();

            var painter = ImGui.GetWindowDrawList();

            var contentRegionMin = ImGui.GetWindowContentRegionMin();

            var contentRegionAvail = ImGui.GetContentRegionAvail();

            var drawOriginX = windowPos.X + contentRegionMin.X;
            var drawOriginY = windowPos.Y + contentRegionMin.Y;

            var windowContentWidth = contentRegionAvail.X;

            var trackHeight = 20;

            var trackMargin = 5;

            var trackHeaderWidth = 70;
            var trackContentWidth = windowContentWidth - trackHeaderWidth;

            var sequenceHeaderHeight = 20;

            var timelineFramesMarkerHeight = 3;
            var timelineSecondsMarkerHeight = 8;

            var widthPerFrame = trackContentWidth / _editingService.OpenedSequence.FrameLength;
            var sequenceFPS = _editingService.OpenedSequence.FPS;
            var sequenceCursorTimeFrame = _editingService.OpenedSequence.CursorTimeFrame;

            var sequenceFrameLength = _editingService.OpenedSequence.FrameLength;

            #region Detect click on timeline (not on cursor)

            ImGui.SetCursorPos(new Vector2(drawOriginX + trackHeaderWidth, drawOriginY) - windowPos);
            ImGui.SetItemAllowOverlap();
            ImGui.InvisibleButton($"##timeline-header",
                new Vector2(windowContentWidth - trackHeaderWidth, sequenceHeaderHeight));

            if (ImGui.IsItemActive())
            {
                int timelineClickedFrame =
                    (int) ((ImGui.GetMousePos().X - drawOriginX - trackHeaderWidth) / widthPerFrame);
                if (sequenceCursorTimeFrame != timelineClickedFrame)
                {
                    _editingService.OpenedSequence.CursorTimeFrame =
                        Math.Clamp(timelineClickedFrame, 0, sequenceFrameLength);

                    // no sequence data has changed, so we just need to preview new frame
                    _previewService.SyncVisiblePreview();
                }
            }

            #endregion

            #region Sequence header

            painter.AddRectFilled(
                new Vector2(drawOriginX + trackHeaderWidth, drawOriginY),
                new Vector2(drawOriginX + windowContentWidth, drawOriginY + sequenceHeaderHeight),
                0xFF333333
            );

            #endregion

            #region Timeline frames markers

            for (int i = 0; i < sequenceFrameLength; i++)
            {
                int markerStripHeight;
                if (i % sequenceFPS == 0)
                {
                    markerStripHeight = timelineSecondsMarkerHeight;
                    var text = $"{i / sequenceFPS}";
                    var textSize = ImGui.CalcTextSize(text);

                    // seconds text markers
                    painter.AddText(
                        new Vector2(drawOriginX + trackHeaderWidth + i * widthPerFrame - textSize.X / 2,
                            drawOriginY + ((sequenceHeaderHeight - timelineSecondsMarkerHeight) / 2f) -
                            textSize.Y / 2),
                        0xFFFFFFFF,
                        text
                    );
                }
                else
                {
                    markerStripHeight = timelineFramesMarkerHeight;
                }

                painter.AddLine(
                    new Vector2(drawOriginX + trackHeaderWidth + i * widthPerFrame,
                        drawOriginY + sequenceHeaderHeight - markerStripHeight),
                    new Vector2(drawOriginX + trackHeaderWidth + i * widthPerFrame,
                        drawOriginY + sequenceHeaderHeight),
                    0xFFFFFFFF
                );

                if (_previewService.HasCached(i, ImagePresence.GPU))
                {
                    painter.AddRectFilled(
                        new Vector2(drawOriginX + trackHeaderWidth + i * widthPerFrame + 1,
                            drawOriginY + sequenceHeaderHeight - markerStripHeight),
                        new Vector2(drawOriginX + trackHeaderWidth + (i + 1) * widthPerFrame - 1,
                            drawOriginY + sequenceHeaderHeight),
                        0xFF00FF00
                    );
                }
                else if (_previewService.HasCached(i, ImagePresence.DISK))
                {
                    painter.AddRectFilled(
                        new Vector2(drawOriginX + trackHeaderWidth + i * widthPerFrame + 1,
                            drawOriginY + sequenceHeaderHeight - markerStripHeight),
                        new Vector2(drawOriginX + trackHeaderWidth + (i + 1) * widthPerFrame - 1,
                            drawOriginY + sequenceHeaderHeight),
                        0xFFFF0000
                    );
                }
            }

            #endregion

            #region Cursor

            var cursorPosition =
                new Vector2(
                    drawOriginX + trackHeaderWidth + (sequenceCursorTimeFrame + _cursorDragFrames) * widthPerFrame -
                    _cursorSize.X / 2, drawOriginY);

            ImGui.SetCursorPos(cursorPosition - windowPos);
            ImGui.SetItemAllowOverlap();
            ImGui.InvisibleButton($"##cursor", _cursorSize);

            if (ImGui.IsItemActive())
            {
                var mouseDragDelta = ImGui.GetMouseDragDelta();
                _cursorDragFrames = (int) (mouseDragDelta.X / widthPerFrame);
                _cursorDragFrames = Math.Clamp(_cursorDragFrames, -sequenceCursorTimeFrame,
                    sequenceFrameLength - sequenceCursorTimeFrame);
                _isDraggingCursor = true;
            }

            for (var i = 0; i < _cursorShapePoints.Length; i++)
            {
                _cursorCurrentPoints[i].X = _cursorShapePoints[i].X + cursorPosition.X;
                _cursorCurrentPoints[i].Y = _cursorShapePoints[i].Y + cursorPosition.Y;
            }

            painter.AddConvexPolyFilled(ref _cursorCurrentPoints[0], 5, 0xFFAA6666);

            #endregion

            for (var i = 0; i < _editingService.OpenedSequence.Tracks.Count; i++)
            {
                var track = _editingService.OpenedSequence.Tracks[i];

                // Track header
                painter.AddRectFilled(
                    new Vector2(drawOriginX, drawOriginY + sequenceHeaderHeight + i * (trackHeight + trackMargin)),
                    new Vector2(drawOriginX + trackHeaderWidth,
                        drawOriginY + sequenceHeaderHeight + (i + 1) * trackHeight + i * trackMargin),
                    0xFF444444
                );

                painter.AddText(
                    new Vector2(drawOriginX, drawOriginY + sequenceHeaderHeight + i * (trackHeight + trackMargin)),
                    0xFFFFFFFF, $"TRACK {i}");

                // track content background
                painter.AddRectFilled(
                    new Vector2(drawOriginX + trackHeaderWidth,
                        drawOriginY + sequenceHeaderHeight + i * (trackHeight + trackMargin)),
                    new Vector2(drawOriginX + contentRegionAvail.X,
                        drawOriginY + sequenceHeaderHeight + (i + 1) * trackHeight + i * trackMargin),
                    0xFF222222
                );

                for (int j = 0; j < _editingService.OpenedSequence.Tracks[i].EmptyClips.Count; j++)
                {
                    var clip = _editingService.OpenedSequence.Tracks[i].EmptyClips[j];

                    var clipTopLeft = new Vector2(
                        drawOriginX + trackHeaderWidth +
                        trackContentWidth * ((float) (clip.StartFrame) / sequenceFrameLength),
                        drawOriginY + sequenceHeaderHeight + (i) * (trackHeight + trackMargin)
                    );
                    var clipBottomRight = new Vector2(
                        drawOriginX + trackHeaderWidth + trackContentWidth *
                        ((float) (clip.StartFrame + clip.FrameLength) / sequenceFrameLength),
                        drawOriginY + sequenceHeaderHeight + (i + 1) * trackHeight + (i) * trackMargin
                    );

                    _clipRenderer.Render(ref painter, clip, ref clipTopLeft, ref clipBottomRight);

                    ImGui.SetCursorPos(clipTopLeft - windowPos);
                    ImGui.SetItemAllowOverlap();
                    if (ImGui.InvisibleButton($"##clip{clip.Guid:N}",
                            new Vector2(clipBottomRight.X - clipTopLeft.X, clipBottomRight.Y - clipTopLeft.Y)))
                    {
                        _editingService.SelectedClip = clip;
                    }

                    if (ImGui.IsItemActive())
                    {
                        _ghostClip.ReferencedClip = clip;
                        _ghostClip.SourceTrackIndex = i;
                        _ghostClip.SourceStartFrame = clip.StartFrame;
                        var mouseDragDelta = ImGui.GetMouseDragDelta();

                        int deltaTracks = (int) (mouseDragDelta.Y / (trackHeight + trackMargin));

                        int newTrackId = i + deltaTracks;
                        if (newTrackId >= 0 &&
                            newTrackId < _editingService.OpenedSequence.Tracks.Count)
                        {
                            _ghostClip.CurrentTrackIndex = newTrackId;
                        }
                        else
                        {
                            _ghostClip.CurrentTrackIndex = i;
                        }

                        _ghostClip.FrameLength = clip.FrameLength;

                        _ghostClip.StartFrame = clip.StartFrame + (int) (mouseDragDelta.X / widthPerFrame);

                        if (_ghostClip.StartFrame < 0)
                        {
                            _ghostClip.StartFrame = 0;
                        }
                        else if (_ghostClip.EndFrame >= sequenceFrameLength)
                        {
                            _ghostClip.StartFrame = sequenceFrameLength - _ghostClip.FrameLength;
                        }

                        _ghostClip.Visible = true;
                        _isDraggingClip = true;
                    }
                }
            }

            if (_ghostClip.Visible)
            {
                var clip = _ghostClip;
                var clipTopLeft = new Vector2(
                    drawOriginX + trackHeaderWidth +
                    trackContentWidth * ((float) (clip.StartFrame) / sequenceFrameLength),
                    drawOriginY + sequenceHeaderHeight + (_ghostClip.CurrentTrackIndex) * (trackHeight + trackMargin)
                );
                var clipBottomRight = new Vector2(
                    drawOriginX + trackHeaderWidth + trackContentWidth *
                    ((float) (clip.StartFrame + clip.FrameLength) / sequenceFrameLength),
                    drawOriginY + sequenceHeaderHeight + (_ghostClip.CurrentTrackIndex + 1) * trackHeight +
                    (_ghostClip.CurrentTrackIndex) * trackMargin
                );

                _clipRenderer.RenderGhost(ref painter, clip, ref clipTopLeft, ref clipBottomRight);
            }

            if (_isDraggingClip)
            {
                if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    // TODO: Check overlap with other clips in current track
                    if (_ghostClip.SourceTrackIndex != _ghostClip.CurrentTrackIndex)
                    {
                        if (_ghostClip.ReferencedClip is EmptyClip empty)
                        {
                            _editingService.OpenedSequence.Tracks[_ghostClip.SourceTrackIndex].EmptyClips.Remove(empty);
                            _editingService.OpenedSequence.Tracks[_ghostClip.CurrentTrackIndex].EmptyClips.Add(empty);
                        }
                        else if (_ghostClip.ReferencedClip is AssetClip asset)
                        {
                            _editingService.OpenedSequence.Tracks[_ghostClip.SourceTrackIndex].AssetClips.Remove(asset);
                            _editingService.OpenedSequence.Tracks[_ghostClip.CurrentTrackIndex].AssetClips.Add(asset);
                        }
                    }

                    if (_ghostClip.StartFrame != _ghostClip.SourceStartFrame)
                    {
                        _previewService.InvalidateRange(_ghostClip.SourceStartFrame, _ghostClip.FrameLength);
                        _previewService.InvalidateRange(_ghostClip.StartFrame, _ghostClip.FrameLength);
                        _ghostClip.ReferencedClip.StartFrame = _ghostClip.StartFrame;
                    }

                    _ghostClip.Visible = false;
                    _ghostClip.ReferencedClip = null;
                    _isDraggingClip = false;
                }
            }

            if (_isDraggingCursor)
            {
                if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {
                    _editingService.OpenedSequence.CursorTimeFrame += _cursorDragFrames;
                    _cursorDragFrames = 0;
                    _isDraggingCursor = false;

                    _previewService.SyncVisiblePreview();
                }
            }

            END:
            ImGui.End();
        }
    }
}