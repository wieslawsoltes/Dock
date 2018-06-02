// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Model;

namespace AvaloniaDemo.CodeGen
{
    /// <summary>
    /// Code generator contract.
    /// </summary>
    public interface ICodeGen
    {
        /// <summary>
        /// Generates code view from view model.
        /// </summary>
        /// <param name="view">The view from code is generated.</param>
        /// <param name="path">The output file path.</param>
        void Generate(IView view, string path);
    }
}
