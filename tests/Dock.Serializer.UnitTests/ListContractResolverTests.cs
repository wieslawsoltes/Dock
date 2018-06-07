// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;

namespace Dock.Serializer.UnitTests
{
    public class ListContractResolverTests
    {
        [Fact]
        public void ListContractResolver_Ctor()
        {
            var actual = new ListContractResolver();
            Assert.NotNull(actual);
        }
    }
}
