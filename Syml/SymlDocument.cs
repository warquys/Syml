using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Syml;

public class SymlDocument
{
    private static readonly Regex HeaderRegex = new Regex("(\n)?\\[([^\\]]+)\\](\\s)*(\r)?(\n)");
        
    // ReSharper disable once MemberCanBePrivate.Global
    public readonly Dictionary<string, DocumentSection> Sections = new Dictionary<string, DocumentSection>();

    public delegate void UpdateEventHandler();

    /// <summary>
    /// Invoked after the section Dictionary has been modified
    /// </summary>
    public event UpdateEventHandler UpdateEvent;

    private void InvokeUpdateEvent() {
        UpdateEvent?.Invoke();
    }

    /// <summary>
    /// Loads all sections from the string into the sections dictionary and validates all entries
    /// </summary>
    /// <param name="text">The encoded syml string</param>
    /// <exception cref="SymlException">Validation is not successful</exception>
    public void Load(string text)
    {
        if (text.Trim().Length == 0) return;
        var headerMatches = HeaderRegex.Matches(text);
        var headers = new List<SectionHeader>();
        for (var i = 0; i < headerMatches.Count; i++)
        {
            var match = headerMatches[i];
            var name = match.Groups[2].Value;
            var end = match.Index + match.Length;
            headers.Add(new SectionHeader
            {
                Name = name,
                Start = match.Index,
                End = end
            });
        }
        for (var i = 0; i < headers.Count - 1; i++)
        {
            var header = headers[i];
            var len = headers[i + 1].Start - header.End;
            var content = text.Substring(header.End, len);
            Sections[header.Name] = new DocumentSection
            {
                Name = header.Name,
                YamlContent = content.Trim(),
                DocumentIndex = header.End
            };
        }

        var lastHeader = headers.Last();
        var lastContent = text.Substring(lastHeader.End);
        Sections[lastHeader.Name] = new DocumentSection
        {
            Name = lastHeader.Name,
            YamlContent = lastContent,
            DocumentIndex = lastHeader.End
        };
        Validate();
        InvokeUpdateEvent();
    }
        
    /// <summary>
    /// Validates all entries
    /// </summary>
    /// <exception cref="SymlException">Validation is not successful</exception>
    public void Validate()
    {
        foreach (var valuePair in Sections)
        {
            valuePair.Value.Validate();
        }
    }

    /// <summary>
    /// Dumps the section dictionary to a string
    /// </summary>
    /// <returns>The encoded string</returns>
    public string Dump()
    {
        if (Sections.Count == 0) return "";
        return string.Join("\n\n", Sections.Values.Select(section => section.Serialized));
    }

    /// <summary>
    /// Gets and exports a section
    /// </summary>
    /// <param name="section">identifier of the section</param>
    /// <typeparam name="T">export type reference</typeparam>
    /// <returns>exported section</returns>
    public T Get<T>(string section) where T: IDocumentSection
    {
        return Sections[section].Export<T>();
    }
        
    /// <summary>
    /// Gets and exports a section, using the <see cref="DocumentSectionAttribute"/> to retrieve the section name
    /// </summary>
    /// <typeparam name="T">export type reference</typeparam>
    /// <returns>exported section</returns>
    public T Get<T>() where T: IDocumentSection
    {
        var sectionAttribute = (DocumentSectionAttribute) typeof(T).GetCustomAttributes(false)
            .First(x => x is DocumentSectionAttribute);
        return Sections[sectionAttribute.SectionName].Export<T>();
    }
        
    /// <summary>
    /// Gets and exports a section, using the <see cref="DocumentSectionAttribute"/> to retrieve the section name
    /// </summary>
    /// <param name="type">export type reference</param>
    /// <returns>exported section</returns>
    public object Get(Type type)
    {
        var sectionAttribute = (DocumentSectionAttribute) type.GetCustomAttributes(false)
            .First(x => x is DocumentSectionAttribute);
        return Sections[sectionAttribute.SectionName].Export(type);
    }
        
    /// <summary>
    /// Imports and sets a section
    /// </summary>
    /// <param name="section">identifier of the section</param>
    /// <param name="obj">object implementing <see cref="IDocumentSection"/></param>
    public void Set(string section, object obj)
    {
        var documentSection = new DocumentSection
        {
            DocumentIndex = -1,
            Name = section
        };
        documentSection.ImportUnsafe(obj);
        Sections[documentSection.Name] = documentSection;
        InvokeUpdateEvent();
    }
        
    /// <summary>
    /// Imports and sets a section, using the <see cref="DocumentSectionAttribute"/> to retrieve the section name
    /// </summary>
    /// <param name="obj">object implementing <see cref="IDocumentSection"/></param>
    public void Set(object obj)
    {
        var type = obj.GetType();
        var sectionAttribute = (DocumentSectionAttribute) type.GetCustomAttributes(false)
            .First(x => x is DocumentSectionAttribute);
        var documentSection = new DocumentSection
        {
            DocumentIndex = -1,
            Name = sectionAttribute.SectionName
        };
        documentSection.ImportUnsafe(obj);
        Sections[sectionAttribute.SectionName] = documentSection;
        InvokeUpdateEvent();
    }

    /// <summary>
    /// Checks the presence of a document section using the <see cref="DocumentSectionAttribute"/>
    /// </summary>
    /// <typeparam name="T">The class of the section being searched</typeparam>
    /// <returns>true if the document contains the sections, otherwise false </returns>
    public bool Has<T>() where T : IDocumentSection
    {
        var type = typeof(T);
        var sectionAttribute = (DocumentSectionAttribute) type.GetCustomAttributes(false)
            .First(x => x is DocumentSectionAttribute);
        return Sections.ContainsKey(sectionAttribute.SectionName);
    }
}

public class SectionHeader
{
    public string Name { get; set; }
    public int Start { get; set; }
    public int End { get; set; }
}

[AttributeUsage(AttributeTargets.Class)]
public class DocumentSectionAttribute : Attribute
{
    public readonly string SectionName;
    public DocumentSectionAttribute(string sectionName)
    {
        SectionName = sectionName;
    }
}
    
public interface IDocumentSection { }