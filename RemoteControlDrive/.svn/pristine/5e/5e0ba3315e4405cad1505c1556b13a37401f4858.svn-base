using RcdCmn;
using System;
using System.Reflection;

namespace CommWrapper
{
    public class LengthAttribute : Attribute
    {
        int m_length;

        public LengthAttribute(int length)
        {
            m_length = length;
        }
 
        public static int GetLength(PropertyInfo prop)
        {
            object[] attrs = prop.GetCustomAttributes(true);
            if (Attribute.IsDefined(prop, typeof(LengthAttribute)))
            {
                return prop.GetCustomAttribute<LengthAttribute>().m_length;
            }
            else
            {
                throw new UserException("Attribute Not Found");
            }
        }

    }
}