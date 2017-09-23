/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// Abstraction for window rendering types
        /// </summary>
        public interface IWindow
        {
            String Title();
            void Render(Int32 id);
            void SetEditedObject(Object value, Action<Object> callback);
        }
    }
}