﻿using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text;

namespace BlackJackAOP
{
    public sealed class CodeGenerationContext
    {
        private readonly StringBuilder _writer = new();

        public ISet<Assembly> References { get; } = new HashSet<Assembly> { 
            typeof(object).Assembly, 
            Assembly.Load(new AssemblyName("System.Runtime")),
            Assembly.Load(new AssemblyName("System.ComponentModel")),
            typeof(IServiceCollection).Assembly,
            typeof(CodeGenerationContext).Assembly
        };

        public int IndentLevel { get; private set; }

        public string SourceCode => _writer.ToString();

        public CodeGenerationContext WriteLines(params string[] lines)
        {
            if (lines.Length == 0)
            {
                _writer.AppendLine();
            }
            lines = lines.SelectMany(it => it.Split('\n')).ToArray();
            foreach (var line in lines)
            {
                _writer.AppendLine($"{new string(' ', 4 * IndentLevel)}{line}");
            }
            return this;
        }

        public IDisposable CodeBlock(string? start = null, string? end = null) => new CodeBlockScope(this, start??"{", end??"}");

        public IDisposable Indent() => new IndentScope(this);

        private class CodeBlockScope : IDisposable
        {
            private readonly CodeGenerationContext _context;
            private readonly string _end;            

            public CodeBlockScope(CodeGenerationContext context, string start, string end) 
            {
                _end = end;
                _context = context;
                _context.WriteLines(start);
                _context.IndentLevel++;
            }

            public void Dispose()
            {
                _context.IndentLevel--;
                _context.WriteLines(_end);
            }
        }

        private class IndentScope : IDisposable
        {
            private readonly CodeGenerationContext _context;

            public IndentScope(CodeGenerationContext context)
            {
                _context = context;
                _context.IndentLevel++;
            }

            public void Dispose()=> _context.IndentLevel--;
        }
    }
}
