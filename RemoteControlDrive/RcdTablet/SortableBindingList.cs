using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace RcdTablet
{
    internal class SortableBindingList<T> : BindingList<T>
    {
        private PropertyDescriptor _sortProperty;
        private ListSortDirection _sortDirection;
        private bool _isSorted;

        protected override PropertyDescriptor SortPropertyCore { get { return _sortProperty; } }

        protected override ListSortDirection SortDirectionCore { get { return _sortDirection; } }


        protected override bool SupportsSortingCore
        {
            get { return true; }
        }

        protected override bool IsSortedCore
        {
            get { return _isSorted; }
        }

        protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
        {
            _sortProperty = property;
            _sortDirection = direction;

            List<T> items = Items as List<T>;

            if (items != null)
            {
                PropertyComparer<T> pc =
                  new PropertyComparer<T>(property, direction);
                items.Sort(pc);
                _isSorted = true;
            }
            else
            {
                _isSorted = false;
            }

            // Let bound controls know they should refresh their views
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override void RemoveSortCore()
        {
            _isSorted = false;
        }


        public void Sort(PropertyDescriptor property, ListSortDirection direction)
        {
            _sortProperty = property;
            _sortDirection = direction;

            List<T> items = Items as List<T>;

            if (items != null)
            {
                PropertyComparer<T> pc =
                  new PropertyComparer<T>(property, direction);
                items.Sort(pc);
                _isSorted = true;
            }
            else
            {
                _isSorted = false;
            }

            // Let bound controls know they should refresh their views
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        public void AddRange(SortableBindingList<T> src)
        {
            foreach (T item in src.Items)
            {
                Add(item);
            }
        }
    }


    public class PropertyComparer<T> : IComparer<T>
    {
        readonly PropertyDescriptor prop;
        readonly ListSortDirection direction;

        public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
        {
            prop = property;
            this.direction = direction;
        }

        public int Compare(T x, T y)
        {
            int compareResult = 0;
            int order = direction == ListSortDirection.Ascending ? 1 : -1;

            if (prop.PropertyType == typeof(int))
            {
                int xVal = (int)prop.GetValue(x);
                int yVal = (int)prop.GetValue(y);
                compareResult = xVal >= yVal ? 1 : -1;
            }
            else if (prop.PropertyType == typeof(int?))
            {
                int? xVal = (int?)prop.GetValue(x);
                int? yVal = (int?)prop.GetValue(y);

                if (xVal == null && yVal != null) compareResult = 1;
                else if (xVal != null && yVal == null) compareResult = -1;
                else if (xVal == null && yVal == null) compareResult = 0;
                else compareResult = xVal >= yVal ? 1 : -1;
            }
            else if (prop.PropertyType == typeof(string))
            {
                string xVal = (string)prop.GetValue(x) ?? "";
                string yVal = (string)prop.GetValue(y);
                compareResult = xVal.CompareTo(yVal);
            }
            else
            {
                compareResult = 0;
            }

            return order * compareResult;
        }

    }
}
