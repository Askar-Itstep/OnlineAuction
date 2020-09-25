using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace OnlineAuction.ServiceClasses
{
    public static class DatePickerHelper
    {
        //public static MvcHtmlString DatePickerFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, 
        //                                                                Expression<Func<TModel, TProperty>> expression, object htmlAttributes)
        //{
        //    var sb = new StringBuilder();
        //    var metaData = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
        //    var dtpId = "dtp" + metaData.PropertyName;
        //    var dtp = htmlHelper.TextBoxFor(expression, htmlAttributes).ToHtmlString();
        //    sb.AppendFormat("<div class='input-group date' id='{0}'> {1} <span class='input-group-addon'><span class='glyphicon glyphicon-calendar'></span></span></div>", dtpId, dtp);
        //    return MvcHtmlString.Create(sb.ToString());
        //}
    }
}