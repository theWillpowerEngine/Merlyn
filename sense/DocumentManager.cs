using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shiro.Sense
{
    internal class ScintillaDocumentWrapper
    {
        internal Document Document;
        internal string LastSavedText = "";
        internal string Content = "";
        internal string FileName;

        private bool HasExtraRef = false;
        internal void Ref()
        {
            if(!HasExtraRef)
            {
                HasExtraRef = true;
                DocumentManager.Editor.AddRefDocument(Document);
            }
        }
        internal void UnRef()
        {
            if (HasExtraRef)
            {
                HasExtraRef = false;
                DocumentManager.Editor.ReleaseDocument(Document);
            }
        }

        internal ScintillaDocumentWrapper(Document d)
        {
            Document = d;
        }
    }

    internal static class DocumentManager
    {
        internal static Scintilla Editor;
        private static Dictionary<string, ScintillaDocumentWrapper> Documents = new Dictionary<string, ScintillaDocumentWrapper>();

        private static string ActiveDocument = null;

        internal static string AddDocument(string name, string fileName = null, string content = "")
        {
            string actualName = name.TrimStart('*').Trim();

            var i = 1;
            while (Documents.ContainsKey(actualName))
                actualName = name + " " + i++;

            if (ActiveDocument != null)
                Documents[ActiveDocument].Ref();

            Editor.Document = Document.Empty;
            Editor.Text = content;

            var wrapper = new ScintillaDocumentWrapper(Editor.Document) {
                FileName = fileName,
                LastSavedText = (fileName != null) ? content : "",
                Content = content
            };
            Documents.Add(actualName, wrapper);

            ActiveDocument = actualName;
            return actualName;
        }

        internal static void Switch(string name)
        {
            name = name.TrimStart('*').Trim();

            if (!Documents.ContainsKey(name))
                throw new ApplicationException("Can't switch to document '" + name + "', I don't have it in the DocumentManager.");

            if (name == ActiveDocument)
                return;

            if (ActiveDocument != null)
                Documents[ActiveDocument].Ref();

            Documents[ActiveDocument].Content = Editor.Text;

            Editor.Document = Documents[name].Document;
            Documents[name].UnRef();
            ActiveDocument = name;
        }

        internal static string Rename(string oldName, string newName, string fileName)
        {
            oldName = oldName.TrimStart('*').Trim();

            if (!Documents.ContainsKey(oldName))
                throw new ApplicationException("Can't rename document '" + oldName + "', I don't have it in the DocumentManager.");

            string actualName = newName;

            var i = 1;
            while (Documents.ContainsKey(actualName))
                actualName = newName + " " + i++;

            Documents[actualName] = Documents[oldName];
            Documents.Remove(oldName);
            Documents[actualName].FileName = fileName;

            if (ActiveDocument == oldName)
                ActiveDocument = actualName;

            return actualName;
        }

        internal static void ReleaseDocument(string name)
        {
            name = name.TrimStart('*').Trim();

            if (ActiveDocument != name)
                Documents[name].UnRef();
            else
                ActiveDocument = null;

            Documents.Remove(name);
        }

        internal static bool HasDocument(string name)
        {
            name = name.TrimStart('*').Trim();
            return Documents.ContainsKey(name);
        }
        internal static bool HasFileName(string name)
        {
            name = name.TrimStart('*').Trim();
            return Documents.ContainsKey(name) && !string.IsNullOrEmpty(Documents[name].FileName);
        }
        internal static string GetFileName(string name)
        {
            name = name.TrimStart('*').Trim();
            return Documents[name].FileName;
        }

        internal static string GetSavedContent(string name)
        {
            name = name.TrimStart('*').Trim();
            return Documents[name].LastSavedText;
        }

        internal static string GetDocumentContentCurrent(string name)
        {
            name = name.TrimStart('*').Trim();
            return Documents[name].Content;
        }

        internal static void UpdateSavedContent(string name, string text)
        {
            name = name.TrimStart('*').Trim();
            Documents[name].LastSavedText = text;
        }
        internal static void UpdateContent(string name, string text)
        {
            name = name.TrimStart('*').Trim();
            Documents[name].Content = text;
        }
    }
}
