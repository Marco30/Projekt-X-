using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;// används för att skapa egna HTML helpers 
using System.Web.Mvc.Ajax;
using System.Web.Routing;// används för att skapa egna HTML helpers 

namespace OnlineVoting.HtmlHelpers
{
    public static class CustomHtmlHelepers
    {
        public static IHtmlString ActionLinkWithSpan(this HtmlHelper htmlHelper, string linkText, string action, string controller, object routeValues, object htmlAttributes, object htmlSpanAttributes)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var span = new TagBuilder("span") { InnerHtml = linkText };
            span.MergeAttributes(new RouteValueDictionary(htmlSpanAttributes));
            var anchor = new TagBuilder("a") { InnerHtml = span.ToString() };
            anchor.Attributes["href"] = urlHelper.Action(action, controller, routeValues);
            anchor.MergeAttributes(new RouteValueDictionary(htmlAttributes));

            return MvcHtmlString.Create(anchor.ToString());

        }


        public static IHtmlString ActionLinkWithSpan(this AjaxHelper ajaxHelper, string linkText, string action, string controller, object routeValues, object htmlAttributes, object htmlSpanAttributes, AjaxOptions ajaxOptions)
        {
            var urlHelper = new UrlHelper(ajaxHelper.ViewContext.RequestContext);
            var span = new TagBuilder("span") { InnerHtml = linkText };
            span.MergeAttributes(new RouteValueDictionary(htmlSpanAttributes));
            var anchor = new TagBuilder("a") { InnerHtml = span.ToString() };
            anchor.Attributes["href"] = urlHelper.Action(action, controller, routeValues);
            anchor.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            anchor.MergeAttributes((ajaxOptions ?? new AjaxOptions()).ToUnobtrusiveHtmlAttributes());

            return MvcHtmlString.Create(anchor.ToString());

        }

        public static IHtmlString ActionLinkWithSpanAndText(this HtmlHelper htmlHelper, string linkText, string action, string controller, object routeValues, object htmlAttributes, object htmlSpanAttributes, string PtagText, object htmlTextAttributes)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);

            var span = new TagBuilder("span") { InnerHtml = linkText };
            span.MergeAttributes(new RouteValueDictionary(htmlSpanAttributes));

            var p = new TagBuilder("p") { InnerHtml = PtagText };
            p.MergeAttributes(new RouteValueDictionary(htmlTextAttributes));

            span.InnerHtml += p;// läger till P tag i Span tagen 

            var anchor = new TagBuilder("a") { InnerHtml = span.ToString()};
            anchor.Attributes["href"] = urlHelper.Action(action, controller, routeValues);
            anchor.MergeAttributes(new RouteValueDictionary(htmlAttributes));


            return MvcHtmlString.Create(anchor.ToString());

        }

        public static IHtmlString ActionLinkWithSpanAndText(this AjaxHelper ajaxHelper, string linkText, string action, string controller, object routeValues, object htmlAttributes, object htmlSpanAttributes, string PtagText, object htmlTextAttributes, AjaxOptions ajaxOptions)
        {
            var urlHelper = new UrlHelper(ajaxHelper.ViewContext.RequestContext);
            var span = new TagBuilder("span") { InnerHtml = linkText };
            span.MergeAttributes(new RouteValueDictionary(htmlSpanAttributes));

            var p = new TagBuilder("p") { InnerHtml = PtagText };
            p.MergeAttributes(new RouteValueDictionary(htmlTextAttributes));

            span.InnerHtml += p;// läger till P tag i Span tagen 

            var anchor = new TagBuilder("a") { InnerHtml = span.ToString() };
            anchor.Attributes["href"] = urlHelper.Action(action, controller, routeValues);
            anchor.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            anchor.MergeAttributes((ajaxOptions ?? new AjaxOptions()).ToUnobtrusiveHtmlAttributes());

            return MvcHtmlString.Create(anchor.ToString());

        }


    }
}