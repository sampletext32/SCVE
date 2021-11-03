﻿using SCVE.Core.Main;
using SCVE.Core.Rendering;
using SCVE.UI;
using SCVE.UI.UpToDate;

namespace SCVE.UITreeDebugger
{
    public class EngineRunnableTreeViewer : EngineRunnableUI
    {
        private EngineRunnableUI _analyzeUI;
        private EngineRunnableUI _selfUi;
        private FlexComponent selfFlex = new()
        {
            Direction = AlignmentDirection.Vertical
        };

        public EngineRunnableTreeViewer(EngineRunnableUI analyzeUI)
        {
            _analyzeUI = analyzeUI;
            AddChild(analyzeUI);
            _selfUi    = new EngineRunnableUI().WithBootstraped(selfFlex);
        }

        public override void Init()
        {
            _analyzeUI.Init();
            var visitor = new InfoAggregatorComponentVisitor(selfFlex);
            visitor.Accept(_analyzeUI);
            
            _selfUi.Init();
            this.Reflow();

            Engine.Instance.Input.WindowSizeChanged += InputOnWindowSizeChanged;
        }

        private void InputOnWindowSizeChanged(int arg1, int arg2)
        {
            _selfUi.Reflow();
            this.Reflow();
        }

        public void Render(IRenderer renderer)
        {
            RenderSelf(renderer);
        }

        public void Update(float deltaTime)
        {
            base.Update(deltaTime);
            _analyzeUI.Update(deltaTime);
        }

        public override void RenderSelf(IRenderer renderer)
        {
            _selfUi.Render(renderer);
        }
    }
}