﻿using OpenTK.Windowing.GraphicsLibraryFramework;
using SCVE.Core.Input;

namespace SCVE.OpenTKBindings
{
    public static class CodeExtensions
    {
        public static KeyCode ToScveCode(this Keys keys)
        {
            // since scve keycode is a direct copy - this conversion is appropriate
            return (KeyCode)keys;
        }
        
        public static MouseCode ToScveCode(this MouseButton button)
        {
            // since scve mousecode is a direct copy - this conversion is appropriate
            return (MouseCode)button;
        }
    }
}