using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace google_dialog.Extensions
{
    public static class XElementExtension
    {
        public static TVChannel ToTVChannel(this XElement element)
        {
            return new TVChannel
            {
                PartitionKey = "fr-FR",
                RowKey = element.Attribute("id").Value,
                DisplayName = element.Descendants("display-name").First().Value,
                IconSrc = element.Descendants("icon").First().Attribute("src").Value
            };
        }

        public static TVProgram ToTVProgram(this XElement element)
        {
            var rating = element.Descendants("rating").FirstOrDefault();
            var starRating = element.Descendants("star-rating").FirstOrDefault();

            var result = new TVProgram
            {
                PartitionKey = element.Attribute("start").Value.ParseDate().ToString("yyyyMMdd"),

                Channel = element.Attribute("channel").Value,
                Start = element.Attribute("start").Value.ParseDate(),
                Stop = element.Attribute("stop").Value.ParseDate(),
                Title = element.Descendants("title").FirstOrDefault()?.Value,
                Description = element.Descendants("desc").FirstOrDefault()?.Value,
                Category = element.Descendants("category").FirstOrDefault()?.Value,
                IconSrc = element.Descendants("icon").FirstOrDefault()?.Attribute("src")?.Value,
                Rating = rating?.Descendants("value").FirstOrDefault()?.Value,
                RatingIconSrc = rating?.Descendants("icon").FirstOrDefault()?.Attribute("src").Value,
                StarRating = starRating?.Descendants("value").FirstOrDefault()?.Value?.Split("/")?.First(),
                LengthInMinutes = Int32.Parse(element.Descendants("length").FirstOrDefault().Value)
            };

            result.RowKey = BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(result.Title + result.Start + result.Stop)));

            return result;
        }
    }

}
