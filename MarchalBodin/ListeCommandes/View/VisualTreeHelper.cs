using System.Collections.ObjectModel;
using System.Windows;

namespace ListeCommandes.View
{
    public static class VisualTreeHelper
    {
        public static Collection<T> GetVisualChildren<T>(DependencyObject current) where T : DependencyObject
        {
            if (current == null)
                return null;

            var children = new Collection<T>();
            GetVisualChildren(current, children);
            return children;
        }

        private static void GetVisualChildren<T>(DependencyObject current, Collection<T> children) where T : DependencyObject
        {
            if (current != null)
            {
                if (current.GetType() == typeof(T))
                    children.Add((T)current);

                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(current); i++)
                {
                    GetVisualChildren(System.Windows.Media.VisualTreeHelper.GetChild(current, i), children);
                }
            }
        }
    }
}
