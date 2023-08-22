using RcdCmn;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static RcdDao.Attributes.ComboInputAttribute;
using static RcdDao.DaoCommon;

namespace RcdDao.Attributes
{
    public class SubSelectionAttribute : Attribute
    {
        public Dictionary<string, List<ComboSelect>> Options { get; private set; }

        public string SurName { get; private set; }

        public string SurMemberName { get; private set; }

        public string DisplayMemberName { get; private set; }

        public string ValueMemberName { get; private set; }

        public string MethodName { get; private set; }

        private Type DaoType;

        private object[] MethodArgs;

        public string SurMemberName2 { get; private set; }

        public SubSelectionAttribute(Type dao, string methodName, string surName, string surMember, string displayMember, string valueMember, object[] methodArgs = null)
        {
            SurName = surName;
            SurMemberName = surMember;
            DisplayMemberName = displayMember;
            ValueMemberName = valueMember;
            DaoType = dao;
            MethodName = methodName;
            MethodArgs = methodArgs == null ? new object[] { } : methodArgs;
            Options = GetOptions();
        }

        private Dictionary<string, List<ComboSelect>> GetOptions()
        {
            object dao = Activator.CreateInstance(DaoType);
            object obtainedList = DaoType.GetMethod(MethodName).Invoke(dao, MethodArgs);

            if (!IsList(obtainedList))
            {
                throw new UserException("選択リスト取得失敗");
            }

            Dictionary<string, List<ComboSelect>> result = new Dictionary<string, List<ComboSelect>>();

            IList tempList = (IList)obtainedList;
            if (tempList.Count <= 0)
            {
                return result;
            }

            var firstItem = tempList[0];
            bool hasSurMember = firstItem.GetType().GetProperty(SurMemberName) != null;
            bool hasDisplayMember = firstItem.GetType().GetProperty(DisplayMemberName) != null;
            bool hasValueMember = firstItem.GetType().GetProperty(ValueMemberName) != null;
            if (!hasSurMember || !hasDisplayMember || !hasValueMember)
            {
                throw new UserException("選択リスト取得失敗");
            }


            foreach (var item in tempList)
            {
                string surMember = item.GetType().GetProperty(SurMemberName).GetValue(item).ToString();
                string displayMember = item.GetType().GetProperty(DisplayMemberName).GetValue(item).ToString();
                string valueMember = item.GetType().GetProperty(ValueMemberName).GetValue(item).ToString();

                if (!result.ContainsKey(surMember))
                {
                    result[surMember] = new List<ComboSelect>();
                    result[surMember].Add(new ComboSelect("", ""));
                }

                ComboSelect cmb = new ComboSelect(displayMember, valueMember);
                result[surMember].Add(cmb);
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

    }
}