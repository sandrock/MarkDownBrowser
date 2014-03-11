// MarkDownBrowser
// Copyright 2013 SandRock
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

namespace Srk.BrowseMark.LocalHttpServer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using uhttpsharp;

    public class HttpRequestHandler : uhttpsharp.HttpRequestHandler
    {
        /// <summary>
        /// Gets the specified resource as text.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        protected static string GetAssemblyResourceAsText(Assembly assembly, string path, Encoding encoding)
        {
            path = assembly.GetName().Name + "." + path;
            var stream = assembly.GetManifestResourceStream(path);
            if (stream == null)
                return null;

            try
            {
                var reader = new StreamReader(stream, encoding);
                var result = reader.ReadToEnd();
                return result;
            }
            finally
            {
                stream.Close();
            }
        }
    }
}
