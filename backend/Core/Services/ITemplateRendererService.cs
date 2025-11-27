namespace NetFormsManager.Core.Services;

public interface ITemplateRendererService
{
    public ICompiledTemplate<T> Compile<T>(string template);
    public string Render(string template, Dictionary<string, object?> view);
}

public interface ICompiledTemplate<in T>
{
    public string Render(T data);
}