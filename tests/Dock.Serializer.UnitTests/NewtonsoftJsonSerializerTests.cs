// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Xunit;

namespace Dock.Serializer.UnitTests
{
    public class NewtonsoftJsonSerializerTests
    {
        [Fact]
        public void NewtonsoftJsonSerializer_Ctor()
        {
            var actual = new NewtonsoftJsonSerializer();
            Assert.NotNull(actual);
        }
    }
}
