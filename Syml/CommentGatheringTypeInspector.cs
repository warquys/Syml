﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.TypeInspectors;

namespace Syml;

/**
     * The code bellow is from https://dotnetfiddle.net/8M6iIE
     * Great thanks to Antoine Aubry for providing this awesome
     * code sample, showing how to emit comments in YamlDotNet
     */
public class CommentGatheringTypeInspector : TypeInspectorSkeleton
{
    private readonly ITypeInspector _innerTypeDescriptor;

    public CommentGatheringTypeInspector(ITypeInspector innerTypeDescriptor)
    {
        _innerTypeDescriptor = innerTypeDescriptor ?? throw new ArgumentNullException("innerTypeDescriptor");
    }

    public override string GetEnumName(Type enumType, string name) => _innerTypeDescriptor.GetEnumName(enumType, name);

    public override string GetEnumValue(object enumValue) => _innerTypeDescriptor.GetEnumValue(enumValue);

    public override IEnumerable<IPropertyDescriptor> GetProperties(Type type, object container)
    {
        return _innerTypeDescriptor
            .GetProperties(type, container)
            .Select(d => new CommentsPropertyDescriptor(d));
    }

    private sealed class CommentsPropertyDescriptor : IPropertyDescriptor
    {
        private readonly IPropertyDescriptor _baseDescriptor;

        public CommentsPropertyDescriptor(IPropertyDescriptor baseDescriptor)
        {
            _baseDescriptor = baseDescriptor;
            Name = baseDescriptor.Name;
            AllowNulls = baseDescriptor.AllowNulls;
            Required = baseDescriptor.Required;
            ConverterType = baseDescriptor.ConverterType;
        }

        public string Name { get; }

        public Type Type => _baseDescriptor.Type;

        public Type TypeOverride
        {
            get => _baseDescriptor.TypeOverride;
            set => _baseDescriptor.TypeOverride = value;
        }

        public int Order { get; set; }

        public ScalarStyle ScalarStyle
        {
            get => _baseDescriptor.ScalarStyle;
            set => _baseDescriptor.ScalarStyle = value;
        }

        public bool CanWrite => _baseDescriptor.CanWrite;
        public bool AllowNulls { get; }
        public bool Required { get; }
        public Type ConverterType { get; }

        public void Write(object target, object value)
        {
            _baseDescriptor.Write(target, value);
        }

        public T GetCustomAttribute<T>() where T : Attribute
        {
            return _baseDescriptor.GetCustomAttribute<T>();
        }

        public IObjectDescriptor Read(object target)
        {
            var description = _baseDescriptor.GetCustomAttribute<DescriptionAttribute>();
            return description != null
                ? new CommentsObjectDescriptor(_baseDescriptor.Read(target), description.Description)
                : _baseDescriptor.Read(target);
        }
    }
}