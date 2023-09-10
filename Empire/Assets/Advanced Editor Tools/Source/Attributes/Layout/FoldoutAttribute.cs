
namespace AdvancedEditorTools.Attributes
{
    public abstract class FoldoutAttribute : LayoutAttribute { }

    public class BeginFoldoutAttribute : FoldoutAttribute
    {
        public string label;
        /// <param name="label">Label that will be displayed along the foldout button</param>
        public BeginFoldoutAttribute(string label)
        {
            this.label = label;
        }
    }
    public class EndFoldoutAttribute : FoldoutAttribute
    {
        public bool includeLast;
    }
}
