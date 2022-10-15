using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using PreMailer.Net;

namespace Mark.HtmlToPdf;

public class CssInliner
{
    public static string Inline(string html, string? css = null)
    {
        using var preMailer = new PreMailer.Net.PreMailer(html);
        preMailer.MoveCssInline(removeStyleElements: true, css: css);
        return preMailer.Document.Body!.InnerHtml;
    }
}