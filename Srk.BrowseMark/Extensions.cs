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

namespace Srk.BrowseMark
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Threading;

    public static class Extensions
    {
        public static void BeginInvoke(this Dispatcher dispatcher, Action action)
        {
            dispatcher.BeginInvoke(action);
        }

        public static IEnumerable<T> FindChildren<T>(this DependencyObject element, bool recursive, bool subRecursive)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                if (child is T)
                {
                    yield return (T)child;

                    if (subRecursive)
                    {
                        foreach (var item in child.FindChildren<T>(recursive, subRecursive))
                        {
                            yield return item;
                        }
                    }
                }
                else
                {
                    if (recursive)
                    {
                        foreach (var item in child.FindChildren<T>(recursive, subRecursive))
                        {
                            yield return item;
                        }
                    }
                }
            }
        }
    }
}
