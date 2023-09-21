using Microsoft.OpenApi.Models;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;

namespace FunBot.Extensions
{
    public static class AddSwaggerGenExtensions
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            return services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "DiscrodBot",
                    Version = "v1",
                    Description = "Версия пакета DiscrodBot.Shared 1.0.0"
                });

                c.UseAllOfToExtendReferenceSchemas();

                c.CustomSchemaIds(x => x.Name);

                //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlCommentsWithRemarks(xmlPath, true);

                //const string xmlFileShared = "Stemy.DiscrodBot.Shared.xml";
                //var xmlPathShared = Path.Combine(AppContext.BaseDirectory, xmlFileShared);
                //c.IncludeXmlCommentsWithRemarks(xmlPathShared, true);

                c.AddEnumsWithValuesFixFilters(o =>
                {
                    // add schema filter to fix enums (add 'x-enumNames' for NSwag or its alias from XEnumNamesAlias) in schema
                    o.ApplySchemaFilter = true;

                    // alias for replacing 'x-enumNames' in swagger document
                    o.XEnumNamesAlias = "x-enum-varnames";

                    // alias for replacing 'x-enumDescriptions' in swagger document
                    o.XEnumDescriptionsAlias = "x-enum-descriptions";

                    // add parameter filter to fix enums (add 'x-enumNames' for NSwag or its alias from XEnumNamesAlias) in schema parameters
                    o.ApplyParameterFilter = true;

                    // add document filter to fix enums displaying in swagger document
                    o.ApplyDocumentFilter = true;

                    // add descriptions from DescriptionAttribute or xml-comments to fix enums (add 'x-enumDescriptions' or its alias from XEnumDescriptionsAlias for schema extensions) for applied filters
                    o.IncludeDescriptions = true;

                    // add remarks for descriptions from xml-comments
                    o.IncludeXEnumRemarks = true;

                    // get descriptions from DescriptionAttribute then from xml-comments
                    o.DescriptionSource = DescriptionSources.DescriptionAttributesThenXmlComments;

                    // new line for enum values descriptions
                    // o.NewLine = Environment.NewLine;
                    o.NewLine = "\n";

                    // get descriptions from xml-file comments on the specified path
                    // should use "options.IncludeXmlComments(xmlFilePath);" before
                    //o.IncludeXmlCommentsFrom(xmlPath);
                    //o.IncludeXmlCommentsFrom(xmlPathShared);
                });
            });
        }
    }
}