
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
