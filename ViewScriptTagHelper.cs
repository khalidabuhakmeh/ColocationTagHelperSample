namespace ColocationTagHelperSample;

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

public class ViewScriptTagHelper : TagHelper
{
    private readonly IWebHostEnvironment environment;
    private readonly IFileVersionProvider fileVersionProvider;
    private const string AppendVersionAttributeName = "append-version";

    public ViewScriptTagHelper(IWebHostEnvironment environment, IFileVersionProvider fileVersionProvider)
    {
        this.environment = environment;
        this.fileVersionProvider = fileVersionProvider;
    }

    [ViewContext] 
    public ViewContext? ViewContext { get; set; }
    
    /// <summary>
    /// Value indicating if file version should be appended to src urls.
    /// </summary>
    /// <remarks>
    /// A query string "v" with the encoded content of the file is added.
    /// </remarks>
    [HtmlAttributeName(AppendVersionAttributeName)]
    public bool? AppendVersion { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // remove the `page-script` tag if script doesn't exist
        output.TagName = null;
        output.TagMode = TagMode.StartTagAndEndTag;

        var viewPath = ViewContext?.View.Path;
        var src = $"{viewPath}.js";
        
        /* When the app is published, the framework automatically moves the script to the web root.
           So we should check both places, with the content root first for development */
        var fileInfo = environment.ContentRootFileProvider.GetFileInfo(src) ?? 
                       environment.WebRootFileProvider.GetFileInfo(src);

        if (fileInfo is {Exists: true})
        {
            // switch it to script now
            output.TagName = "script";
            output.Content = new DefaultTagHelperContent();
            
            if (AppendVersion == true)
            {
                // people love their cache busting versions
                src = fileVersionProvider.AddFileVersionToPath(src, src);
            }
            
            output.Attributes.Add("src", src);
        }
        else
        {
            output.SuppressOutput();
        }
    }
}