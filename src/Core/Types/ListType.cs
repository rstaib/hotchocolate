using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HotChocolate.Language;

namespace HotChocolate.Types
{
    public class ListType
        : IOutputType
        , IInputType
        , INullableType
    {
        private readonly bool _isInputType;
        private readonly IInputType _inputType;

        public ListType(IType elementType)
        {
            if (elementType == null)
            {
                throw new ArgumentNullException(nameof(elementType));
            }

            if (elementType.IsListType())
            {
                throw new ArgumentException(
                    "It is not possible to put a list type into list type.",
                    nameof(elementType));
            }

            _isInputType = elementType.InnerType().IsInputType();
            _inputType = elementType.InnerType() as IInputType;
            ElementType = elementType;
            NativeType = _isInputType ? CreateListType(_inputType.NativeType) : null;
        }

        public IType ElementType { get; }

        public Type NativeType { get; }

        public bool IsInstanceOfType(IValueNode literal)
        {
            if (literal == null)
            {
                throw new ArgumentNullException(nameof(literal));
            }

            if (_isInputType)
            {
                if (literal is NullValueNode)
                {
                    return true;
                }

                if (literal is ListValueNode listValueLiteral)
                {
                    if (listValueLiteral.Items.Any())
                    {
                        return true;
                    }
                    else
                    {
                        IValueNode value = listValueLiteral.Items.First();
                        if (!_inputType.IsInstanceOfType(value))
                        {
                            return !ElementType.IsNonNullType()
                                && value is NullValueNode;
                        }
                        return true;
                    }
                }
                return false;
            }

            throw new InvalidOperationException(
                "The specified type is not an input type.");
        }

        public object ParseLiteral(IValueNode literal)
        {
            if (literal == null)
            {
                throw new ArgumentNullException(nameof(literal));
            }

            if (literal is NullValueNode)
            {
                return null;
            }

            if (_isInputType && literal is ListValueNode listValueLiteral)
            {
                return CreateArray(literal, listValueLiteral);
            }

            throw new InvalidOperationException(
                "The specified type is not an input type.");
        }

        private object CreateArray(IValueNode literal,
            ListValueNode listValueLiteral)
        {
            Type elementType = _inputType.NativeType;
            Array array = Array.CreateInstance(
                elementType,
                listValueLiteral.Items.Count);

            for (int i = 0; i < listValueLiteral.Items.Count; i++)
            {
                object element = _inputType.ParseLiteral(literal);
                array.SetValue(element, i);
            }

            return array;
        }

        public IValueNode ParseValue(object value)
        {
            if (_isInputType)
            {
                if (value == null)
                {
                    return new NullValueNode(null);
                }

                if (value is IEnumerable e)
                {
                    List<IValueNode> items = new List<IValueNode>();
                    foreach (object v in e)
                    {
                        items.Add(_inputType.ParseValue(value));
                    }
                    return new ListValueNode(null, items);
                }
            }

            throw new InvalidOperationException(
                "The specified type is not an input type.");
        }

        private static Type CreateListType(Type elementType)
        {
            return Array.CreateInstance(elementType, 0).GetType();
        }
    }

    // this is just a marker type for the fluent code-first api.
    public sealed class ListType<T>
        : IOutputType
        , IInputType
        where T : IType
    {
        private ListType()
        {
        }

        public Type NativeType => throw new NotImplementedException();

        public bool IsInstanceOfType(IValueNode literal)
        {
            throw new NotImplementedException();
        }

        public object ParseLiteral(IValueNode literal)
        {
            throw new NotImplementedException();
        }

        public IValueNode ParseValue(object value)
        {
            throw new NotImplementedException();
        }
    }
}
