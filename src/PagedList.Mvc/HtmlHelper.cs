using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace PagedList.Mvc
{
	///<summary>
	///	Extension methods for generating paging controls that can operate on instances of IPagedList.
	///</summary>
	public static class HtmlHelper
	{
        public const string CSS_CLASS_PAGER = "pager";
        public const string CSS_CLASS_PAGINATION = "pagination";
        public const string CSS_CLASS_DISABLED = "disabled";
        public const string CSS_CLASS_ACTIVE = "active";
        public const string CSS_CLASS_FIRST = "first";
        public const string CSS_CLASS_PREVIOUS = "previous";
        public const string CSS_CLASS_NEXT = "next";
        public const string CSS_CLASS_LAST = "last";

        public const string TEXT_PREVIOUS = "上一页";
        public const string TEXT_NEXT = "下一页";

        private static TagBuilder WrapInListItem(string text)
		{
			var li = new TagBuilder("li");
			li.SetInnerText(text);
			return li;
		}

		private static TagBuilder WrapInListItem(TagBuilder inner, PagedListRenderOptions options, params string[] classes)
		{
			var li = new TagBuilder("li");
			foreach (var @class in classes)
				li.AddCssClass(@class);
			if (options.FunctionToTransformEachPageLink != null)
				return options.FunctionToTransformEachPageLink(li, inner);
			li.InnerHtml = inner.ToString();
			return li;
		}

		private static TagBuilder First(IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptions options)
		{
			const int targetPageNumber = 1;
			var first = new TagBuilder("a")
			            	{
			            		InnerHtml = string.Format(options.LinkToFirstPageFormat, targetPageNumber)
			            	};
			
			if (list.IsFirstPage)
				return WrapInListItem(first, options, "PagedList-skipToFirst", "disabled");

			first.Attributes["href"] = generatePageUrl(targetPageNumber);
			return WrapInListItem(first, options, "PagedList-skipToFirst");
		}

		private static TagBuilder Previous(IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptions options)
		{
			var targetPageNumber = list.PageNumber - 1;
			var previous = new TagBuilder("a")
			               	{
			               		InnerHtml = string.Format(options.LinkToPreviousPageFormat, targetPageNumber)
			               	};
			previous.Attributes["rel"] = "prev";
			
			if (!list.HasPreviousPage)
				return WrapInListItem(previous, options, "PagedList-skipToPrevious", "disabled");

			previous.Attributes["href"] = generatePageUrl(targetPageNumber);
			return WrapInListItem(previous, options, "PagedList-skipToPrevious");
		}

		private static TagBuilder Page(int i, IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptions options)
		{
			var format = options.FunctionToDisplayEachPageNumber
				?? (pageNumber => string.Format(options.LinkToIndividualPageFormat, pageNumber));
			var targetPageNumber = i;
			var page = new TagBuilder("a");
			page.SetInnerText(format(targetPageNumber));

			if (i == list.PageNumber)
				return WrapInListItem(page, options, "active");

			page.Attributes["href"] = generatePageUrl(targetPageNumber);
			return WrapInListItem(page, options);
		}

		private static TagBuilder Next(IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptions options)
		{
			var targetPageNumber = list.PageNumber + 1;
			var next = new TagBuilder("a")
			           	{
			           		InnerHtml = string.Format(options.LinkToNextPageFormat, targetPageNumber)
			           	};
			next.Attributes["rel"] = "next";
			
			if (!list.HasNextPage)
				return WrapInListItem(next, options, "PagedList-skipToNext", "disabled");

			next.Attributes["href"] = generatePageUrl(targetPageNumber);
			return WrapInListItem(next, options, "PagedList-skipToNext");
		}

		private static TagBuilder Last(IPagedList list, Func<int, string> generatePageUrl, PagedListRenderOptions options)
		{
			var targetPageNumber = list.PageCount;
			var last = new TagBuilder("a")
			           	{
			           		InnerHtml = string.Format(options.LinkToLastPageFormat, targetPageNumber)
			           	};
			
			if (list.IsLastPage)
				return WrapInListItem(last, options, "PagedList-skipToLast", "disabled");

			last.Attributes["href"] = generatePageUrl(targetPageNumber);
			return WrapInListItem(last, options, "PagedList-skipToLast");
		}

		private static TagBuilder PageCountAndLocationText(IPagedList list, PagedListRenderOptions options)
		{
			var text = new TagBuilder("a");
			text.SetInnerText(string.Format(options.PageCountAndCurrentLocationFormat, list.PageNumber, list.PageCount));

			return WrapInListItem(text, options, "PagedList-pageCountAndLocation", "disabled");
		}

		private static TagBuilder ItemSliceAndTotalText(IPagedList list, PagedListRenderOptions options)
		{
			var text = new TagBuilder("a");
			text.SetInnerText(string.Format(options.ItemSliceAndTotalFormat, list.FirstItemOnPage, list.LastItemOnPage, list.TotalItemCount));

			return WrapInListItem(text, options, "PagedList-pageCountAndLocation", "disabled");
		}

		private static TagBuilder Ellipses(PagedListRenderOptions options)
		{
			var a = new TagBuilder("a")
			        	{
			        		InnerHtml = options.EllipsesFormat
			        	};

			return WrapInListItem(a, options, "PagedList-ellipses", "disabled");
		}

		
		///<summary>
		///	Displays a configurable paging control for instances of PagedList.
		///</summary>
		///<param name = "html">This method is meant to hook off HtmlHelper as an extension method.</param>
		///<param name = "list">The PagedList to use as the data source.</param>
		///<param name = "options">Formatting options.</param>
		///<returns>Outputs the paging control HTML.</returns>
		public static MvcHtmlString PagedListPager(this System.Web.Mvc.HtmlHelper html,
												   IPagedList list,
												   PagedListRenderOptions options)
		{
            // Calculate total pages
            int pageCount = (int)Math.Ceiling(
                (double)list.TotalItemCount / list.PageNumber);

            if (options.Display == PagedListDisplayMode.Never || (options.Display == PagedListDisplayMode.IfNeeded && pageCount <= 1))
                return null;

			var listItemLinks = new List<TagBuilder>();

            //calculate start and end of range of page numbers
            var firstPageToDisplay = 1;
			var lastPageToDisplay = pageCount;
			var pageNumbersToDisplay = lastPageToDisplay;
            if (options.MaximumPageNumbersToDisplay.HasValue && pageCount > options.MaximumPageNumbersToDisplay)
            {
				// cannot fit all pages into pager
                var maxPageNumbersToDisplay = options.MaximumPageNumbersToDisplay.Value;
                firstPageToDisplay = list.PageNumber - maxPageNumbersToDisplay / 2;
                if (firstPageToDisplay < 1)
                    firstPageToDisplay = 1;
                pageNumbersToDisplay = maxPageNumbersToDisplay;
	            lastPageToDisplay = firstPageToDisplay + pageNumbersToDisplay - 1;
                if (lastPageToDisplay > list.PageCount)
                    firstPageToDisplay = list.PageCount - maxPageNumbersToDisplay + 1;
            }

			//first
			if (options.DisplayLinkToFirstPage == PagedListDisplayMode.Always || (options.DisplayLinkToFirstPage == PagedListDisplayMode.IfNeeded && firstPageToDisplay > 1))
				listItemLinks.Add(First(list, generatePageUrl, options));

			//previous
			if (options.DisplayLinkToPreviousPage == PagedListDisplayMode.Always || (options.DisplayLinkToPreviousPage == PagedListDisplayMode.IfNeeded && !list.IsFirstPage))
				listItemLinks.Add(Previous(list, generatePageUrl, options));

			//text
			if (options.DisplayPageCountAndCurrentLocation)
				listItemLinks.Add(PageCountAndLocationText(list, options));

			//text
			if (options.DisplayItemSliceAndTotal)
				listItemLinks.Add(ItemSliceAndTotalText(list, options));

			//page
			if (options.DisplayLinkToIndividualPages)
			{
				//if there are previous page numbers not displayed, show an ellipsis
				if (options.DisplayEllipsesWhenNotShowingAllPageNumbers && firstPageToDisplay > 1)
					listItemLinks.Add(Ellipses(options));

				foreach (var i in Enumerable.Range(firstPageToDisplay, pageNumbersToDisplay))
				{
					//show delimiter between page numbers
					if (i > firstPageToDisplay && !string.IsNullOrWhiteSpace(options.DelimiterBetweenPageNumbers))
						listItemLinks.Add(WrapInListItem(options.DelimiterBetweenPageNumbers));

					//show page number link
					listItemLinks.Add(Page(i, list, generatePageUrl, options));
				}

				//if there are subsequent page numbers not displayed, show an ellipsis
				if (options.DisplayEllipsesWhenNotShowingAllPageNumbers && (firstPageToDisplay + pageNumbersToDisplay - 1) < list.PageCount)
					listItemLinks.Add(Ellipses(options));
			}

			//next
			if (options.DisplayLinkToNextPage == PagedListDisplayMode.Always || (options.DisplayLinkToNextPage == PagedListDisplayMode.IfNeeded && !list.IsLastPage))
				listItemLinks.Add(Next(list, generatePageUrl, options));

			//last
            if (options.DisplayLinkToLastPage == PagedListDisplayMode.Always || (options.DisplayLinkToLastPage == PagedListDisplayMode.IfNeeded && lastPageToDisplay < list.PageCount))
				listItemLinks.Add(Last(list, generatePageUrl, options));

            if(listItemLinks.Any())
            {
                //append class to first item in list?
                if (!string.IsNullOrWhiteSpace(options.ClassToApplyToFirstListItemInPager))
                    listItemLinks.First().AddCssClass(options.ClassToApplyToFirstListItemInPager);

                //append class to last item in list?
                if (!string.IsNullOrWhiteSpace(options.ClassToApplyToLastListItemInPager))
                    listItemLinks.Last().AddCssClass(options.ClassToApplyToLastListItemInPager);

                //append classes to all list item links
                foreach (var li in listItemLinks)
                    foreach (var c in options.LiElementClasses ?? Enumerable.Empty<string>())
                        li.AddCssClass(c);
            }

			//collapse all of the list items into one big string
			var listItemLinksString = listItemLinks.Aggregate(
				new StringBuilder(),
				(sb, listItem) => sb.Append(listItem.ToString()),
				sb=> sb.ToString()
				);

			var ul = new TagBuilder("ul")
						{
							InnerHtml = listItemLinksString
						};
			foreach (var c in options.UlElementClasses ?? Enumerable.Empty<string>())
				ul.AddCssClass(c);

			var outerDiv = new TagBuilder("div");
			foreach(var c in options.ContainerDivClasses ?? Enumerable.Empty<string>())
				outerDiv.AddCssClass(c);
			outerDiv.InnerHtml = ul.ToString();

			return new MvcHtmlString(outerDiv.ToString());
		}
		
	}
}