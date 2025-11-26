using NetMailGun.Core.Services;

namespace NetMailGun.Infrastructure.Templates;

public class MustacheTemplateService : ITemplateRendererService
{
    public ICompiledTemplate<T> Compile<T>(string template)
    {
        var renderer = new Stubble.Compilation.StubbleCompilationRenderer();
        return new MustacheCompiledTemplate<T>(renderer.Compile<T>(template));
    }

    public string Render(string template, Dictionary<string, object?> view) =>
        Stubble.Core.StaticStubbleRenderer.Render(template, view);

    private class MustacheCompiledTemplate<T>(Func<T, string> template) : ICompiledTemplate<T>
    {
        public string Render(T data) => template(data);
    }
}