using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using FluentValidation.Internal;

namespace NetMailGun.Utils;

public static class CamelCasePropertyNameResolver
{
    private static readonly JsonNamingPolicy NamingPolicy = JsonNamingPolicy.CamelCase;

    public static string ResolvePropertyName(Type _, MemberInfo? memberInfo, LambdaExpression? expression)
    {
        if (expression != null)
        {
            var chain = PropertyChain.FromExpression(expression);
            
            if (chain.Count > 0)
            {
                var pathBuilder = new StringBuilder();
                var chainSpan = chain.ToString().AsSpan();
                foreach (var range in chain.ToString().AsSpan().Split('.'))
                {
                    var segment = chainSpan[range];
                    if (pathBuilder.Length > 0) pathBuilder.Append('.');
                    pathBuilder.Append(NamingPolicy.ConvertName(segment.ToString()));
                }

                return pathBuilder.ToString();
            }
        }

        return memberInfo != null ? NamingPolicy.ConvertName(memberInfo.Name) : string.Empty;
    }
}