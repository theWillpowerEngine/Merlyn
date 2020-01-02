using ScintillaNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShIDE
{
    internal class ScintillaDocumentWrapper
    {
        internal Document Document;
        internal string LastSavedText = "";
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
            string actualName = name;

            var i = 1;
            while (Documents.ContainsKey(actualName))
                actualName = name + " " + i++;

            if (ActiveDocument != null)
                Documents[ActiveDocument].Ref();

            Editor.Document = Document.Empty;
            Editor.Text = content;

            var wrapper = new ScintillaDocumentWrapper(Editor.Document) {
                FileName = fileName,
                LastSavedText = ""
            };
            Documents.Add(actualName, wrapper);

            ActiveDocument = actualName;
            return actualName;
        }

        internal static void Switch(string name)
        {
            if (!Documents.ContainsKey(name))
                throw new ApplicationException("Can't switch to document '" + name + "', I don't have it in the DocumentManager.");

            if (name == ActiveDocument)
                return;

            if (ActiveDocument != null)
                Documents[ActiveDocument].Ref();

            Editor.Document = Documents[name].Document;
            Documents[name].UnRef();
            ActiveDocument = name;
        }

        internal static string Rename(string oldName, string newName, string fileName)
        {
            if (!Documents.ContainsKey(oldName))
                throw new ApplicationException("Can't rename document '" + oldName + "', I don't have it in the DocumentManager.");

            string actualName = newName;

            var i = 1;
            while (Documents.ContainsKey(newName))
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
            if (ActiveDocument != name)
                Documents[name].UnRef();
            else
                ActiveDocument = null;

            Documents.Remove(name);
        }

        internal static bool HasDocument(string name)
        {
            return Documents.ContainsKey(name);
        }
        internal static bool HasFileName(string name)
        {
            return Documents.ContainsKey(name) && !string.IsNullOrEmpty(Documents[name].FileName);
        }
        internal static string GetFileName(string name)
        {
            return Documents[name].FileName;
        }
    }
}
