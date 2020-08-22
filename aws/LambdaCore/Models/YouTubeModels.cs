using System;
using System.Collections.Generic;

namespace LambdaCore.Models
{
    public class GetItemsResponse
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public string nextPageToken { get; set; }
        public string regionCode { get; set; }
        public PageInfo pageInfo { get; set; }
        public List<Item> items { get; set; }
    }

    public class PageInfo
    {
        public int totalResults { get; set; }
        public int resultsPerPage { get; set; }
    }

    public class Item
    {
        public string kind { get; set; }
        public string etag { get; set; }
        public dynamic id { get; set; }
        public Snippet snippet { get; set; }
    }

    public class ItemId
    {
        public string kind { get; set; }
        public string videoId { get; set; }
    }

    public class Snippet
    {
        public DateTime publishedAt { get; set; }

        public string channelId { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public Thumbnails thumbnails { get; set; }
        public string channelTitle { get; set; }
        public string liveBroadcastContent { get; set; }
        public DateTime Date { get; set; }
        public string playlistId { get; set; }
        public int position { get; set; }
        public ResourceId resourceId { get; set; }
    }

    public class ResourceId
    {
        public string videoId { get; set; }
    }

    public class Thumbnails
    {
        public Thumbnail @default { get; set; }
        public Thumbnail medium { get; set; }
        public Thumbnail high { get; set; }
    }

    public class Thumbnail
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }
}
