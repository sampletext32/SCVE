﻿using System.Collections.Generic;
using SCVE.Editor.Editing;

namespace SCVE.Editor.ProjectStructure
{
    public class VideoProject
    {
        public ICollection<ProjectSequenceAsset> Sequences { get; set; }
        public ICollection<ProjectImageAsset> Images { get; set; }

        public VideoProject()
        {
        }

        private Sequence LoadSequence(string location, string name)
        {
            return null;
        }

        private Image LoadImage()
        {
            return null;
        }
    }
}