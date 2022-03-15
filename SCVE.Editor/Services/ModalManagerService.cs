﻿using System;
using SCVE.Editor.Abstractions;
using SCVE.Editor.ImGuiUi;
using SCVE.Editor.ImGuiUi.Panels;

namespace SCVE.Editor.Services
{
    public class ModalManagerService : IService
    {
        private readonly ProjectCreationPanel _projectCreationPanel;
        private readonly SequenceCreationPanel _sequenceCreationPanel;
        private readonly FilePickerModalPanel _filePickerModalPanel;
        private readonly SettingsModalPanel _settingsModalPanel;

        public ModalManagerService(
            ProjectCreationPanel projectCreationPanel,
            SequenceCreationPanel sequenceCreationPanel,
            FilePickerModalPanel filePickerModalPanel,
            SettingsModalPanel settingsModalPanel)
        {
            _projectCreationPanel = projectCreationPanel;
            _sequenceCreationPanel = sequenceCreationPanel;
            _filePickerModalPanel = filePickerModalPanel;
            _settingsModalPanel = settingsModalPanel;
        }

        public string FilePickerSelectedPath => _filePickerModalPanel.SelectedPath;

        public void OpenProjectCreationPanel(Action closed = null, Action dismissed = null)
        {
            _projectCreationPanel.Open(closed, dismissed);
        }

        public void OpenSequenceCreationPanel(Action closed = null, Action dismissed = null)
        {
            _sequenceCreationPanel.Open(closed, dismissed);
        }

        public void OpenFilePickerPanel(string location, Action closed = null, Action dismissed = null)
        {
            _filePickerModalPanel.Open(location, closed, dismissed);
        }

        public void OpenSettingsPanel(Action closed = null, Action dismissed = null)
        {
            _settingsModalPanel.LoadDraft();
            _settingsModalPanel.Open(closed, dismissed);
        }
    }
}