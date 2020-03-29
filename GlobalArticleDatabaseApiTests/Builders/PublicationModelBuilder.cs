using GlobalArticleDatabaseAPI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GlobalArticleDatabaseApiTests.Builders
{
    public class PublicationModelBuilder
    {
        private readonly Publication _publication;

        public PublicationModelBuilder()
        {
            _publication = new Publication();
        }

        public PublicationModelBuilder WithRandomValues()
        {
            _publication.Date = DateTime.UtcNow;
            _publication.Link = $"Link_{DateTime.Now.Ticks}";
            _publication.Publisher = $"Publisher_{DateTime.Now.Ticks}";

            return this;
        }

        public Publication Build() => _publication;
    }
}
