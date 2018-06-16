namespace Terminal.Interactive.PoorMans
{
    public interface IMetaNode
    {
        IMetaNode Traverse(string[] path);
        object GetValue();
    }
}