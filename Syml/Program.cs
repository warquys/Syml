﻿using System;
using System.ComponentModel;
using System.IO;

namespace Syml;

public class Program
{
    public static void Main()
    {
        var text = File.ReadAllText("D:\\Projects\\Syml\\Serialized.syml");
        var document = new SymlDocument();
        document.Load(text);
        document.Set(new ContactRegion
        {
            Name = "Max Mustermann",
            Age = 18,
            Locale = ExampleLocale.GERMAN
        });
            
        document.Set(new HomeRegion
        {
            Address = "Musterstraße 12",
            City = "Munich"
        });

        Console.WriteLine(document.Get<ContactRegion>());
        Console.WriteLine(document.Get<HomeRegion>());

        File.WriteAllText("D:\\Projects\\Syml\\Serialized.syml", document.Dump());
    }
}

[DocumentSection("Contact")]
public class ContactRegion : IDocumentSection
{
    [Description("Name of the contact")]
    public string Name { get; set; }
        
    [Description("Age of the contact")]
    public int Age { get; set; }
        
    [Description("Locale of the contact")]
    public ExampleLocale Locale { get; set; }
        
    public override string ToString() => $"[Contact] Name: {Name} Age: {Age} Locale: {Locale}";
}
    
[DocumentSection("Home")]
public class HomeRegion : IDocumentSection
{
    public string Address { get; set; }
    public string City { get; set; }
        
    public override string ToString() => $"[Home] Address: {Address} City: {City}";
}


public enum ExampleLocale
{
    GERMAN,
    ENGLISH
}