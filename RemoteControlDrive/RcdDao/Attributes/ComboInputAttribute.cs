using RcdCmn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static RcdDao.DaoCommon;

namespace RcdDao.Attributes
{
    public class ComboInputAttribute : Attribute
    {
        public List<ComboSelect> Items { get; private set; }

        public string ListMethodName { get; private set; }

        public string DisplayMemberName { get; private set; }

        public string ValueMemberName { get; private set; }

        private Type DaoType;

        private object[] MethodArgs;

        public ComboInputAttribute(Type dao, string methodName, string displayMember, string valueMember, object[] methodArgs = null)
        {
            DisplayMemberName = displayMember;
            ValueMemberName = valueMember;
            DaoType = dao;
            ListMethodName = methodName;
            MethodArgs = methodArgs == null ? new object[]{ } : methodArgs;
            Items = GetSelectionList();
        }

        private List<ComboSelect> GetSelectionList()
        {
            object dao = Activator.CreateInstance(DaoType);
            object obtainedList = DaoType.GetMethod(ListMethodName).Invoke(dao, MethodArgs);

            if (!IsList(obtainedList))
            {
                throw new UserException("選択リスト取得失敗");
            }

            List<ComboSelect> result = new List<ComboSelect>();
            result.Add(new ComboSelect("", ""));

            IList tempList = (IList)obtainedList;
            if (tempList.Count <= 0)
            {
                return result;
            }

            var firstItem = tempList[0];
            bool hasDisplayMember = firstItem.GetType().GetProperty(DisplayMemberName) != null;
            bool hasValueMember = firstItem.GetType().GetProperty(ValueMemberName) != null;
            if (!hasDisplayMember || !hasValueMember)
            {
                throw new UserException("選択リスト取得失敗");
            }

            
            foreach (var item in tempList)
            {
                string displayMember = item.GetType().GetProperty(DisplayMemberName).GetValue(item).ToString();
                string valueMember = item.GetType().GetProperty(ValueMemberName).GetValue(item).ToString();
                ComboSelect cmb = new ComboSelect(displayMember, valueMember);
                result.Add(cmb);
            }

            return result;
        }

        private bool IsList(object o)
        {
            if (o == null) return false;
            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public class ComboSelect : Result
        {
            public string DisplayMember { get; set; }

            public string ValueMember { get; set; }

            public int? IntValueMember { get; set; }

            public ComboSelect(string _displayMember, string _valueMember)
            {
                DisplayMember = _displayMember;
                ValueMember = _valueMember;
            }

            public ComboSelect(string _displayMember, int _valueMember)
            {
                DisplayMember = _displayMember;
                IntValueMember = _valueMember;
            }

            public ComboSelect() { }
        }
    }
}