using RcdCmn;
using System;
using System.Reflection;

namespace CommWrapper
{
    public class OrderAttribute : Attribute
    {
        int m_order;

        public OrderAttribute(int order)
        {
            m_order = order;
        }

        public static int GetOrder(PropertyInfo prop)
        {
            object[] attrs = prop.GetCustomAttributes(true);
            if (Attribute.IsDefined(prop, typeof(OrderAttribute)))
            {
                return prop.GetCustomAttribute<OrderAttribute>().m_order;
            } else
            {
                throw new UserException("Attribute Not Found");
            }
        }
    }
}