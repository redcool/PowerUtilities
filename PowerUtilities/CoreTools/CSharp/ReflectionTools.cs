using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PowerUtilities
{
    public static class ReflectionTools
    {
        /// <summary>
        /// flags : private instance
        /// </summary>
        public const BindingFlags instanceBindings = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;
        public const BindingFlags callBindings = instanceBindings | BindingFlags.InvokeMethod | BindingFlags.GetField | BindingFlags.GetProperty;


        /// <summary>
        /// type's fields has attribute
        /// 
        /// {type : [methodInfos]}
        /// </summary>
        static Dictionary<Type, MethodInfo[]> typeHasAttributeMethodsDict = new ();
        static Dictionary<Type, FieldInfo[]> typeHasAttributeFieldsDict = new ();

        /// <summary>
        /// types has attributeType
        /// 
        /// {attrType : [types]}
        /// </summary>
        static Dictionary<Type, Type[]> attributeTypesDict = new Dictionary<Type, Type[]>();

        /// <summary>
        /// field string split fields
        /// </summary>
        static Dictionary<string, string[]> fieldPathDict = new Dictionary<string, string[]>();

        static ReflectionTools()
        {
            typeHasAttributeMethodsDict.Clear();
            attributeTypesDict.Clear();
            typeHasAttributeFieldsDict.Clear();
        }

        /// <summary>
        /// Get method when methodInfo is null
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodInfo"></param>
        /// <param name="methodName"></param>
        /// <param name="flags"></param>
        public static void GetMethod(this Type type, ref MethodInfo methodInfo, string methodName, BindingFlags flags = ReflectionTools.callBindings)
        {
            if (methodInfo == null)
                methodInfo = type.GetMethod(methodName, flags);
        }
        public static void GetMethod(this Type type, ref MethodInfo methodInfo, string methodName, BindingFlags flags,Binder binder, Type[] argTypes, ParameterModifier[] mods)
        {
            if (methodInfo == null)
                methodInfo = type.GetMethod(methodName, flags, binder, argTypes, mods);
        }
        /// <summary>
        /// Get type's fields have attribute<T> 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static MethodInfo[] GetMethodsHasCustomAttribute<T>(Type type) where T : Attribute
        {
            if (!typeHasAttributeMethodsDict.TryGetValue(type, out var methods))
            {
                methods = type.GetMethods(callBindings);
            }

            var methodArr = methods.Where(m => m.GetCustomAttribute<T>() != null).ToArray();
            typeHasAttributeMethodsDict[type] = methodArr;
            return methodArr;
        }
        /// <summary>
        /// Get type's fields have attribute<T>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FieldInfo[] GetFieldsHasCustomAttribute<T>(Type type) where T : Attribute
        {
            if (!typeHasAttributeFieldsDict.TryGetValue(type, out var fields))
            {
                fields = type.GetFields(callBindings);
            }

            var fieldArr = fields.Where(m => m.GetCustomAttribute<T>() != null).ToArray();
            typeHasAttributeFieldsDict[type] = fieldArr;
            return fieldArr;
        }
        /// <summary>
        /// Get types has attribute<T> in T's assembly 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Type[] GetTypesHasAttribute<T>(bool isInAppDomain = false) where T : Attribute
        {
            var attrType = typeof(T);
            Type[] typeArr = null;
            if (!attributeTypesDict.TryGetValue(attrType, out var types) || types.Length == 0)
            {
                if (isInAppDomain)
                {
                    typeArr = GetAppDomainTypes<Type>(t => t.GetCustomAttribute<T>() != null)
                        .ToArray();
                }
                else
                    typeArr = typeof(T).Assembly.GetTypes()
                        .Where(t => t.GetCustomAttribute<T>() != null)
                        .ToArray();
            }

            attributeTypesDict[attrType] = typeArr;
            return typeArr;
        }

        /// <summary>
        /// Get methodInfo[] has attribute T from appDomain assemblies
        /// 
        /// first time Slow
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static MethodInfo[] GetMethodsHasAttribute<T>(params string[] dllStartWithNames) where T : Attribute
        {
            var attrType = typeof(T);
            if (!typeHasAttributeMethodsDict.TryGetValue(attrType, out var methods))
            {
                methods = AppDomain.CurrentDomain.GetAssemblies()
                .Where(dll => IsContains(dllStartWithNames,dll.FullName))
                .SelectMany(dll => dll.GetTypes())
                .SelectMany(t => GetMethodsHasCustomAttribute<T>(t))
                .Where(ms => ms != null)
                .ToArray()
                ;
            }

            return typeHasAttributeMethodsDict[attrType] = methods;
        }
        static bool IsContains(string[] dllStartWithNames,string dllFullname)
        {
            if (dllStartWithNames.Length == 0)
                return true;

            foreach (var name in dllStartWithNames)
            {
                if (dllFullname.StartsWith(name))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Get FieldInfos has attribute T from appDomain assemblies
        /// First time Slow
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dllStartWithNames"></param>
        /// <returns></returns>
        public static FieldInfo[] GetFieldsHasAttribute<T>(params string[] dllStartWithNames) where T : Attribute
        {
            var attrType = typeof(T);
            if (!typeHasAttributeFieldsDict.TryGetValue(attrType, out var fields))
            {
                fields = AppDomain.CurrentDomain.GetAssemblies()
                .Where(dll => IsContains(dllStartWithNames, dll.FullName))
                .SelectMany(dll => dll.GetTypes())
                .SelectMany(t => GetFieldsHasCustomAttribute<T>(t))
                .Where(ms => ms != null)
                .ToArray()
                ;
            }

            return typeHasAttributeFieldsDict[attrType] = fields;
        }

        /// <summary>
        /// Search Types from currentDomain assemblies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetAppDomainTypes<T>(Func<Type, bool> predicate)
        {
            if (predicate == null)
                return Enumerable.Empty<Type>();

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes().Where(predicate))
            ;
        }

        public static Type GetAppDomainType(string typeFullName)
        {
            return GetAppDomainTypes<Type>(type => type.FullName == typeFullName).FirstOrDefault();
        }

        /// <summary>
        /// Get all types passed by predication from current Assembly or currentDomain assemblies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predication"></param>
        /// <param name="isIncludeDomain"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypes<T>(Func<Type, bool> predication, bool isIncludeDomain = false)
        {
            if (predication == null)
                return Enumerable.Empty<Type>();

            if (isIncludeDomain)
                return GetAppDomainTypes<T>(predication);
            else
                return Assembly.GetCallingAssembly().GetTypes().Where(predication);
        }
        /// <summary>
        /// Get all types derived from T from current Assembly or currentDomain assemblies
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isIncludeDomain"> find in currentDomain assemblies</param>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypesDerivedFrom<T>(bool isIncludeDomain = false)
        {
            var tType = typeof(T);
            return GetTypes<T>(t => t.BaseType != null && t.BaseType == tType, isIncludeDomain);
        }
        /// <summary>
        /// Is type implements interfaceType?
        /// </summary>
        /// <param name="type"></param>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public static bool IsImplementOf(this Type type, Type interfaceType)
        {
            return type.GetInterfaces()
                .Any(obj => (obj.IsGenericType && obj.GetGenericTypeDefinition() == interfaceType)
                    || interfaceType.IsAssignableFrom(type)
                )
                ;
        }
        /// <summary>
        /// Is type is List<>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsList(this Type type)
        {
            return IsImplementOf(type, typeof(IList<>));
            //return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }
        /// <summary>
        /// Can convert to targetType?
        /// </summary>
        /// <param name="type"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool IsAssignableTo(this Type type,Type targetType)
        {
            return targetType.IsAssignableFrom(type);
        }

        /// <summary>
        /// Get a private field
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static T GetFieldValue<T>(this Type type, object instance, string fieldName, BindingFlags flags = instanceBindings)
        {
            var obj = GetFieldValue(type, instance, fieldName, flags);
            return obj != null ? (T)obj : default;
        }
        public static object GetFieldValue(this Type type, object instance, string fieldName, BindingFlags flags = instanceBindings)
        {
            var field = type.GetField(fieldName, flags);
            return field != null ? field.GetValue(instance) : default;
        }

        /// <summary>
        /// Get field values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <param name="caller">static set null,noStatic field need a instance</param>
        /// <param name="flags"></param>
        /// <param name="nameMatch"></param>
        /// <param name="nameMatchMode"></param>
        /// <returns></returns>
        public static List<T> GetFieldValues<T>(Type t, object caller = null, BindingFlags flags = instanceBindings, string nameMatch = "", StringEx.NameMatchMode nameMatchMode = StringEx.NameMatchMode.StartWith)
        {
            var list = new List<T>();

            var fields = t.GetFields(flags);
            foreach (var field in fields)
            {
                if (!field.Name.IsMatch(nameMatch, nameMatchMode))
                    continue;

                var value = field.GetValue(caller);
                list.Add((T)value);
            }
            return list;
        }

        public static void SetFieldValue(this Type type, object instance, string fieldName, object value, BindingFlags flags = instanceBindings)
        {
            var field = type.GetField(fieldName, flags);
            if (field != null)
                field.SetValue(instance, value);
            else
            {
                // find base upwards
                if (type.BaseType != null)
                    SetFieldValue(type.BaseType, instance, fieldName, value, flags);
            }
        }

        /// <summary>
        /// Get a private Property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="caller"></param>
        /// <param name="propertyName"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static T GetPropertyValue<T>(this Type type, object caller, string propertyName, BindingFlags flags = instanceBindings)
        {
            var obj = GetPropertyValue(type,caller, propertyName, flags);
            return obj != null ? (T)obj : default;
        }

        public static object GetPropertyValue(this Type type,object caller,string propertyName,BindingFlags flags = instanceBindings)
        {
            var prop = type.GetProperty(propertyName, flags);
            return prop != null ? prop.GetValue(caller) : default;
        }

        /// <summary>
        /// Get hierarchy object value use Reflection API(slow)
        /// fieldExpress : like ( object1.object2.object3.name )
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="fieldExpress"></param>
        /// <returns></returns>
        public static object GetObjectHierarchy(this object instance, string fieldExpress)
        {
            if (instance == null || string.IsNullOrEmpty(fieldExpress))
                return null;

            var fieldNames = DictionaryTools.Get(fieldPathDict, fieldExpress, fieldExpress => fieldExpress.SplitBy('.'));
            //var fieldNames = fieldExpress.SplitBy('.');

            Type instType = instance.GetType();
            FieldInfo field = null;

            foreach (var fieldName in fieldNames)
            {
                field = instType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field == null)
                {
                    throw new ArgumentException($"{instance} dont have path: {fieldExpress}");
                }

                instance = field.GetValue(instance);
                // current object is null
                if (instance == null)
                    return null;
                // next field
                instType = instance.GetType();
            }
            return instance;
        }

        public static FieldInfo GetFieldInfoHierarchy(this object instance, string fieldExpress,out object fieldInst)
        {
            fieldInst = null;

            var fieldNames = fieldExpress.SplitBy('.');
            Type instType = instance.GetType();

            FieldInfo field = null;

            foreach (var fieldName in fieldNames)
            {
                // keep last
                fieldInst  = instance;

                field = instType.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (field == null)
                {
                    throw new ArgumentException($"{instance} dont have path: {fieldExpress}");
                }
                instance = field.GetValue(instance);
                // current object is null
                if (instance == null)
                    return null;
                // next field
                instType = instance.GetType();
            }
            return field;
        }

        /// <summary>
        /// chain call method,field,property.
        /// 
        /// a.Test().Length();
        /// 
        /// var a = new A();
        /// var lastResult = InvokeMemberChain(typeof(A), a, new[] { "Test", "Length" }, new List<object[]> { new object[] { "123" }, null });
        /// WriteLine(lastResult);
        /// </summary>
        /// <param name="fistCallerType"></param>
        /// <param name="firstCaller"></param>
        /// <param name="memberNames"></param>
        /// <param name="args"></param>
        /// <returns></returns>

        public static object InvokeMemberChain(this Type fistCallerType, object firstCaller, string[] memberNames, List<object[]> args)
        {

            var caller = firstCaller;
            var callerType = fistCallerType;
            for (int i = 0; i < memberNames.Length; i++)
            {
                var member = memberNames[i];
                var param = args[i];

                // next method
                caller = callerType?.InvokeMember(member, callBindings, null, caller, param);
                callerType = caller?.GetType();
            }
            return caller;
        }

        public static object InvokeMember(this Type type, string name, object caller, object[] args)
        {
            return type.InvokeMember(name, callBindings, null, caller, args);
        }

        public static object InvokeMethod(this Type type, string name, Type[] argTypes, object caller, object[] args)
        {
            argTypes = argTypes ?? Type.EmptyTypes;
            args = args ?? new object[] { };
            if (argTypes == Type.EmptyTypes)
            {
                return type.GetMethod(name).Invoke(caller, args);
            }

            return type.GetMethod(name, instanceBindings, null, argTypes, null)?.Invoke(caller, args);
        }

        public static object GetPropertyValue(this Type type, string name,object caller, object[] args)
        {
            return type.GetProperty(name).GetValue(caller, args);
        }

        /// <summary>
        /// Get FirstMemberInfo(PropertyInfo or FieldInfo)'s value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <param name="memberName"></param>
        /// <param name="caller"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object GetMemberValue(this Type type,string memberName,object caller, params object[] args)
        {
            var m = type.GetMember(memberName, instanceBindings).FirstOrDefault();
            if(m == null)
                return default;

            if (args == null || args.Length == 0)
                args = Type.EmptyTypes;

            if(m is PropertyInfo propertyInfo)
                return propertyInfo.GetValue(caller,args);
            if (m is FieldInfo fieldInfo)
                return fieldInfo.GetValue(caller);
            if(m is MethodInfo methodInfo)
                return methodInfo.Invoke(caller,args);

            return default;
        }

        public static T GetMemberValue<T>(this Type type, string memberName, object caller,params object[] args)
        {
            return (T)GetMemberValue(type, memberName, caller, args);
        }
        /// <summary>
        /// call caller.member with args
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="caller"></param>
        /// <param name="memberName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static T GetMemberValue<T>(this object caller,string memberName, object[] args)
        {
            var type = caller.GetType();
            return GetMemberValue<T>(type, memberName, caller, args);
        }

        public static void SetMemberValue(this Type type,string memberName,object caller, object[] args)
        {
            var m = type.GetMember(memberName, instanceBindings).FirstOrDefault();
            if (m == null)
                return;

            if (m is PropertyInfo propertyInfo)
                propertyInfo.SetValue(caller, args);
            if (m is FieldInfo fieldInfo)
                fieldInfo.SetValue(caller,args);
            
        }

        /// <summary>
        /// Copy readTarget's fieldInfos to writeTarget same named fieldInfos
        /// </summary>
        /// <param name="readTarget">read field from this</param>
        /// <param name="writeTarget">write field to this</param>
        /// <param name="bindingFlags">filed binding flags</param>
        /// <param name="onSetValue"> onSetValue: (readValue, writeValue), result default result is readValue </param>
        public static void CopyFieldInfoValues(object readTarget, object writeTarget, BindingFlags bindingFlags = instanceBindings, Func<object, object, object> onSetValue = null)
        {
            var readType = readTarget.GetType();
            var writeType = writeTarget.GetType();
            var writeTargetFields = writeType.GetFields(bindingFlags);

            for (int i = 0; i < writeTargetFields.Length; i++)
            {
                var writeField = writeTargetFields[i];
                var readField = readType.GetField(writeField.Name, bindingFlags);
                if (readField == null)
                    continue;

                var readValue = readField.GetValue(readTarget);
                if (onSetValue == null)
                {
                    writeField.SetValue(writeTarget, readValue);
                }
                else
                {
                    var writeValue = writeField.GetValue(writeTarget);

                    var result = onSetValue.Invoke(readValue, writeValue);
                    writeField.SetValue(writeTarget, result);
                }
            }
        }
    }
}
