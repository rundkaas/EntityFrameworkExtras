﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace EntityFrameworkExtras
{
    internal class StoredProcedureParserHelper
    {
        public string GetParameterName(PropertyInfo propertyInfo)
        {
            return propertyInfo.Name;
        }

        public string GetUserDefinedTableType(PropertyInfo propertyInfo)
        {
            Type collectionType = GetCollectionType(propertyInfo.PropertyType);
            var attribute = Attributes.GetAttribute<UserDefinedTableTypeAttribute>(collectionType);

            if (attribute == null)
                throw new InvalidOperationException(
                    String.Format("{0} has not been decorated with UserDefinedTableTypeAttribute.",
                                  propertyInfo.PropertyType));

            return attribute.Name;
        }

        public Type GetCollectionType(Type type)
        {
            foreach (Type interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType
                    && interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    return interfaceType.GetGenericArguments()[0];
                }
            }
            return null;
        }

        public object GetUserDefinedTableValue(PropertyInfo propertyInfo, object storedProcedure)
        {
            Type enumerableType = GetCollectionType(propertyInfo.PropertyType);
            object propertyValue = propertyInfo.GetValue(storedProcedure, null);

            var generator = new UserDefinedTableGenerator(enumerableType, propertyValue);

            DataTable table = generator.GenerateTable();

            return table;
        }

        public bool IsUserDefinedTableParameter(PropertyInfo propertyInfo)
        {
            Type collectionType = GetCollectionType(propertyInfo.PropertyType);

            return collectionType != null;
        }

        public bool ParameterIsMandatory(StoredProcedureParameterOptions options)
        {
            return (options & StoredProcedureParameterOptions.Mandatory) ==
                   StoredProcedureParameterOptions.Mandatory;
        }
    }
}