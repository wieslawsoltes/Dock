// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Dock.Serializer
{
    public interface IDockSerializer
    {
        string Serialize<T>(T value);
        T Deserialize<T>(string text);
        T Load<T>(string path);
        void Save<T>(string path, T value);
    }
}
