﻿using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Syml;

public class DocumentSection 
{
    public int DocumentIndex { get; internal set; }
    public string Name { get; internal set; }
    public string YamlContent { get; internal set; }
    public string Serialized => $"[{Name}]\n{YamlContent}".Trim();
        
    public T Export<T>() where T : IDocumentSection
    {
        try
        {
            var ret = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build().Deserialize<T>(YamlContent);
            return ret;
        }
        catch (Exception e)
        {
            throw new SymlException($"Error occured while exporting the section '{Name}'[{DocumentIndex}] as {typeof(T).Name}: {e.Message}", e);
        }
    }

    public object Export(Type type)
    {
        try
        {
            var ret = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build().Deserialize(YamlContent, type);
            return ret;
        }
        catch (Exception e)
        {
            throw new SymlException($"Error occured while exporting the section '{Name}'[{DocumentIndex}] as {type.Name}: {e.Message}", e);
        }
    }
        
    public object Export()
    {
        try
        {
            var ret = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build().Deserialize<object>(YamlContent);
            return ret;
        }
        catch (Exception e)
        {
            throw new SymlException($"Error occured while exporting the section '{Name}'[{DocumentIndex}]: {e.Message}", e);
        }
    }
        
    public string Import<T>(T entity) where T : IDocumentSection
    {
        YamlContent = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
            .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
            .Build().Serialize(entity);
        return YamlContent;
    }

    public string ImportUnsafe(object obj)
    {
        YamlContent = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeInspector(inner => new CommentGatheringTypeInspector(inner))
            .WithEmissionPhaseObjectGraphVisitor(args => new CommentsObjectGraphVisitor(args.InnerVisitor))
            .Build().Serialize(obj);
        return YamlContent;
    }

    public void Validate() => Export();
}


[Serializable]
public class SymlException : Exception
{
    public SymlException() { }

    public SymlException(string message) : base(message) { }

    public SymlException(string message, Exception inner) : base(message, inner) { }

#if NET5_0_OR_GREATER
    [Obsolete]
#endif
    protected SymlException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}