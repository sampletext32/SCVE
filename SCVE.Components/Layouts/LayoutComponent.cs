﻿using System.Collections.Generic;
using SCVE.Core;
using SCVE.Core.Rendering;

namespace SCVE.Components.Layouts
{
    public abstract class LayoutComponent : Component
    {
        protected List<Divider> Dividers { get; set; } = new();

        /// <summary>
        /// Method is supposed to resize first depth level children 
        /// </summary>
        protected abstract void ConstraintChildren();

        protected void ValidateDividersCount()
        {
            if (Children.Count <= 1)
            {
                return;
            }
            
            // Divider is between two children so there must be children-1 dividers
            if (Dividers.Count > Children.Count - 1)
            {
                Dividers.RemoveRange(Children.Count - 1, Children.Count - 1 - Dividers.Count);
            }

            if (Dividers.Count < Children.Count - 1)
            {
                for (var i = 0; i < Children.Count - 1 - Dividers.Count; i++)
                {
                    Dividers.Add(new Divider());
                }
            }
        }

        protected override void OnResized()
        {
            base.OnResized();
            ConstraintChildren();
        }

        public override void AddChild(Component component)
        {
            if (component is not LayoutCell cell)
            {
                component = new LayoutCell(component);
            }

            base.AddChild(component);
            ConstraintChildren();
        }

        public override void Render(IRenderer renderer)
        {
            foreach (var child in Children)
            {
                child.Render(renderer);
            }

            foreach (var divider in Dividers)
            {
                divider.Render(renderer);
            }
        }
    }
}