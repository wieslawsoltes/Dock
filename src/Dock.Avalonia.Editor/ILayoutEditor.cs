using Dock.Model;

namespace Dock.Avalonia.Editor
{
    public interface ILayoutEditor
    {
        void AddLayout(IDock dock);
        void AddRoot(IDock dock);
        void AddSplitter(IDock dock);
        void AddDocument(IDock dock);
        void AddTool(IDock dock);
        void AddView(IDock dock);
        void AddToolTab(IDock dock);
        void AddDocumentTab(IDock dock);
        void InsertLayoutBefore(IDock dock);
        void InsertRootBefore(IDock dock);
        void InsertSplitterBefore(IDock dock);
        void InsertDocumentBefore(IDock dock);
        void InsertToolBefore(IDock dock);
        void InsertViewBefore(IDock dock);
        void InsertToolTabBefore(IDock dock);
        void InsertDocumentTabBefore(IDock dock);
        void InsertLayoutAfter(IDock dock);
        void InsertRootAfter(IDock dock);
        void InsertSplitterAfter(IDock dock);
        void InsertDocumentAfter(IDock dock);
        void InsertToolAfter(IDock dock);
        void InsertViewAfter(IDock dock);
        void InsertToolTabAfter(IDock dock);
        void InsertDocumentTabAfter(IDock dock);
        void ConvertToLayout(IDock dock);
        void ConvertToRoot(IDock dock);
        void ConvertToSplitter(IDock dock);
        void ConvertToDocument(IDock dock);
        void ConvertToTool(IDock dock);
        void ConvertToView(IDock dock);
        void ConvertToToolTab(IDock dock);
        void ConvertToDocumentTab(IDock dock);
    }
}
