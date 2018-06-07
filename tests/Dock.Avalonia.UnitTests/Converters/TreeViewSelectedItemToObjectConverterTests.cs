// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Dock.Avalonia.Converters;
using Xunit;

namespace Dock.Avalonia.UnitTests.Converters
{
    public class TreeViewSelectedItemToObjectConverterTests
    {
        [Fact]
        public void TreeViewSelectedItemToObjectConverter_Ctor()
        {
            var actual = new TreeViewSelectedItemToObjectConverter();
            Assert.NotNull(actual);
        }
    }
}
